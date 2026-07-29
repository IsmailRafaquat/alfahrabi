using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.PurchaseReturns;
using EHub.ShopManagement.SaleReturns;
using EHub.ShopManagement.Sales;
using EHub.ShopManagement.Settings;
using EHub.ShopManagement.StockTransactions;
using EHub.ShopManagement.Suppliers;
using EHub.ShopManagement.SupplierPayments;
using EHub.ShopManagement.Units;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace EHub.ShopManagement.Reports;

/// <summary>
/// Read-only reporting over existing Shop Management transactional tables. Never creates, updates,
/// posts, cancels, deletes, or reverses anything - every method is a projection/aggregation query.
/// </summary>
[Authorize(EHubPermissions.ShopReports.Default)]
public partial class ShopReportAppService : ApplicationService, IShopReportAppService
{
    private const int MaxExportRows = 50000;

    private readonly IRepository<ShopSale, Guid> _saleRepository;
    private readonly IRepository<ShopSaleItem, Guid> _saleItemRepository;
    private readonly IRepository<ShopSaleReturn, Guid> _saleReturnRepository;
    private readonly IRepository<ShopSaleReturnItem, Guid> _saleReturnItemRepository;
    private readonly IRepository<ShopGoodsReceipt, Guid> _goodsReceiptRepository;
    private readonly IRepository<ShopGoodsReceiptItem, Guid> _goodsReceiptItemRepository;
    private readonly IRepository<ShopPurchaseOrder, Guid> _purchaseOrderRepository;
    private readonly IRepository<ShopPurchaseReturn, Guid> _purchaseReturnRepository;
    private readonly IRepository<ShopPurchaseReturnItem, Guid> _purchaseReturnItemRepository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopProductCategory, Guid> _categoryRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly IRepository<ShopProductBatch, Guid> _productBatchRepository;
    private readonly IRepository<ShopStockTransaction, Guid> _stockTransactionRepository;
    private readonly IRepository<ShopCustomer, Guid> _customerRepository;
    private readonly IRepository<ShopCustomerPayment, Guid> _customerPaymentRepository;
    private readonly IRepository<ShopSupplier, Guid> _supplierRepository;
    private readonly IRepository<ShopSupplierPayment, Guid> _supplierPaymentRepository;
    private readonly ShopSupplierPaymentManager _supplierPaymentManager;
    private readonly IRepository<ShopExpense, Guid> _expenseRepository;
    private readonly IRepository<ShopExpenseCategory, Guid> _expenseCategoryRepository;
    private readonly IRepository<ShopCashRegister, Guid> _cashRegisterRepository;
    private readonly IRepository<ShopCashRegisterTransaction, Guid> _cashTransactionRepository;
    private readonly IRepository<ShopCashClosing, Guid> _cashClosingRepository;
    private readonly IRepository<ShopBankAccount, Guid> _bankAccountRepository;
    private readonly IRepository<ShopBankTransaction, Guid> _bankTransactionRepository;
    private readonly IRepository<ShopSetting, Guid> _settingRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IShopReportDateRangeResolver _dateRangeResolver;
    private readonly IShopReportExportService _exportService;

