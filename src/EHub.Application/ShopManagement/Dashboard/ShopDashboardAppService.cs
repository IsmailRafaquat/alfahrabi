using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.BankAccounts;
using EHub.ShopManagement.CashRegisters;
using EHub.ShopManagement.Customers;
using EHub.ShopManagement.CustomerPayments;
using EHub.ShopManagement.ExpenseCategories;
using EHub.ShopManagement.Expenses;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.ProductBatches;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.PurchaseReturns;
using EHub.ShopManagement.SaleReturns;
using EHub.ShopManagement.Sales;
using EHub.ShopManagement.Settings;
using EHub.ShopManagement.Suppliers;
using EHub.ShopManagement.SupplierPayments;
using EHub.ShopManagement.Units;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.Dashboard;

/// <summary>
/// Read-only aggregation over existing Shop Management entities. Never creates, updates, posts,
/// deletes, or cancels anything - all figures are computed with SQL Sum/Count/GroupBy against
/// posted/completed records only, reusing the same status enums and accounting formulas already
/// established by the Sale/Purchase/Expense/Ledger/Cash/Bank modules.
/// </summary>
[Authorize(EHubPermissions.ShopDashboard.Default)]
public class ShopDashboardAppService : ApplicationService, IShopDashboardAppService
{
    private readonly IRepository<ShopSale, Guid> _saleRepository;
    private readonly IRepository<ShopSaleItem, Guid> _saleItemRepository;
    private readonly IRepository<ShopSaleReturn, Guid> _saleReturnRepository;
    private readonly IRepository<ShopSaleReturnItem, Guid> _saleReturnItemRepository;
    private readonly IRepository<ShopGoodsReceipt, Guid> _goodsReceiptRepository;
    private readonly IRepository<ShopPurchaseReturn, Guid> _purchaseReturnRepository;
    private readonly IRepository<ShopExpense, Guid> _expenseRepository;
    private readonly IRepository<ShopExpenseCategory, Guid> _expenseCategoryRepository;
    private readonly IRepository<ShopCustomer, Guid> _customerRepository;
    private readonly IRepository<ShopCustomerPayment, Guid> _customerPaymentRepository;
    private readonly IRepository<ShopSupplier, Guid> _supplierRepository;
    private readonly IRepository<ShopSupplierPayment, Guid> _supplierPaymentRepository;
    private readonly ShopSupplierPaymentManager _supplierPaymentManager;
    private readonly IRepository<ShopCashRegisterTransaction, Guid> _cashTransactionRepository;
    private readonly IRepository<ShopBankAccount, Guid> _bankAccountRepository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly IRepository<ShopProductBatch, Guid> _productBatchRepository;
    private readonly IRepository<ShopSetting, Guid> _settingRepository;

    public ShopDashboardAppService(
        IRepository<ShopSale, Guid> saleRepository,
        IRepository<ShopSaleItem, Guid> saleItemRepository,
        IRepository<ShopSaleReturn, Guid> saleReturnRepository,
        IRepository<ShopSaleReturnItem, Guid> saleReturnItemRepository,
        IRepository<ShopGoodsReceipt, Guid> goodsReceiptRepository,
        IRepository<ShopPurchaseReturn, Guid> purchaseReturnRepository,
        IRepository<ShopExpense, Guid> expenseRepository,
        IRepository<ShopExpenseCategory, Guid> expenseCategoryRepository,
        IRepository<ShopCustomer, Guid> customerRepository,
        IRepository<ShopCustomerPayment, Guid> customerPaymentRepository,
        IRepository<ShopSupplier, Guid> supplierRepository,
        IRepository<ShopSupplierPayment, Guid> supplierPaymentRepository,
        ShopSupplierPaymentManager supplierPaymentManager,
        IRepository<ShopCashRegisterTransaction, Guid> cashTransactionRepository,
        IRepository<ShopBankAccount, Guid> bankAccountRepository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        IRepository<ShopProductBatch, Guid> productBatchRepository,
        IRepository<ShopSetting, Guid> settingRepository)
    {
        _saleRepository = saleRepository;
        _saleItemRepository = saleItemRepository;
        _saleReturnRepository = saleReturnRepository;
        _saleReturnItemRepository = saleReturnItemRepository;
        _goodsReceiptRepository = goodsReceiptRepository;
        _purchaseReturnRepository = purchaseReturnRepository;
        _expenseRepository = expenseRepository;
        _expenseCategoryRepository = expenseCategoryRepository;
        _customerRepository = customerRepository;
        _customerPaymentRepository = customerPaymentRepository;
        _supplierRepository = supplierRepository;
        _supplierPaymentRepository = supplierPaymentRepository;
        _supplierPaymentManager = supplierPaymentManager;
        _cashTransactionRepository = cashTransactionRepository;
        _bankAccountRepository = bankAccountRepository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
        _productBatchRepository = productBatchRepository;
        _settingRepository = settingRepository;
    }