    public ShopReportAppService(
        IRepository<ShopSale, Guid> saleRepository,
        IRepository<ShopSaleItem, Guid> saleItemRepository,
        IRepository<ShopSaleReturn, Guid> saleReturnRepository,
        IRepository<ShopSaleReturnItem, Guid> saleReturnItemRepository,
        IRepository<ShopGoodsReceipt, Guid> goodsReceiptRepository,
        IRepository<ShopGoodsReceiptItem, Guid> goodsReceiptItemRepository,
        IRepository<ShopPurchaseOrder, Guid> purchaseOrderRepository,
        IRepository<ShopPurchaseReturn, Guid> purchaseReturnRepository,
        IRepository<ShopPurchaseReturnItem, Guid> purchaseReturnItemRepository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopProductCategory, Guid> categoryRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        IRepository<ShopProductBatch, Guid> productBatchRepository,
        IRepository<ShopStockTransaction, Guid> stockTransactionRepository,
        IRepository<ShopCustomer, Guid> customerRepository,
        IRepository<ShopCustomerPayment, Guid> customerPaymentRepository,
        IRepository<ShopSupplier, Guid> supplierRepository,
        IRepository<ShopSupplierPayment, Guid> supplierPaymentRepository,
        ShopSupplierPaymentManager supplierPaymentManager,
        IRepository<ShopExpense, Guid> expenseRepository,
        IRepository<ShopExpenseCategory, Guid> expenseCategoryRepository,
        IRepository<ShopCashRegister, Guid> cashRegisterRepository,
        IRepository<ShopCashRegisterTransaction, Guid> cashTransactionRepository,
        IRepository<ShopCashClosing, Guid> cashClosingRepository,
        IRepository<ShopBankAccount, Guid> bankAccountRepository,
        IRepository<ShopBankTransaction, Guid> bankTransactionRepository,
        IRepository<ShopSetting, Guid> settingRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IShopReportDateRangeResolver dateRangeResolver,
        IShopReportExportService exportService)
    {
        _saleRepository = saleRepository;
        _saleItemRepository = saleItemRepository;
        _saleReturnRepository = saleReturnRepository;
        _saleReturnItemRepository = saleReturnItemRepository;
        _goodsReceiptRepository = goodsReceiptRepository;
        _goodsReceiptItemRepository = goodsReceiptItemRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _purchaseReturnRepository = purchaseReturnRepository;
        _purchaseReturnItemRepository = purchaseReturnItemRepository;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitRepository = unitRepository;
        _productBatchRepository = productBatchRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _customerRepository = customerRepository;
        _customerPaymentRepository = customerPaymentRepository;
        _supplierRepository = supplierRepository;
        _supplierPaymentRepository = supplierPaymentRepository;
        _supplierPaymentManager = supplierPaymentManager;
        _expenseRepository = expenseRepository;
        _expenseCategoryRepository = expenseCategoryRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _cashTransactionRepository = cashTransactionRepository;
        _cashClosingRepository = cashClosingRepository;
        _bankAccountRepository = bankAccountRepository;
        _bankTransactionRepository = bankTransactionRepository;
        _settingRepository = settingRepository;
        _userRepository = userRepository;
        _dateRangeResolver = dateRangeResolver;
        _exportService = exportService;
    }

    // ------------------------------------------------------------------
    // Shared helpers
    // ------------------------------------------------------------------

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:ReportTenantRequired");