    public async Task<ShopDashboardDto> GetAsync(GetShopDashboardInput input)
    {
        var tenantId = RequireTenant();
        var (from, to) = ResolvePeriod(input);
        var toExclusive = to.AddDays(1);
        var today = Clock.Now.Date;

        var setting = await GetSettingAsync(tenantId);

        var dto = new ShopDashboardDto
        {
            Period = input.Period,
            DateFrom = from,
            DateTo = to,
            ShopDisplayName = setting?.ShopDisplayName ?? string.Empty,
            CurrencyCode = setting?.CurrencyCode ?? "PKR",
            CurrencySymbol = setting?.CurrencySymbol ?? string.Empty,
            DecimalPlaces = setting?.DecimalPlaces ?? 2,
        };

        var canViewFinancial = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopDashboard.ViewFinancialSummary);
        var canViewBalances = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopDashboard.ViewBalances);
        var canViewInventoryValue = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopDashboard.ViewInventoryValue);
        var canViewStockAlerts = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopDashboard.ViewStockAlerts);
        var canViewSalesChart = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopDashboard.ViewSalesChart);
        var canViewTopProducts = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopDashboard.ViewTopProducts);
        var canViewRecent = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopDashboard.ViewRecentTransactions);

        var summary = new ShopDashboardSummaryDto();

        if (canViewFinancial)
        {
            var (grossSales, salesCount) = await ComputeSalesAsync(tenantId, from, toExclusive);
            var saleReturns = await ComputeSaleReturnsAsync(tenantId, from, toExclusive);
            var (totalPurchases, purchasesCount) = await ComputePurchasesAsync(tenantId, from, toExclusive);
            var purchaseReturns = await ComputePurchaseReturnsAsync(tenantId, from, toExclusive);
            var (totalExpenses, expensesCount) = await ComputeExpensesAsync(tenantId, from, toExclusive);

            summary.TotalSales = grossSales;
            summary.TotalSalesCount = salesCount;
            summary.GrossSales = grossSales;
            summary.SaleReturns = saleReturns;
            summary.NetSales = Math.Round(grossSales - saleReturns, 2);
            summary.TotalPurchases = totalPurchases;
            summary.TotalPurchasesCount = purchasesCount;
            summary.PurchaseReturns = purchaseReturns;
            summary.NetPurchases = Math.Round(totalPurchases - purchaseReturns, 2);
            summary.TotalExpenses = totalExpenses;
            summary.TotalExpensesCount = expensesCount;
        }

        if (canViewBalances)
        {
            var balances = await ComputeBalancesAsync(tenantId);
            summary.CustomerReceivables = balances.CustomerReceivables;
            summary.SupplierPayables = balances.SupplierPayables;
            summary.CashBalance = balances.CashBalance;
            summary.BankBalance = balances.BankBalance;
            summary.TotalAvailableBalance = Math.Round(balances.CashBalance + balances.BankBalance, 2);
            dto.Balances = balances;
        }

        var (inventoryQuantity, inventoryValue) = await ComputeInventoryAsync(tenantId, canViewInventoryValue);
        summary.InventoryQuantity = inventoryQuantity;
        summary.InventoryValue = canViewInventoryValue ? inventoryValue : null;

        if (canViewStockAlerts)
        {
            var (lowStockCount, outOfStockCount) = await ComputeStockAlertCountsAsync(tenantId);
            var (nearExpiryCount, expiredCount) = await ComputeBatchAlertCountsAsync(tenantId, today);
            summary.LowStockProductCount = lowStockCount;
            summary.OutOfStockProductCount = outOfStockCount;
            summary.NearExpiryBatchCount = nearExpiryCount;
            summary.ExpiredBatchCount = expiredCount;

            dto.LowStockProducts = await ComputeLowStockProductsAsync(tenantId, 10);
            dto.NearExpiryBatches = await ComputeNearExpiryBatchesAsync(tenantId, today, 10);
            dto.ExpiredBatches = await ComputeExpiredBatchesAsync(tenantId, today, 10);
        }

        if (canViewStockAlerts || canViewInventoryValue)
        {
            dto.Inventory = new ShopDashboardInventorySummaryDto
            {
                InventoryQuantity = inventoryQuantity,
                InventoryValue = canViewInventoryValue ? inventoryValue : null,
                LowStockProductCount = summary.LowStockProductCount ?? 0,
                OutOfStockProductCount = summary.OutOfStockProductCount ?? 0,
                NearExpiryBatchCount = summary.NearExpiryBatchCount ?? 0,
                ExpiredBatchCount = summary.ExpiredBatchCount ?? 0,
            };
        }

        summary.ActiveCustomerCount = await ComputeActiveCustomerCountAsync(tenantId);
        summary.ActiveSupplierCount = await ComputeActiveSupplierCountAsync(tenantId);

        dto.Summary = summary;

        if (canViewSalesChart)
        {
            dto.SalesExpenseChart = await ComputeSalesExpenseChartAsync(tenantId, input.Period, from, to);
            var (prevFrom, prevTo) = ResolvePreviousPeriod(input.Period, from, to);
            dto.SalesTrend = await ComputeSalesTrendAsync(tenantId, from, toExclusive, prevFrom, prevTo.AddDays(1));
        }

        if (canViewTopProducts)
        {
            dto.TopProducts = await ComputeTopProductsAsync(tenantId, from, toExclusive, input.TopProductsCount);
        }

        if (canViewRecent)
        {
            dto.RecentSales = await ComputeRecentSalesAsync(tenantId, input.RecentItemsCount);
            dto.RecentPurchases = await ComputeRecentPurchasesAsync(tenantId, input.RecentItemsCount);
            dto.RecentExpenses = await ComputeRecentExpensesAsync(tenantId, input.RecentItemsCount);
        }

        return dto;
    }

    public async Task<ShopDashboardSummaryDto> GetSummaryAsync(GetShopDashboardInput input)
    {
        // GetAsync already builds the full summary as its first step; reuse it directly instead
        // of duplicating the aggregation logic, at the cost of also touching the other sections
        // this endpoint doesn't strictly need - all sections stay cheap COUNT/SUM queries.
        var full = await GetAsync(input);
        return full.Summary;
    }

    [Authorize(EHubPermissions.ShopDashboard.ViewSalesChart)]
    public async Task<ShopDashboardSalesExpenseChartDto> GetSalesExpenseChartAsync(GetShopDashboardInput input)
    {
        var tenantId = RequireTenant();
        var (from, to) = ResolvePeriod(input);
        return await ComputeSalesExpenseChartAsync(tenantId, input.Period, from, to);
    }

    [Authorize(EHubPermissions.ShopDashboard.ViewTopProducts)]
    public async Task<ListResultDto<ShopDashboardTopProductDto>> GetTopProductsAsync(GetShopDashboardInput input)
    {
        var tenantId = RequireTenant();
        var (from, to) = ResolvePeriod(input);
        var items = await ComputeTopProductsAsync(tenantId, from, to.AddDays(1), input.TopProductsCount);
        return new ListResultDto<ShopDashboardTopProductDto>(items);
    }

    [Authorize(EHubPermissions.ShopDashboard.ViewStockAlerts)]
    public async Task<ListResultDto<ShopDashboardLowStockProductDto>> GetLowStockProductsAsync()
    {
        var tenantId = RequireTenant();
        var items = await ComputeLowStockProductsAsync(tenantId, int.MaxValue);
        return new ListResultDto<ShopDashboardLowStockProductDto>(items);
    }

    [Authorize(EHubPermissions.ShopDashboard.ViewStockAlerts)]
    public async Task<ListResultDto<ShopDashboardBatchAlertDto>> GetNearExpiryBatchesAsync(int maxResultCount = 10)
    {
        var tenantId = RequireTenant();
        var items = await ComputeNearExpiryBatchesAsync(tenantId, Clock.Now.Date, maxResultCount);
        return new ListResultDto<ShopDashboardBatchAlertDto>(items);
    }

    [Authorize(EHubPermissions.ShopDashboard.ViewStockAlerts)]
    public async Task<ListResultDto<ShopDashboardBatchAlertDto>> GetExpiredBatchesAsync(int maxResultCount = 10)
    {
        var tenantId = RequireTenant();
        var items = await ComputeExpiredBatchesAsync(tenantId, Clock.Now.Date, maxResultCount);
        return new ListResultDto<ShopDashboardBatchAlertDto>(items);
    }

    [Authorize(EHubPermissions.ShopDashboard.ViewBalances)]
    public async Task<ShopDashboardBalanceSummaryDto> GetBalancesAsync()
    {
        var tenantId = RequireTenant();
        return await ComputeBalancesAsync(tenantId);
    }

    [Authorize(EHubPermissions.ShopDashboard.ViewRecentTransactions)]
    public async Task<ListResultDto<ShopDashboardRecentSaleDto>> GetRecentSalesAsync(int maxResultCount = 5)
    {
        var tenantId = RequireTenant();
        var items = await ComputeRecentSalesAsync(tenantId, maxResultCount);
        return new ListResultDto<ShopDashboardRecentSaleDto>(items);
    }

    [Authorize(EHubPermissions.ShopDashboard.ViewRecentTransactions)]
    public async Task<ListResultDto<ShopDashboardRecentPurchaseDto>> GetRecentPurchasesAsync(int maxResultCount = 5)
    {
        var tenantId = RequireTenant();
        var items = await ComputeRecentPurchasesAsync(tenantId, maxResultCount);
        return new ListResultDto<ShopDashboardRecentPurchaseDto>(items);
    }

    [Authorize(EHubPermissions.ShopDashboard.ViewRecentTransactions)]
    public async Task<ListResultDto<ShopDashboardRecentExpenseDto>> GetRecentExpensesAsync(int maxResultCount = 5)
    {
        var tenantId = RequireTenant();
        var items = await ComputeRecentExpensesAsync(tenantId, maxResultCount);
        return new ListResultDto<ShopDashboardRecentExpenseDto>(items);
    }

    // ------------------------------------------------------------------
    // Period resolution
    // ------------------------------------------------------------------

    private (DateTime From, DateTime To) ResolvePeriod(GetShopDashboardInput input)
    {
        var today = Clock.Now.Date;

        switch (input.Period)
        {
            case ShopDashboardPeriod.Today:
                return (today, today);
            case ShopDashboardPeriod.Yesterday:
                var yesterday = today.AddDays(-1);
                return (yesterday, yesterday);
            case ShopDashboardPeriod.Last7Days:
                return (today.AddDays(-6), today);
            case ShopDashboardPeriod.Last30Days:
                return (today.AddDays(-29), today);
            case ShopDashboardPeriod.ThisMonth:
                return (new DateTime(today.Year, today.Month, 1), today);
            case ShopDashboardPeriod.LastMonth:
                var firstOfThisMonth = new DateTime(today.Year, today.Month, 1);
                var firstOfLastMonth = firstOfThisMonth.AddMonths(-1);
                return (firstOfLastMonth, firstOfThisMonth.AddDays(-1));
            case ShopDashboardPeriod.ThisYear:
                return (new DateTime(today.Year, 1, 1), today);
            case ShopDashboardPeriod.Custom:
                if (!input.DateFrom.HasValue || !input.DateTo.HasValue)
                    throw new BusinessException("ShopManagement:DashboardDateRangeRequired");
                var customFrom = input.DateFrom.Value.Date;
                var customTo = input.DateTo.Value.Date;
                if (customFrom > customTo) throw new BusinessException("ShopManagement:DashboardDateRangeInvalid");
                return (customFrom, customTo);
            default:
                throw new BusinessException("ShopManagement:DashboardPeriodInvalid");
        }
    }

    private static (DateTime From, DateTime To) ResolvePreviousPeriod(ShopDashboardPeriod period, DateTime from, DateTime to)
    {
        switch (period)
        {
            case ShopDashboardPeriod.ThisMonth:
            case ShopDashboardPeriod.LastMonth:
                var firstOfPeriodMonth = new DateTime(from.Year, from.Month, 1);
                var firstOfPrevMonth = firstOfPeriodMonth.AddMonths(-1);
                return (firstOfPrevMonth, firstOfPeriodMonth.AddDays(-1));
            case ShopDashboardPeriod.ThisYear:
                return (new DateTime(from.Year - 1, 1, 1), new DateTime(from.Year - 1, 12, 31));
            default:
                var dayCount = (to - from).Days + 1;
                var previousTo = from.AddDays(-1);
                var previousFrom = previousTo.AddDays(-(dayCount - 1));
                return (previousFrom, previousTo);
        }
    }

    // ------------------------------------------------------------------
    // Financial aggregates
    // ------------------------------------------------------------------

    private async Task<(decimal Total, int Count)> ComputeSalesAsync(Guid tenantId, DateTime from, DateTime toExclusive)
    {
        var query = (await _saleRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == ShopSaleStatus.Completed && x.SaleDate >= from && x.SaleDate < toExclusive);

        var total = await AsyncExecuter.SumAsync(query.Select(x => (decimal?)x.GrandTotal)) ?? 0m;
        var count = await AsyncExecuter.CountAsync(query);
        return (Math.Round(total, 2), count);
    }

    private async Task<decimal> ComputeSaleReturnsAsync(Guid tenantId, DateTime from, DateTime toExclusive)
    {
        var query = (await _saleReturnRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == ShopSaleReturnStatus.Completed && x.ReturnDate >= from && x.ReturnDate < toExclusive);

        var total = await AsyncExecuter.SumAsync(query.Select(x => (decimal?)x.GrandTotal)) ?? 0m;
        return Math.Round(total, 2);
    }

    private async Task<(decimal Total, int Count)> ComputePurchasesAsync(Guid tenantId, DateTime from, DateTime toExclusive)
    {
        var query = (await _goodsReceiptRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == ShopGoodsReceiptStatus.Completed && x.ReceiptDate >= from && x.ReceiptDate < toExclusive);

        var total = await AsyncExecuter.SumAsync(query.Select(x => (decimal?)x.GrandTotal)) ?? 0m;
        var count = await AsyncExecuter.CountAsync(query);
        return (Math.Round(total, 2), count);
    }

    private async Task<decimal> ComputePurchaseReturnsAsync(Guid tenantId, DateTime from, DateTime toExclusive)
    {
        var query = (await _purchaseReturnRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == ShopPurchaseReturnStatus.Completed && x.ReturnDate >= from && x.ReturnDate < toExclusive);

        var total = await AsyncExecuter.SumAsync(query.Select(x => (decimal?)x.GrandTotal)) ?? 0m;
        return Math.Round(total, 2);
    }

    private async Task<(decimal Total, int Count)> ComputeExpensesAsync(Guid tenantId, DateTime from, DateTime toExclusive)
    {
        var query = (await _expenseRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == ShopExpenseStatus.Posted && x.ExpenseDate >= from && x.ExpenseDate < toExclusive);

        var total = await AsyncExecuter.SumAsync(query.Select(x => (decimal?)x.Amount)) ?? 0m;
        var count = await AsyncExecuter.CountAsync(query);
        return (Math.Round(total, 2), count);
    }

    private async Task<decimal> ComputeNetSalesAsync(Guid tenantId, DateTime from, DateTime toExclusive)
    {
        var (gross, _) = await ComputeSalesAsync(tenantId, from, toExclusive);
        var returns = await ComputeSaleReturnsAsync(tenantId, from, toExclusive);
        return Math.Round(gross - returns, 2);
    }

    private async Task<int> ComputeActiveCustomerCountAsync(Guid tenantId)
    {
        var query = (await _customerRepository.GetQueryableAsync()).AsNoTracking();
        return await AsyncExecuter.CountAsync(query.Where(x => x.TenantId == tenantId && x.IsActive));
    }

    private async Task<int> ComputeActiveSupplierCountAsync(Guid tenantId)
    {
        var query = (await _supplierRepository.GetQueryableAsync()).AsNoTracking();
        return await AsyncExecuter.CountAsync(query.Where(x => x.TenantId == tenantId && x.IsActive));
    }

    // ------------------------------------------------------------------
    // Balances (all-time, not date-filtered - current outstanding position)
    // ------------------------------------------------------------------

    private async Task<ShopDashboardBalanceSummaryDto> ComputeBalancesAsync(Guid tenantId)
    {
        var (receivables, customerAdvances) = await ComputeCustomerBalancesAsync(tenantId);
        var (payables, supplierAdvances) = await ComputeSupplierBalancesAsync(tenantId);
        var cashBalance = await ComputeCashBalanceAsync(tenantId);
        var bankBalance = await ComputeBankBalanceAsync(tenantId);

        return new ShopDashboardBalanceSummaryDto
        {
            CashBalance = cashBalance,
            BankBalance = bankBalance,
            CustomerReceivables = receivables,
            SupplierPayables = payables,
            CustomerAdvanceBalance = customerAdvances,
            SupplierAdvanceBalance = supplierAdvances,
        };
    }

    /// <summary>
    /// Mirrors ShopCustomerLedgerAppService.GetBalanceSummaryAsync's formula (OpeningBalance + Sale
    /// debits [floored at zero, i.e. GrandTotal-PaidAmount when positive] - CustomerPayment credits -
    /// SaleReturn credits) but computed as three grouped SQL aggregates across ALL customers at once,
    /// instead of one full ledger reconstruction per customer.
    /// </summary>
    private async Task<(decimal Receivables, decimal Advances)> ComputeCustomerBalancesAsync(Guid tenantId)
    {
        var saleQuery = (await _saleRepository.GetQueryableAsync()).AsNoTracking();
        var saleDebits = await AsyncExecuter.ToListAsync(
            saleQuery.Where(x => x.TenantId == tenantId && x.Status == ShopSaleStatus.Completed)
                .Select(x => new { x.CustomerId, Debit = (x.GrandTotal - x.PaidAmount) > 0 ? (x.GrandTotal - x.PaidAmount) : 0 })
                .GroupBy(x => x.CustomerId)
                .Select(g => new { CustomerId = g.Key, Debit = g.Sum(x => x.Debit) }));

        var paymentQuery = (await _customerPaymentRepository.GetQueryableAsync()).AsNoTracking();
        var paymentCredits = await AsyncExecuter.ToListAsync(
            paymentQuery.Where(x => x.TenantId == tenantId && x.Status == ShopCustomerPaymentStatus.Posted)
                .GroupBy(x => x.CustomerId)
                .Select(g => new { CustomerId = g.Key, Credit = g.Sum(x => x.Amount) }));

        var returnQuery = (await _saleReturnRepository.GetQueryableAsync()).AsNoTracking();
        var returnCredits = await AsyncExecuter.ToListAsync(
            returnQuery.Where(x => x.TenantId == tenantId && x.Status == ShopSaleReturnStatus.Completed)
                .GroupBy(x => x.CustomerId)
                .Select(g => new { CustomerId = g.Key, Credit = g.Sum(x => x.GrandTotal) }));

        var customerQuery = (await _customerRepository.GetQueryableAsync()).AsNoTracking();
        var customers = await AsyncExecuter.ToListAsync(
            customerQuery.Where(x => x.TenantId == tenantId).Select(x => new { x.Id, x.OpeningBalance }));

        var debitDict = saleDebits.ToDictionary(x => x.CustomerId, x => x.Debit);
        var creditDict = new Dictionary<Guid, decimal>();
        foreach (var row in paymentCredits) creditDict[row.CustomerId] = creditDict.GetValueOrDefault(row.CustomerId) + row.Credit;
        foreach (var row in returnCredits) creditDict[row.CustomerId] = creditDict.GetValueOrDefault(row.CustomerId) + row.Credit;

        decimal receivables = 0, advances = 0;
        foreach (var customer in customers)
        {
            var balance = customer.OpeningBalance + debitDict.GetValueOrDefault(customer.Id) - creditDict.GetValueOrDefault(customer.Id);
            if (balance > 0) receivables += balance; else advances += -balance;
        }

        return (Math.Round(receivables, 2), Math.Round(advances, 2));
    }

    /// <summary>Same approach as ComputeCustomerBalancesAsync, mirroring ShopSupplierLedgerAppService.</summary>
    private async Task<(decimal Payables, decimal Advances)> ComputeSupplierBalancesAsync(Guid tenantId)
    {
        var grQuery = (await _goodsReceiptRepository.GetQueryableAsync()).AsNoTracking();
        var grDebits = await AsyncExecuter.ToListAsync(
            grQuery.Where(x => x.TenantId == tenantId && x.Status == ShopGoodsReceiptStatus.Completed)
                .GroupBy(x => x.SupplierId)
                .Select(g => new { SupplierId = g.Key, Debit = g.Sum(x => x.GrandTotal) }));

        var paymentQuery = (await _supplierPaymentRepository.GetQueryableAsync()).AsNoTracking();
        var paymentCredits = await AsyncExecuter.ToListAsync(
            paymentQuery.Where(x => x.TenantId == tenantId && x.Status == ShopSupplierPaymentStatus.Posted)
                .GroupBy(x => x.SupplierId)
                .Select(g => new { SupplierId = g.Key, Credit = g.Sum(x => x.Amount) }));

        var returnQuery = (await _purchaseReturnRepository.GetQueryableAsync()).AsNoTracking();
        var returnCredits = await AsyncExecuter.ToListAsync(
            returnQuery.Where(x => x.TenantId == tenantId && x.Status == ShopPurchaseReturnStatus.Completed)
                .GroupBy(x => x.SupplierId)
                .Select(g => new { SupplierId = g.Key, Credit = g.Sum(x => x.GrandTotal) }));

        var supplierQuery = (await _supplierRepository.GetQueryableAsync()).AsNoTracking();
        var suppliers = await AsyncExecuter.ToListAsync(
            supplierQuery.Where(x => x.TenantId == tenantId).Select(x => new { x.Id, x.OpeningBalance }));

        var debitDict = grDebits.ToDictionary(x => x.SupplierId, x => x.Debit);
        var creditDict = new Dictionary<Guid, decimal>();
        foreach (var row in paymentCredits) creditDict[row.SupplierId] = creditDict.GetValueOrDefault(row.SupplierId) + row.Credit;
        foreach (var row in returnCredits) creditDict[row.SupplierId] = creditDict.GetValueOrDefault(row.SupplierId) + row.Credit;

        decimal payables = 0, advances = 0;
        foreach (var supplier in suppliers)
        {
            var balance = supplier.OpeningBalance + debitDict.GetValueOrDefault(supplier.Id) - creditDict.GetValueOrDefault(supplier.Id);
            if (balance > 0) payables += balance; else advances += -balance;
        }

        return (Math.Round(payables, 2), Math.Round(advances, 2));
    }

    private async Task<decimal> ComputeCashBalanceAsync(Guid tenantId)
    {
        var query = (await _cashTransactionRepository.GetQueryableAsync()).AsNoTracking();
        var balance = await AsyncExecuter.SumAsync(
            query.Where(x => x.TenantId == tenantId)
                .Select(x => x.Direction == ShopCashDirection.In ? (decimal?)x.Amount : -(decimal?)x.Amount)) ?? 0m;
        return Math.Round(balance, 2);
    }

    private async Task<decimal> ComputeBankBalanceAsync(Guid tenantId)
    {
        var query = (await _bankAccountRepository.GetQueryableAsync()).AsNoTracking();
        var balance = await AsyncExecuter.SumAsync(
            query.Where(x => x.TenantId == tenantId && x.IsActive).Select(x => (decimal?)x.CurrentBalance)) ?? 0m;
        return Math.Round(balance, 2);
    }

    // ------------------------------------------------------------------
    // Inventory
    // ------------------------------------------------------------------

    private async Task<(decimal Quantity, decimal Value)> ComputeInventoryAsync(Guid tenantId, bool includeValue)
    {
        var productQuery = (await _productRepository.GetQueryableAsync()).AsNoTracking();
        var activeProducts = productQuery.Where(x => x.TenantId == tenantId && x.IsActive);

        var quantity = await AsyncExecuter.SumAsync(activeProducts.Select(x => (decimal?)x.CurrentStock)) ?? 0m;
        if (!includeValue) return (quantity, 0m);

        var nonBatchValue = await AsyncExecuter.SumAsync(
            activeProducts.Where(x => !x.TrackBatch).Select(x => (decimal?)(x.CurrentStock * x.PurchasePrice))) ?? 0m;

        var batchQuery = (await _productBatchRepository.GetQueryableAsync()).AsNoTracking();
        var batchValue = await AsyncExecuter.SumAsync(
            from b in batchQuery
            join p in productQuery on b.ProductId equals p.Id
            where b.TenantId == tenantId && p.TenantId == tenantId && p.IsActive && p.TrackBatch
            select (decimal?)(b.AvailableQuantity * b.UnitCost)) ?? 0m;

        return (quantity, Math.Round(nonBatchValue + batchValue, 2));
    }

    private async Task<(int LowStockCount, int OutOfStockCount)> ComputeStockAlertCountsAsync(Guid tenantId)
    {
        var query = (await _productRepository.GetQueryableAsync()).AsNoTracking();
        var candidates = query.Where(x => x.TenantId == tenantId && x.IsActive && x.CurrentStock <= x.ReorderLevel);

        var lowStockCount = await AsyncExecuter.CountAsync(candidates.Where(x => x.CurrentStock > 0));
        var outOfStockCount = await AsyncExecuter.CountAsync(candidates.Where(x => x.CurrentStock <= 0));
        return (lowStockCount, outOfStockCount);
    }

    private async Task<List<ShopDashboardLowStockProductDto>> ComputeLowStockProductsAsync(Guid tenantId, int maxResultCount)
    {
        var productQuery = (await _productRepository.GetQueryableAsync()).AsNoTracking();
        var unitQuery = (await _unitRepository.GetQueryableAsync()).AsNoTracking();

        var candidates = await AsyncExecuter.ToListAsync(
            from p in productQuery
            join u in unitQuery on p.UnitId equals u.Id
            where p.TenantId == tenantId && p.IsActive && p.CurrentStock <= p.ReorderLevel
            select new { p.Id, p.Code, p.Name, UnitName = u.Name, p.CurrentStock, p.ReorderLevel });

        return candidates
            .Select(x => new ShopDashboardLowStockProductDto
            {
                ProductId = x.Id,
                ProductCode = x.Code,
                ProductName = x.Name,
                UnitName = x.UnitName,
                CurrentStock = x.CurrentStock,
                ReorderLevel = x.ReorderLevel,
                RequiredReorderQuantity = Math.Max(x.ReorderLevel - x.CurrentStock, 0),
                IsOutOfStock = x.CurrentStock <= 0,
            })
            .OrderByDescending(x => x.IsOutOfStock)
            .ThenBy(x => x.ReorderLevel > 0 ? x.CurrentStock / x.ReorderLevel : 0)
            .ThenBy(x => x.ProductName)
            .Take(maxResultCount)
            .ToList();
    }

    private async Task<(int NearExpiryCount, int ExpiredCount)> ComputeBatchAlertCountsAsync(Guid tenantId, DateTime today)
    {
        var batchQuery = (await _productBatchRepository.GetQueryableAsync()).AsNoTracking();
        var productQuery = (await _productRepository.GetQueryableAsync()).AsNoTracking();

        var nearExpiryCount = await AsyncExecuter.CountAsync(
            from b in batchQuery
            join p in productQuery on b.ProductId equals p.Id
            where b.TenantId == tenantId && b.AvailableQuantity > 0 && b.ExpiryDate != null
                  && b.ExpiryDate.Value.Date >= today
                  && b.ExpiryDate.Value.Date <= today.AddDays(p.ExpiryAlertDays ?? 30)
            select b.Id);

        var expiredCount = await AsyncExecuter.CountAsync(
            batchQuery.Where(b => b.TenantId == tenantId && b.AvailableQuantity > 0 && b.ExpiryDate != null && b.ExpiryDate.Value.Date < today));

        return (nearExpiryCount, expiredCount);
    }

    private async Task<List<ShopDashboardBatchAlertDto>> ComputeNearExpiryBatchesAsync(Guid tenantId, DateTime today, int maxResultCount)
    {
        var batchQuery = (await _productBatchRepository.GetQueryableAsync()).AsNoTracking();
        var productQuery = (await _productRepository.GetQueryableAsync()).AsNoTracking();

        var rows = await AsyncExecuter.ToListAsync(
            (from b in batchQuery
             join p in productQuery on b.ProductId equals p.Id
             where b.TenantId == tenantId && b.AvailableQuantity > 0 && b.ExpiryDate != null
                   && b.ExpiryDate.Value.Date >= today
                   && b.ExpiryDate.Value.Date <= today.AddDays(p.ExpiryAlertDays ?? 30)
             orderby b.ExpiryDate
             select new { b.Id, b.ProductId, ProductCode = p.Code, ProductName = p.Name, b.BatchNumber, b.ExpiryDate, b.AvailableQuantity, b.Status })
            .Take(maxResultCount));

        return rows.Select(x => new ShopDashboardBatchAlertDto
        {
            ProductId = x.ProductId,
            ProductCode = x.ProductCode,
            ProductName = x.ProductName,
            ProductBatchId = x.Id,
            BatchNumber = x.BatchNumber ?? string.Empty,
            ExpiryDate = x.ExpiryDate,
            DaysToExpiry = x.ExpiryDate.HasValue ? (int)(x.ExpiryDate.Value.Date - today).TotalDays : null,
            AvailableQuantity = x.AvailableQuantity,
            Status = x.Status,
        }).ToList();
    }

    private async Task<List<ShopDashboardBatchAlertDto>> ComputeExpiredBatchesAsync(Guid tenantId, DateTime today, int maxResultCount)
    {
        var batchQuery = (await _productBatchRepository.GetQueryableAsync()).AsNoTracking();
        var productQuery = (await _productRepository.GetQueryableAsync()).AsNoTracking();

        var rows = await AsyncExecuter.ToListAsync(
            (from b in batchQuery
             join p in productQuery on b.ProductId equals p.Id
             where b.TenantId == tenantId && b.AvailableQuantity > 0 && b.ExpiryDate != null && b.ExpiryDate.Value.Date < today
             orderby b.ExpiryDate
             select new { b.Id, b.ProductId, ProductCode = p.Code, ProductName = p.Name, b.BatchNumber, b.ExpiryDate, b.AvailableQuantity, b.Status })
            .Take(maxResultCount));

        return rows.Select(x => new ShopDashboardBatchAlertDto
        {
            ProductId = x.ProductId,
            ProductCode = x.ProductCode,
            ProductName = x.ProductName,
            ProductBatchId = x.Id,
            BatchNumber = x.BatchNumber ?? string.Empty,
            ExpiryDate = x.ExpiryDate,
            DaysToExpiry = x.ExpiryDate.HasValue ? (int)(x.ExpiryDate.Value.Date - today).TotalDays : null,
            AvailableQuantity = x.AvailableQuantity,
            Status = x.Status,
        }).ToList();
    }

    // ------------------------------------------------------------------
    // Sales vs Expenses chart + trend
    // ------------------------------------------------------------------

    private async Task<ShopDashboardSalesExpenseChartDto> ComputeSalesExpenseChartAsync(Guid tenantId, ShopDashboardPeriod period, DateTime from, DateTime to)
    {
        var toExclusive = to.AddDays(1);
        var groupByMonth = period == ShopDashboardPeriod.ThisYear
            || (period == ShopDashboardPeriod.Custom && (to - from).Days > 60);

        var saleQuery = (await _saleRepository.GetQueryableAsync()).AsNoTracking();
        var saleReturnQuery = (await _saleReturnRepository.GetQueryableAsync()).AsNoTracking();
        var expenseQuery = (await _expenseRepository.GetQueryableAsync()).AsNoTracking();

        if (groupByMonth)
        {
            var grossByMonth = await AsyncExecuter.ToListAsync(
                saleQuery.Where(x => x.TenantId == tenantId && x.Status == ShopSaleStatus.Completed && x.SaleDate >= from && x.SaleDate < toExclusive)
                    .GroupBy(x => new { x.SaleDate.Year, x.SaleDate.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.GrandTotal) }));

            var returnsByMonth = await AsyncExecuter.ToListAsync(
                saleReturnQuery.Where(x => x.TenantId == tenantId && x.Status == ShopSaleReturnStatus.Completed && x.ReturnDate >= from && x.ReturnDate < toExclusive)
                    .GroupBy(x => new { x.ReturnDate.Year, x.ReturnDate.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.GrandTotal) }));

            var expensesByMonth = await AsyncExecuter.ToListAsync(
                expenseQuery.Where(x => x.TenantId == tenantId && x.Status == ShopExpenseStatus.Posted && x.ExpenseDate >= from && x.ExpenseDate < toExclusive)
                    .GroupBy(x => new { x.ExpenseDate.Year, x.ExpenseDate.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Amount) }));

            var grossDict = grossByMonth.ToDictionary(x => (x.Year, x.Month), x => x.Total);
            var returnDict = returnsByMonth.ToDictionary(x => (x.Year, x.Month), x => x.Total);
            var expenseDict = expensesByMonth.ToDictionary(x => (x.Year, x.Month), x => x.Total);

            var salesPoints = new List<ShopDashboardChartPointDto>();
            var expensePoints = new List<ShopDashboardChartPointDto>();

            var cursor = new DateTime(from.Year, from.Month, 1);
            var end = new DateTime(to.Year, to.Month, 1);
            while (cursor <= end)
            {
                var key = (cursor.Year, cursor.Month);
                var gross = grossDict.GetValueOrDefault(key);
                var ret = returnDict.GetValueOrDefault(key);
                var expense = expenseDict.GetValueOrDefault(key);

                salesPoints.Add(new ShopDashboardChartPointDto { Label = cursor.ToString("MMM yyyy"), PeriodStart = cursor, Value = Math.Round(gross - ret, 2) });
                expensePoints.Add(new ShopDashboardChartPointDto { Label = cursor.ToString("MMM yyyy"), PeriodStart = cursor, Value = Math.Round(expense, 2) });

                cursor = cursor.AddMonths(1);
            }

            return new ShopDashboardSalesExpenseChartDto { GroupBy = "Month", NetSalesPoints = salesPoints, ExpensePoints = expensePoints };
        }
        else
        {
            var grossByDay = await AsyncExecuter.ToListAsync(
                saleQuery.Where(x => x.TenantId == tenantId && x.Status == ShopSaleStatus.Completed && x.SaleDate >= from && x.SaleDate < toExclusive)
                    .GroupBy(x => x.SaleDate.Date)
                    .Select(g => new { Date = g.Key, Total = g.Sum(x => x.GrandTotal) }));

            var returnsByDay = await AsyncExecuter.ToListAsync(
                saleReturnQuery.Where(x => x.TenantId == tenantId && x.Status == ShopSaleReturnStatus.Completed && x.ReturnDate >= from && x.ReturnDate < toExclusive)
                    .GroupBy(x => x.ReturnDate.Date)
                    .Select(g => new { Date = g.Key, Total = g.Sum(x => x.GrandTotal) }));

            var expensesByDay = await AsyncExecuter.ToListAsync(
                expenseQuery.Where(x => x.TenantId == tenantId && x.Status == ShopExpenseStatus.Posted && x.ExpenseDate >= from && x.ExpenseDate < toExclusive)
                    .GroupBy(x => x.ExpenseDate.Date)
                    .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Amount) }));

            var grossDict = grossByDay.ToDictionary(x => x.Date, x => x.Total);
            var returnDict = returnsByDay.ToDictionary(x => x.Date, x => x.Total);
            var expenseDict = expensesByDay.ToDictionary(x => x.Date, x => x.Total);

            var salesPoints = new List<ShopDashboardChartPointDto>();
            var expensePoints = new List<ShopDashboardChartPointDto>();

            for (var day = from; day <= to; day = day.AddDays(1))
            {
                var gross = grossDict.GetValueOrDefault(day);
                var ret = returnDict.GetValueOrDefault(day);
                var expense = expenseDict.GetValueOrDefault(day);

                salesPoints.Add(new ShopDashboardChartPointDto { Label = day.ToString("MMM dd"), PeriodStart = day, Value = Math.Round(gross - ret, 2) });
                expensePoints.Add(new ShopDashboardChartPointDto { Label = day.ToString("MMM dd"), PeriodStart = day, Value = Math.Round(expense, 2) });
            }

            return new ShopDashboardSalesExpenseChartDto { GroupBy = "Day", NetSalesPoints = salesPoints, ExpensePoints = expensePoints };
        }
    }

    private async Task<ShopDashboardSalesTrendDto> ComputeSalesTrendAsync(Guid tenantId, DateTime from, DateTime toExclusive, DateTime prevFrom, DateTime prevToExclusive)
    {
        var currentNet = await ComputeNetSalesAsync(tenantId, from, toExclusive);
        var previousNet = await ComputeNetSalesAsync(tenantId, prevFrom, prevToExclusive);

        var growthAmount = Math.Round(currentNet - previousNet, 2);
        decimal? growthPercentage = previousNet > 0 ? Math.Round(growthAmount / previousNet * 100, 2) : (decimal?)null;

        return new ShopDashboardSalesTrendDto
        {
            CurrentPeriodNetSales = currentNet,
            PreviousPeriodNetSales = previousNet,
            SalesGrowthAmount = growthAmount,
            SalesGrowthPercentage = growthPercentage,
        };
    }

    // ------------------------------------------------------------------
    // Top products
    // ------------------------------------------------------------------

    private async Task<List<ShopDashboardTopProductDto>> ComputeTopProductsAsync(Guid tenantId, DateTime periodFrom, DateTime toExclusive, int count)
    {
        var itemQuery = (await _saleItemRepository.GetQueryableAsync()).AsNoTracking();
        var saleQuery = (await _saleRepository.GetQueryableAsync()).AsNoTracking();

        // Grouped by ProductId + its snapshot fields together: if a product's name/code snapshot ever
        // changed between sales, it would appear as separate rows here rather than being merged - an
        // accepted trade-off for a dashboard widget (quantities still aggregate correctly per snapshot).
        var grossByProduct = await AsyncExecuter.ToListAsync(
            (from item in itemQuery
             join sale in saleQuery on item.SaleId equals sale.Id
             where sale.TenantId == tenantId && sale.Status == ShopSaleStatus.Completed
                   && sale.SaleDate >= periodFrom && sale.SaleDate < toExclusive
             group item by new { item.ProductId, item.ProductCodeSnapshot, item.ProductNameSnapshot, item.UnitNameSnapshot } into g
             select new
             {
                 g.Key.ProductId,
                 g.Key.ProductCodeSnapshot,
                 g.Key.ProductNameSnapshot,
                 g.Key.UnitNameSnapshot,
                 QuantitySold = g.Sum(x => x.Quantity),
                 GrossSalesAmount = g.Sum(x => x.LineTotal),
             }));

        if (grossByProduct.Count == 0) return new List<ShopDashboardTopProductDto>();

        var productIds = grossByProduct.Select(x => x.ProductId).Distinct().ToList();

        var returnItemQuery = (await _saleReturnItemRepository.GetQueryableAsync()).AsNoTracking();
        var saleReturnQuery = (await _saleReturnRepository.GetQueryableAsync()).AsNoTracking();

        var returnsByProduct = await AsyncExecuter.ToListAsync(
            from item in returnItemQuery
            join ret in saleReturnQuery on item.SaleReturnId equals ret.Id
            where ret.TenantId == tenantId && ret.Status == ShopSaleReturnStatus.Completed
                  && ret.ReturnDate >= periodFrom && ret.ReturnDate < toExclusive
                  && productIds.Contains(item.ProductId)
            group item by item.ProductId into g
            select new { ProductId = g.Key, ReturnQuantity = g.Sum(x => x.ReturnQuantity), ReturnAmount = g.Sum(x => x.LineTotal) });

        var returnDict = returnsByProduct.ToDictionary(x => x.ProductId);

        return grossByProduct
            .Select(x =>
            {
                var hasReturn = returnDict.TryGetValue(x.ProductId, out var r);
                var returnQty = hasReturn ? r!.ReturnQuantity : 0m;
                var returnAmt = hasReturn ? r!.ReturnAmount : 0m;
                return new ShopDashboardTopProductDto
                {
                    ProductId = x.ProductId,
                    ProductCode = x.ProductCodeSnapshot,
                    ProductName = x.ProductNameSnapshot,
                    UnitName = x.UnitNameSnapshot,
                    QuantitySold = x.QuantitySold,
                    GrossSalesAmount = Math.Round(x.GrossSalesAmount, 2),
                    ReturnQuantity = returnQty,
                    NetQuantitySold = x.QuantitySold - returnQty,
                    NetSalesAmount = Math.Round(x.GrossSalesAmount - returnAmt, 2),
                };
            })
            .OrderByDescending(x => x.NetQuantitySold)
            .ThenByDescending(x => x.NetSalesAmount)
            .Take(count)
            .ToList();
    }

    // ------------------------------------------------------------------
    // Recent transactions
    // ------------------------------------------------------------------

    private async Task<List<ShopDashboardRecentSaleDto>> ComputeRecentSalesAsync(Guid tenantId, int count)
    {
        var saleQuery = (await _saleRepository.GetQueryableAsync()).AsNoTracking();
        var customerQuery = (await _customerRepository.GetQueryableAsync()).AsNoTracking();

        var rows = await AsyncExecuter.ToListAsync(
            (from s in saleQuery
             where s.TenantId == tenantId && s.Status == ShopSaleStatus.Completed
             join c in customerQuery on s.CustomerId equals c.Id into customers
             from c in customers.DefaultIfEmpty()
             orderby s.SaleDate descending, s.CreationTime descending
             select new { s.Id, s.SaleNumber, s.SaleDate, s.CustomerId, CustomerName = c != null ? c.Name : string.Empty, s.GrandTotal, s.PaidAmount, s.PendingAmount, s.Status })
            .Take(count));

        return rows.Select(x => new ShopDashboardRecentSaleDto
        {
            SaleId = x.Id,
            InvoiceNumber = x.SaleNumber,
            SaleDate = x.SaleDate,
            CustomerId = x.CustomerId,
            CustomerName = x.CustomerName,
            TotalAmount = x.GrandTotal,
            PaidAmount = x.PaidAmount,
            PendingAmount = x.PendingAmount,
            PaymentStatus = ComputePaymentStatus(x.PaidAmount, x.PendingAmount),
            SaleStatus = x.Status,
        }).ToList();
    }

    private async Task<List<ShopDashboardRecentPurchaseDto>> ComputeRecentPurchasesAsync(Guid tenantId, int count)
    {
        var grQuery = (await _goodsReceiptRepository.GetQueryableAsync()).AsNoTracking();
        var supplierQuery = (await _supplierRepository.GetQueryableAsync()).AsNoTracking();
        var returnQuery = (await _purchaseReturnRepository.GetQueryableAsync()).AsNoTracking();

        var rows = await AsyncExecuter.ToListAsync(
            (from gr in grQuery
             where gr.TenantId == tenantId && gr.Status == ShopGoodsReceiptStatus.Completed
             join s in supplierQuery on gr.SupplierId equals s.Id into suppliers
             from s in suppliers.DefaultIfEmpty()
             orderby gr.ReceiptDate descending, gr.CreationTime descending
             select new { gr.Id, gr.GoodsReceiptNumber, gr.ReceiptDate, gr.SupplierId, SupplierName = s != null ? s.Name : string.Empty, gr.GrandTotal })
            .Take(count));

        if (rows.Count == 0) return new List<ShopDashboardRecentPurchaseDto>();

        var receiptIds = rows.Select(x => x.Id).ToList();
        var paidAmounts = await _supplierPaymentManager.GetPostedAllocatedAmountsAsync(tenantId, receiptIds, excludePaymentId: null);

        var returnAmounts = (await AsyncExecuter.ToListAsync(
                returnQuery.Where(x => x.TenantId == tenantId && receiptIds.Contains(x.GoodsReceiptId) && x.Status == ShopPurchaseReturnStatus.Completed)))
            .GroupBy(x => x.GoodsReceiptId)
            .ToDictionary(x => x.Key, x => x.Sum(y => y.GrandTotal));

        return rows.Select(x =>
        {
            var paid = paidAmounts.GetValueOrDefault(x.Id);
            var returned = returnAmounts.GetValueOrDefault(x.Id);
            var pending = Math.Max(0, x.GrandTotal - paid - returned);
            return new ShopDashboardRecentPurchaseDto
            {
                PurchaseId = x.Id,
                DocumentNumber = x.GoodsReceiptNumber,
                DocumentDate = x.ReceiptDate,
                SupplierId = x.SupplierId,
                SupplierName = x.SupplierName,
                TotalAmount = x.GrandTotal,
                PaidAmount = paid,
                PendingAmount = pending,
                Status = ComputePaymentStatus(paid, pending),
            };
        }).ToList();
    }

    private async Task<List<ShopDashboardRecentExpenseDto>> ComputeRecentExpensesAsync(Guid tenantId, int count)
    {
        var expenseQuery = (await _expenseRepository.GetQueryableAsync()).AsNoTracking();
        var categoryQuery = (await _expenseCategoryRepository.GetQueryableAsync()).AsNoTracking();

        var rows = await AsyncExecuter.ToListAsync(
            (from e in expenseQuery
             where e.TenantId == tenantId && e.Status == ShopExpenseStatus.Posted
             join c in categoryQuery on e.ExpenseCategoryId equals c.Id into categories
             from c in categories.DefaultIfEmpty()
             orderby e.ExpenseDate descending, e.CreationTime descending
             select new { e.Id, e.ExpenseNumber, e.ExpenseDate, CategoryName = c != null ? c.Name : string.Empty, e.Description, e.Amount, e.PaymentMethod, e.Status })
            .Take(count));

        return rows.Select(x => new ShopDashboardRecentExpenseDto
        {
            ExpenseId = x.Id,
            ExpenseNumber = x.ExpenseNumber,
            ExpenseDate = x.ExpenseDate,
            ExpenseCategoryName = x.CategoryName,
            Description = x.Description,
            Amount = x.Amount,
            PaymentSource = x.PaymentMethod,
            Status = x.Status,
        }).ToList();
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static ShopGoodsReceiptPaymentStatus ComputePaymentStatus(decimal paidAmount, decimal pendingAmount)
    {
        if (pendingAmount <= 0) return ShopGoodsReceiptPaymentStatus.Paid;
        if (paidAmount > 0) return ShopGoodsReceiptPaymentStatus.PartiallyPaid;
        return ShopGoodsReceiptPaymentStatus.Unpaid;
    }

    private async Task<ShopSetting?> GetSettingAsync(Guid tenantId)
    {
        var query = (await _settingRepository.GetQueryableAsync()).AsNoTracking();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.TenantId == tenantId));
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}