    private async Task<bool> CanViewCostAsync() => await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopReports.ViewCost);

    private async Task<Dictionary<Guid, string>> GetUserNamesAsync(IEnumerable<Guid> userIds)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, string>();

        var query = (await _userRepository.GetQueryableAsync()).AsNoTracking();
        var users = await AsyncExecuter.ToListAsync(query.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.UserName }));
        return users.ToDictionary(x => x.Id, x => x.UserName);
    }

    private async Task<ShopSetting?> GetSettingAsync(Guid tenantId)
    {
        var query = (await _settingRepository.GetQueryableAsync()).AsNoTracking();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.TenantId == tenantId));
    }

    private static string ApplySorting(string? sorting, string defaultSorting) =>
        string.IsNullOrWhiteSpace(sorting) ? defaultSorting : sorting;

    private static readonly System.Reflection.MethodInfo[] QueryableSortMethods =
        typeof(Queryable).GetMethods().Where(m => m.GetParameters().Length == 2 &&
            (m.Name == "OrderBy" || m.Name == "OrderByDescending" || m.Name == "ThenBy" || m.Name == "ThenByDescending")).ToArray();

    /// <summary>
    /// Applies a "Field [desc], Field2 [desc]" sort string to an IQueryable using a properly-typed
    /// member-access expression (built via reflection) rather than EF.Property&lt;object&gt;, which
    /// EF Core's SQL Server provider can fail to translate for ORDER BY on value-typed columns
    /// (e.g. "Translation of 'EF.Property&lt;object&gt;(...)' failed"). This avoids requiring the
    /// System.Linq.Dynamic.Core package while still producing a real, provider-translatable SQL ORDER BY.
    /// </summary>
    private static IQueryable<T> ApplyDynamicSort<T>(IQueryable<T> query, string sorting) where T : class
    {
        IQueryable<T> ordered = query;
        var isFirst = true;
        foreach (var part in sorting.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var tokens = part.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) continue;

            var field = tokens[0];
            var descending = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.PropertyOrField(parameter, field);
            var lambda = Expression.Lambda(property, parameter);

            var methodName = isFirst
                ? (descending ? "OrderByDescending" : "OrderBy")
                : (descending ? "ThenByDescending" : "ThenBy");

            var method = QueryableSortMethods.Single(m => m.Name == methodName).MakeGenericMethod(typeof(T), property.Type);
            ordered = (IQueryable<T>)method.Invoke(null, new object[] { ordered, lambda })!;
            isFirst = false;
        }

        return ordered;
    }

    private void EnsureWithinExportLimit(int count)
    {
        if (count > MaxExportRows) throw new BusinessException("ShopManagement:ReportExportLimitExceeded").WithData("MaxRows", MaxExportRows);
    }

    // ------------------------------------------------------------------
    // 1. Sales Report
    // ------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopReports.Sales)]
    public async Task<ShopSalesReportResultDto> GetSalesReportAsync(GetShopSalesReportInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);

        var filtered = await BuildSalesReportQueryAsync(tenantId, range, input);
        var totals = await ComputeSalesTotalsAsync(filtered);

        var sorting = ApplySorting(input.Sorting, "SaleDate desc, CreationTime desc");
        var totalCount = await AsyncExecuter.CountAsync(filtered);
        var paged = await AsyncExecuter.ToListAsync(ApplyDynamicSort(filtered, sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var items = await ProjectSalesItemsAsync(paged);

        var result = new ShopSalesReportResultDto { Items = items, TotalCount = totalCount, Totals = totals };

        if (input.GroupBy != ShopSalesReportGroupBy.None)
        {
            result.Groups = await BuildSalesGroupsAsync(filtered, input.GroupBy);
        }

        return result;
    }

    [Authorize(EHubPermissions.ShopReports.Sales), Authorize(EHubPermissions.ShopReports.Export)]
    public async Task<IRemoteStreamContent> ExportSalesReportAsync(GetShopSalesReportInput input, ShopReportExportFormat format)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);
        var filtered = await BuildSalesReportQueryAsync(tenantId, range, input);
        var totals = await ComputeSalesTotalsAsync(filtered);

        var count = await AsyncExecuter.CountAsync(filtered);
        EnsureWithinExportLimit(count);

        var all = await AsyncExecuter.ToListAsync(ApplyDynamicSort(filtered, ApplySorting(input.Sorting, "SaleDate desc")));
        var items = await ProjectSalesItemsAsync(all);

        var setting = await GetSettingAsync(tenantId);
        var request = new ShopReportExportRequest
        {
            ReportTitle = "Sales Report",
            ShopName = setting?.ShopDisplayName,
            ShopAddress = CombineAddress(setting),
            ShopPhone = setting?.Phone,
            DateFrom = range.From,
            DateTo = range.ToExclusive.AddDays(-1),
            GeneratedDate = Clock.Now,
            Columns = new List<ShopReportExportColumn>
            {
                new("Invoice", "invoiceNumber"), new("Date", "saleDate"), new("Customer", "customerName"),
                new("Items", "totalItems"), new("Quantity", "totalQuantity"), new("Gross", "grossAmount"),
                new("Discount", "discountAmount"), new("Tax", "taxAmount"), new("Return", "returnAmount"),
                new("Net Sales", "finalSalesAmount"), new("Paid", "paidAmount"), new("Pending", "pendingAmount"),
                new("Status", "saleStatus"),
            },
            Rows = items.Select(x => new Dictionary<string, object?>
            {
                ["invoiceNumber"] = x.InvoiceNumber, ["saleDate"] = x.SaleDate, ["customerName"] = x.CustomerName,
                ["totalItems"] = x.TotalItems, ["totalQuantity"] = x.TotalQuantity, ["grossAmount"] = x.GrossAmount,
                ["discountAmount"] = x.DiscountAmount, ["taxAmount"] = x.TaxAmount, ["returnAmount"] = x.ReturnAmount,
                ["finalSalesAmount"] = x.FinalSalesAmount, ["paidAmount"] = x.PaidAmount, ["pendingAmount"] = x.PendingAmount,
                ["saleStatus"] = x.SaleStatus.ToString(),
            }).ToList(),
            TotalsLines = new List<(string, string)>
            {
                ("Sale Count", totals.SaleCount.ToString()), ("Gross Sales", totals.GrossSales.ToString("N2")),
                ("Discount", totals.TotalDiscount.ToString("N2")), ("Tax", totals.TotalTax.ToString("N2")),
                ("Sale Returns", totals.SaleReturnAmount.ToString("N2")), ("Final Net Sales", totals.FinalNetSales.ToString("N2")),
                ("Paid", totals.PaidAmount.ToString("N2")), ("Pending", totals.PendingAmount.ToString("N2")),
            },
        };

        return await _exportService.ExportAsync(request, format);
    }

    private async Task<IQueryable<ShopSale>> BuildSalesReportQueryAsync(Guid tenantId, ShopReportDateRange range, GetShopSalesReportInput input)
    {
        var saleQuery = (await _saleRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SaleDate >= range.From && x.SaleDate < range.ToExclusive);

        saleQuery = input.SaleStatus.HasValue
            ? saleQuery.Where(x => x.Status == input.SaleStatus.Value)
            : saleQuery.Where(x => x.Status == ShopSaleStatus.Completed);

        if (input.CustomerId.HasValue) saleQuery = saleQuery.Where(x => x.CustomerId == input.CustomerId.Value);
        if (input.PaymentMethod.HasValue) saleQuery = saleQuery.Where(x => x.PaymentMethod == input.PaymentMethod.Value);
        if (input.MinimumAmount.HasValue) saleQuery = saleQuery.Where(x => x.GrandTotal >= input.MinimumAmount.Value);
        if (input.MaximumAmount.HasValue) saleQuery = saleQuery.Where(x => x.GrandTotal <= input.MaximumAmount.Value);
        if (input.HasPendingAmount == true) saleQuery = saleQuery.Where(x => x.PendingAmount > 0);
        if (input.HasPendingAmount == false) saleQuery = saleQuery.Where(x => x.PendingAmount <= 0);
        // PaymentStatus is computed from Paid/Pending (both real persisted columns on ShopSale, unlike
        // Goods Receipt), so it translates directly to SQL rather than needing an in-memory fallback.
        if (input.PaymentStatus == ShopGoodsReceiptPaymentStatus.Paid) saleQuery = saleQuery.Where(x => x.PendingAmount <= 0);
        if (input.PaymentStatus == ShopGoodsReceiptPaymentStatus.Unpaid) saleQuery = saleQuery.Where(x => x.PaidAmount <= 0 && x.PendingAmount > 0);
        if (input.PaymentStatus == ShopGoodsReceiptPaymentStatus.PartiallyPaid) saleQuery = saleQuery.Where(x => x.PaidAmount > 0 && x.PendingAmount > 0);
        if (input.CreatedByUserId.HasValue) saleQuery = saleQuery.Where(x => x.CreatorId == input.CreatedByUserId.Value);
        if (!string.IsNullOrWhiteSpace(input.InvoiceNumber)) saleQuery = saleQuery.Where(x => x.SaleNumber.Contains(input.InvoiceNumber));

        if (input.ProductId.HasValue || input.ProductCategoryId.HasValue)
        {
            var itemQuery = (await _saleItemRepository.GetQueryableAsync()).AsNoTracking();
            var productQuery = (await _productRepository.GetQueryableAsync()).AsNoTracking();

            var matchingSaleIds = itemQuery.Where(i => i.TenantId == tenantId);
            if (input.ProductId.HasValue) matchingSaleIds = matchingSaleIds.Where(i => i.ProductId == input.ProductId.Value);
            if (input.ProductCategoryId.HasValue)
            {
                var categoryProductIds = productQuery.Where(p => p.TenantId == tenantId && p.CategoryId == input.ProductCategoryId.Value).Select(p => p.Id);
                matchingSaleIds = matchingSaleIds.Where(i => categoryProductIds.Contains(i.ProductId));
            }

            var saleIds = matchingSaleIds.Select(i => i.SaleId);
            saleQuery = saleQuery.Where(x => saleIds.Contains(x.Id));
        }

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var term = input.Filter;
            var customerQuery = (await _customerRepository.GetQueryableAsync()).AsNoTracking();
            var matchingCustomerIds = customerQuery.Where(c => c.TenantId == tenantId && (c.Name.Contains(term) || (c.Phone != null && c.Phone.Contains(term)))).Select(c => c.Id);

            var itemQuery = (await _saleItemRepository.GetQueryableAsync()).AsNoTracking();
            var matchingSaleIdsByProduct = itemQuery.Where(i => i.TenantId == tenantId && (i.ProductCodeSnapshot.Contains(term) || i.ProductNameSnapshot.Contains(term))).Select(i => i.SaleId);

            saleQuery = saleQuery.Where(x =>
                x.SaleNumber.Contains(term) ||
                matchingCustomerIds.Contains(x.CustomerId) ||
                (x.Notes != null && x.Notes.Contains(term)) ||
                matchingSaleIdsByProduct.Contains(x.Id));
        }

        return saleQuery;
    }

    private async Task<List<ShopSalesReportItemDto>> ProjectSalesItemsAsync(List<ShopSale> sales)
    {
        if (sales.Count == 0) return new List<ShopSalesReportItemDto>();

        var tenantId = RequireTenant();
        var saleIds = sales.Select(x => x.Id).ToList();
        var customerIds = sales.Select(x => x.CustomerId).Distinct().ToList();

        var customerQuery = (await _customerRepository.GetQueryableAsync()).AsNoTracking();
        var customers = await AsyncExecuter.ToListAsync(customerQuery.Where(x => customerIds.Contains(x.Id)));
        var customerDict = customers.ToDictionary(x => x.Id);

        var itemQuery = (await _saleItemRepository.GetQueryableAsync()).AsNoTracking();
        var itemAggregates = await AsyncExecuter.ToListAsync(
            itemQuery.Where(x => x.TenantId == tenantId && saleIds.Contains(x.SaleId))
                .GroupBy(x => x.SaleId)
                .Select(g => new { SaleId = g.Key, Count = g.Count(), Quantity = g.Sum(x => x.Quantity) }));
        var itemDict = itemAggregates.ToDictionary(x => x.SaleId);

        var returnQuery = (await _saleReturnRepository.GetQueryableAsync()).AsNoTracking();
        var returnAggregates = await AsyncExecuter.ToListAsync(
            returnQuery.Where(x => x.TenantId == tenantId && saleIds.Contains(x.SaleId) && x.Status == ShopSaleReturnStatus.Completed)
                .GroupBy(x => x.SaleId)
                .Select(g => new { SaleId = g.Key, Returned = g.Sum(x => x.GrandTotal) }));
        var returnDict = returnAggregates.ToDictionary(x => x.SaleId, x => x.Returned);

        var userIds = sales.Where(x => x.CreatorId.HasValue).Select(x => x.CreatorId!.Value);
        var userNames = await GetUserNamesAsync(userIds);

        return sales.Select(sale =>
        {
            customerDict.TryGetValue(sale.CustomerId, out var customer);
            itemDict.TryGetValue(sale.Id, out var itemAgg);
            var returned = returnDict.GetValueOrDefault(sale.Id);
            var finalSales = Math.Round(sale.GrandTotal - returned, 2);

            return new ShopSalesReportItemDto
            {
                SaleId = sale.Id,
                InvoiceNumber = sale.SaleNumber,
                SaleDate = sale.SaleDate,
                CustomerId = sale.CustomerId,
                CustomerName = customer?.Name ?? string.Empty,
                CustomerPhone = customer?.Phone,
                GrossAmount = sale.SubTotal,
                DiscountAmount = sale.DiscountAmount,
                TaxAmount = sale.TaxAmount,
                NetAmount = sale.GrandTotal,
                PaidAmount = sale.PaidAmount,
                PendingAmount = sale.PendingAmount,
                ReturnAmount = returned,
                FinalSalesAmount = finalSales,
                PaymentStatus = ComputePaymentStatus(sale.PaidAmount, sale.PendingAmount),
                SaleStatus = sale.Status,
                PaymentMethod = sale.PaymentMethod,
                TotalItems = itemAgg?.Count ?? 0,
                TotalQuantity = itemAgg?.Quantity ?? 0,
                CreatedByUserName = sale.CreatorId.HasValue ? userNames.GetValueOrDefault(sale.CreatorId.Value) : null,
                CreationTime = sale.CreationTime,
            };
        }).ToList();
    }

    private async Task<ShopSalesReportTotalsDto> ComputeSalesTotalsAsync(IQueryable<ShopSale> filtered)
    {
        var tenantId = RequireTenant();

        var saleAgg = await AsyncExecuter.FirstOrDefaultAsync(
            filtered.GroupBy(x => 1).Select(g => new
            {
                Count = g.Count(),
                Gross = g.Sum(x => x.SubTotal),
                Discount = g.Sum(x => x.DiscountAmount),
                Tax = g.Sum(x => x.TaxAmount),
                Net = g.Sum(x => x.GrandTotal),
                Paid = g.Sum(x => x.PaidAmount),
                Pending = g.Sum(x => x.PendingAmount),
            }));

        if (saleAgg == null || saleAgg.Count == 0)
        {
            return new ShopSalesReportTotalsDto();
        }

        var saleIds = await AsyncExecuter.ToListAsync(filtered.Select(x => x.Id));

        var itemQuery = (await _saleItemRepository.GetQueryableAsync()).AsNoTracking();
        var totalQuantity = await AsyncExecuter.SumAsync(
            itemQuery.Where(x => x.TenantId == tenantId && saleIds.Contains(x.SaleId)).Select(x => (decimal?)x.Quantity)) ?? 0m;

        var returnQuery = (await _saleReturnRepository.GetQueryableAsync()).AsNoTracking();
        var returnAmount = await AsyncExecuter.SumAsync(
            returnQuery.Where(x => x.TenantId == tenantId && saleIds.Contains(x.SaleId) && x.Status == ShopSaleReturnStatus.Completed)
                .Select(x => (decimal?)x.GrandTotal)) ?? 0m;

        var finalNetSales = Math.Round(saleAgg.Net - returnAmount, 2);

        return new ShopSalesReportTotalsDto
        {
            SaleCount = saleAgg.Count,
            GrossSales = saleAgg.Gross,
            TotalDiscount = saleAgg.Discount,
            TotalTax = saleAgg.Tax,
            NetSalesBeforeReturns = saleAgg.Net,
            SaleReturnAmount = returnAmount,
            FinalNetSales = finalNetSales,
            PaidAmount = saleAgg.Paid,
            PendingAmount = saleAgg.Pending,
            TotalQuantitySold = totalQuantity,
            AverageSaleValue = saleAgg.Count > 0 ? Math.Round(finalNetSales / saleAgg.Count, 2) : 0,
        };
    }

    private async Task<List<ShopSalesReportGroupItemDto>> BuildSalesGroupsAsync(IQueryable<ShopSale> filtered, ShopSalesReportGroupBy groupBy)
    {
        var tenantId = RequireTenant();
        var sales = await AsyncExecuter.ToListAsync(filtered);
        if (sales.Count == 0) return new List<ShopSalesReportGroupItemDto>();

        var saleIds = sales.Select(x => x.Id).ToList();
        var itemQuery = (await _saleItemRepository.GetQueryableAsync()).AsNoTracking();
        var items = await AsyncExecuter.ToListAsync(itemQuery.Where(x => x.TenantId == tenantId && saleIds.Contains(x.SaleId)));
        var qtyBySale = items.GroupBy(x => x.SaleId).ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        var returnQuery = (await _saleReturnRepository.GetQueryableAsync()).AsNoTracking();
        var returns = await AsyncExecuter.ToListAsync(returnQuery.Where(x => x.TenantId == tenantId && saleIds.Contains(x.SaleId) && x.Status == ShopSaleReturnStatus.Completed));
        var returnBySale = returns.GroupBy(x => x.SaleId).ToDictionary(g => g.Key, g => g.Sum(x => x.GrandTotal));

        Dictionary<Guid, string>? customerNames = null;
        Dictionary<Guid, string>? productNames = null;
        Dictionary<Guid, string>? categoryNames = null;

        if (groupBy == ShopSalesReportGroupBy.Customer)
        {
            var customerIds = sales.Select(x => x.CustomerId).Distinct().ToList();
            var customerQuery = (await _customerRepository.GetQueryableAsync()).AsNoTracking();
            customerNames = (await AsyncExecuter.ToListAsync(customerQuery.Where(x => customerIds.Contains(x.Id)))).ToDictionary(x => x.Id, x => x.Name);
        }
        else if (groupBy is ShopSalesReportGroupBy.Product or ShopSalesReportGroupBy.Category)
        {
            var productIds = items.Select(x => x.ProductId).Distinct().ToList();
            var productQuery = (await _productRepository.GetQueryableAsync()).AsNoTracking();
            var products = await AsyncExecuter.ToListAsync(productQuery.Where(x => productIds.Contains(x.Id)));
            productNames = products.ToDictionary(x => x.Id, x => x.Name);

            if (groupBy == ShopSalesReportGroupBy.Category)
            {
                var categoryIds = products.Select(x => x.CategoryId).Distinct().ToList();
                var categoryQuery = (await _categoryRepository.GetQueryableAsync()).AsNoTracking();
                categoryNames = (await AsyncExecuter.ToListAsync(categoryQuery.Where(x => categoryIds.Contains(x.Id)))).ToDictionary(x => x.Id, x => x.Name);
            }
        }

        // Product/Category grouping works at the sale-ITEM level (each line contributes its own
        // gross/net figures); Day/Month/Customer/PaymentMethod grouping works at the sale-HEADER
        // level (returns are attached once per sale, not split across its lines).
        if (groupBy is ShopSalesReportGroupBy.Product or ShopSalesReportGroupBy.Category)
        {
            var lineGroups = new Dictionary<string, ShopSalesReportGroupItemDto>();

            Dictionary<Guid, Guid>? productToCategory = null;
            if (groupBy == ShopSalesReportGroupBy.Category)
            {
                var productQuery2 = (await _productRepository.GetQueryableAsync()).AsNoTracking();
                var productIds2 = items.Select(x => x.ProductId).Distinct().ToList();
                var products2 = await AsyncExecuter.ToListAsync(productQuery2.Where(x => productIds2.Contains(x.Id)).Select(x => new { x.Id, x.CategoryId }));
                productToCategory = products2.ToDictionary(x => x.Id, x => x.CategoryId);
            }

            foreach (var item in items)
            {
                if (groupBy == ShopSalesReportGroupBy.Product)
                {
                    var pName = productNames?.GetValueOrDefault(item.ProductId) ?? item.ProductNameSnapshot;
                    AddLineToGroup(lineGroups, item.ProductId.ToString(), pName, item);
                }
                else
                {
                    var categoryId = productToCategory!.GetValueOrDefault(item.ProductId);
                    var categoryName = categoryNames?.GetValueOrDefault(categoryId) ?? string.Empty;
                    AddLineToGroup(lineGroups, categoryId.ToString(), categoryName, item);
                }
            }

            return lineGroups.Values.OrderByDescending(x => x.FinalSalesAmount).ToList();
        }

        var groups = new Dictionary<string, ShopSalesReportGroupItemDto>();

        void AddToGroup(string key, string label, ShopSale sale, decimal quantity, decimal returnAmount)
        {
            if (!groups.TryGetValue(key, out var g))
            {
                g = new ShopSalesReportGroupItemDto { GroupKey = key, GroupLabel = label };
                groups[key] = g;
            }

            g.SaleCount++;
            g.GrossAmount += sale.SubTotal;
            g.DiscountAmount += sale.DiscountAmount;
            g.TaxAmount += sale.TaxAmount;
            g.NetAmount += sale.GrandTotal;
            g.ReturnAmount += returnAmount;
            g.FinalSalesAmount += sale.GrandTotal - returnAmount;
            g.TotalQuantity += quantity;
        }

        foreach (var sale in sales)
        {
            var quantity = qtyBySale.GetValueOrDefault(sale.Id);
            var returned = returnBySale.GetValueOrDefault(sale.Id);

            switch (groupBy)
            {
                case ShopSalesReportGroupBy.Day:
                    var day = sale.SaleDate.Date;
                    AddToGroup(day.ToString("yyyy-MM-dd"), day.ToString("MMM dd, yyyy"), sale, quantity, returned);
                    break;
                case ShopSalesReportGroupBy.Month:
                    var month = new DateTime(sale.SaleDate.Year, sale.SaleDate.Month, 1);
                    AddToGroup(month.ToString("yyyy-MM"), month.ToString("MMM yyyy"), sale, quantity, returned);
                    break;
                case ShopSalesReportGroupBy.Customer:
                    var custName = customerNames?.GetValueOrDefault(sale.CustomerId) ?? string.Empty;
                    AddToGroup(sale.CustomerId.ToString(), custName, sale, quantity, returned);
                    break;
                case ShopSalesReportGroupBy.PaymentMethod:
                    AddToGroup(sale.PaymentMethod.ToString(), sale.PaymentMethod.ToString(), sale, quantity, returned);
                    break;
            }
        }

        return groups.Values.OrderByDescending(x => x.FinalSalesAmount).ToList();
    }

    private static void AddLineToGroup(Dictionary<string, ShopSalesReportGroupItemDto> groups, string key, string label, ShopSaleItem item)
    {
        if (!groups.TryGetValue(key, out var g))
        {
            g = new ShopSalesReportGroupItemDto { GroupKey = key, GroupLabel = label };
            groups[key] = g;
        }

        g.SaleCount++;
        g.GrossAmount += item.LineSubTotal;
        g.DiscountAmount += item.DiscountAmount;
        g.TaxAmount += item.TaxAmount;
        g.NetAmount += item.LineTotal;
        g.FinalSalesAmount += item.LineTotal;
        g.TotalQuantity += item.Quantity;
    }

    private static ShopGoodsReceiptPaymentStatus ComputePaymentStatus(decimal paidAmount, decimal pendingAmount)
    {
        if (pendingAmount <= 0) return ShopGoodsReceiptPaymentStatus.Paid;
        if (paidAmount > 0) return ShopGoodsReceiptPaymentStatus.PartiallyPaid;
        return ShopGoodsReceiptPaymentStatus.Unpaid;
    }

    private static string? CombineAddress(ShopSetting? setting)
    {
        if (setting == null) return null;
        var parts = new[] { setting.AddressLine1, setting.AddressLine2, setting.City, setting.StateOrProvince, setting.PostalCode, setting.Country }
            .Where(x => !string.IsNullOrWhiteSpace(x));
        return string.Join(", ", parts);
    }
}
