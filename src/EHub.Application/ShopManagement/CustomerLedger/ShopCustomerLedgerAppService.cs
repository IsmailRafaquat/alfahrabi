using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.CustomerPayments;
using EHub.ShopManagement.Customers;
using EHub.ShopManagement.SaleReturns;
using EHub.ShopManagement.Sales;
using EHub.ShopManagement.Settings;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.CustomerLedger;

[Authorize(EHubPermissions.ShopCustomerLedger.Default)]
public class ShopCustomerLedgerAppService : ApplicationService, IShopCustomerLedgerAppService
{
    private readonly IRepository<ShopCustomer, Guid> _customerRepository;
    private readonly IRepository<ShopSale, Guid> _saleRepository;
    private readonly IRepository<ShopSaleItem, Guid> _saleItemRepository;
    private readonly IRepository<ShopCustomerPayment, Guid> _paymentRepository;
    private readonly IRepository<ShopSaleReturn, Guid> _saleReturnRepository;
    private readonly IRepository<ShopSetting, Guid> _settingRepository;

    public ShopCustomerLedgerAppService(
        IRepository<ShopCustomer, Guid> customerRepository,
        IRepository<ShopSale, Guid> saleRepository,
        IRepository<ShopSaleItem, Guid> saleItemRepository,
        IRepository<ShopCustomerPayment, Guid> paymentRepository,
        IRepository<ShopSaleReturn, Guid> saleReturnRepository,
        IRepository<ShopSetting, Guid> settingRepository)
    {
        _customerRepository = customerRepository;
        _saleRepository = saleRepository;
        _saleItemRepository = saleItemRepository;
        _paymentRepository = paymentRepository;
        _saleReturnRepository = saleReturnRepository;
        _settingRepository = settingRepository;
    }

    public async Task<ShopCustomerLedgerDto> GetLedgerAsync(GetShopCustomerLedgerInput input)
    {
        var dto = await BuildLedgerAsync(input);
        await HideLedgerAmountsIfNotAllowedAsync(dto);
        return dto;
    }

    public async Task<ShopCustomerBalanceSummaryDto> GetBalanceSummaryAsync(Guid customerId)
    {
        var tenantId = RequireTenant();
        var customer = await GetCustomerAsync(customerId, tenantId);
        var (rows, sales, _) = await LoadDataAsync(tenantId, customerId);

        var totalSales = Round(sales.Sum(x => x.GrandTotal));
        var totalInitialPaid = Round(sales.Sum(x => x.PaidAmount));
        var totalPayments = Round(rows.Where(x => x.ReferenceType == ShopCustomerLedgerReferenceType.CustomerPayment).Sum(x => x.Credit));
        var currentBalance = Round(customer.OpeningBalance + rows.Sum(x => x.Debit - x.Credit));
        var lastTransactionDate = rows.Count > 0 ? rows.Max(x => x.TransactionDate) : (DateTime?)null;

        var dto = new ShopCustomerBalanceSummaryDto
        {
            CustomerId = customer.Id,
            CustomerCode = customer.Code,
            CustomerName = customer.Name,
            OpeningBalance = customer.OpeningBalance,
            TotalCompletedSales = totalSales,
            TotalInitialPaidAtSale = totalInitialPaid,
            TotalPostedCustomerPayments = totalPayments,
            CurrentBalance = currentBalance,
            ReceivableAmount = Math.Max(currentBalance, 0),
            AdvanceAmount = Math.Max(-currentBalance, 0),
            LastTransactionDate = lastTransactionDate
        };

        await HideSummaryAmountsIfNotAllowedAsync(dto);
        return dto;
    }

    public async Task<ShopCustomerStatementDto> GetStatementAsync(GetShopCustomerLedgerInput input)
    {
        var tenantId = RequireTenant();
        var ledger = await BuildLedgerAsync(input);
        var customer = await GetCustomerAsync(input.CustomerId, tenantId);

        var settingQuery = await _settingRepository.GetQueryableAsync();
        var setting = await AsyncExecuter.FirstOrDefaultAsync(settingQuery.Where(x => x.TenantId == tenantId));

        var statement = new ShopCustomerStatementDto
        {
            ShopName = setting?.ShopDisplayName ?? string.Empty,
            ShopAddress = CombineAddress(setting?.AddressLine1, setting?.AddressLine2, setting?.City, setting?.StateOrProvince, setting?.PostalCode, setting?.Country),
            ShopPhone = setting?.Phone,
            ShopEmail = setting?.Email,
            CustomerId = ledger.CustomerId,
            CustomerCode = ledger.CustomerCode,
            CustomerName = ledger.CustomerName,
            CustomerAddress = CombineAddress(customer.AddressLine1, customer.AddressLine2, customer.City, customer.StateOrProvince, customer.PostalCode, customer.Country),
            CustomerPhone = customer.Phone,
            CustomerEmail = customer.Email,
            DateFrom = ledger.DateFrom,
            DateTo = ledger.DateTo,
            GeneratedDate = Clock.Now,
            OpeningBalance = ledger.OpeningBalance,
            TotalDebit = ledger.TotalDebit,
            TotalCredit = ledger.TotalCredit,
            ClosingBalance = ledger.ClosingBalance,
            ReceivableAmount = ledger.ReceivableAmount,
            AdvanceAmount = ledger.AdvanceAmount,
            TotalSaleReturns = ledger.TotalSaleReturns,
            TotalRefunds = ledger.TotalRefunds,
            TotalCustomerCredits = ledger.TotalCustomerCredits,
            Entries = ledger.Entries
        };

        await HideStatementAmountsIfNotAllowedAsync(statement);
        return statement;
    }

    private async Task<ShopCustomerLedgerDto> BuildLedgerAsync(GetShopCustomerLedgerInput input)
    {
        var tenantId = RequireTenant();
        var customer = await GetCustomerAsync(input.CustomerId, tenantId);
        var (allRows, allSales, allSaleReturns) = await LoadDataAsync(tenantId, input.CustomerId);

        var openingBalance = customer.OpeningBalance;
        var effectiveOpening = openingBalance;
        var periodRows = allRows;
        var periodSales = allSales;
        var periodSaleReturns = allSaleReturns;

        if (input.DateFrom.HasValue)
        {
            var before = allRows.Where(x => x.TransactionDate.Date < input.DateFrom.Value.Date).ToList();
            effectiveOpening = Round(openingBalance + before.Sum(x => x.Debit - x.Credit));
            periodRows = allRows.Where(x => x.TransactionDate.Date >= input.DateFrom.Value.Date).ToList();
            periodSales = allSales.Where(x => x.SaleDate.Date >= input.DateFrom.Value.Date).ToList();
            periodSaleReturns = allSaleReturns.Where(x => x.ReturnDate.Date >= input.DateFrom.Value.Date).ToList();
        }

        if (input.DateTo.HasValue)
        {
            periodRows = periodRows.Where(x => x.TransactionDate.Date <= input.DateTo.Value.Date).ToList();
            periodSales = periodSales.Where(x => x.SaleDate.Date <= input.DateTo.Value.Date).ToList();
            periodSaleReturns = periodSaleReturns.Where(x => x.ReturnDate.Date <= input.DateTo.Value.Date).ToList();
        }

        var entries = new List<ShopCustomerLedgerEntryDto> { BuildOpeningEntry(customer, input.DateFrom, effectiveOpening, openingBalance) };

        var running = effectiveOpening;
        foreach (var row in periodRows)
        {
            running = Round(running + row.Debit - row.Credit);
            entries.Add(new ShopCustomerLedgerEntryDto
            {
                TransactionDate = row.TransactionDate,
                CreationTime = row.CreationTime,
                ReferenceType = row.ReferenceType,
                ReferenceId = row.ReferenceId,
                ReferenceNumber = row.ReferenceNumber,
                Description = row.Description,
                DebitAmount = row.Debit,
                CreditAmount = row.Credit,
                RunningBalance = running,
                TransactionStatus = row.Status
            });
        }

        var totalDebit = Round(periodRows.Sum(x => x.Debit));
        var totalCredit = Round(periodRows.Sum(x => x.Credit));
        var totalPayments = Round(periodRows.Where(x => x.ReferenceType == ShopCustomerLedgerReferenceType.CustomerPayment).Sum(x => x.Credit));
        var totalSales = Round(periodSales.Sum(x => x.GrandTotal));
        var totalInitialPaid = Round(periodSales.Sum(x => x.PaidAmount));
        var totalSaleReturns = Round(periodSaleReturns.Sum(x => x.GrandTotal));
        var totalRefunds = Round(periodSaleReturns.Sum(x => x.RefundAmount));
        var totalCustomerCredits = Round(periodSaleReturns.Sum(x => x.CustomerCreditAmount));
        var closingBalance = running;

        var displayEntries = new List<ShopCustomerLedgerEntryDto> { entries[0] };
        displayEntries.AddRange(entries.Skip(1).Where(x => MatchesFilters(x, input)));

        return new ShopCustomerLedgerDto
        {
            CustomerId = customer.Id,
            CustomerCode = customer.Code,
            CustomerName = customer.Name,
            DateFrom = input.DateFrom,
            DateTo = input.DateTo,
            OpeningBalance = effectiveOpening,
            TotalSales = totalSales,
            TotalInitialPaid = totalInitialPaid,
            TotalAdditionalPayments = totalPayments,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            ClosingBalance = closingBalance,
            ReceivableAmount = Math.Max(closingBalance, 0),
            AdvanceAmount = Math.Max(-closingBalance, 0),
            TotalSaleReturns = totalSaleReturns,
            TotalRefunds = totalRefunds,
            TotalCustomerCredits = totalCustomerCredits,
            Entries = displayEntries
        };
    }

    private static bool MatchesFilters(ShopCustomerLedgerEntryDto entry, GetShopCustomerLedgerInput input)
    {
        if (input.ReferenceType.HasValue && entry.ReferenceType != input.ReferenceType.Value) return false;

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var filter = input.Filter!;
            var matches = entry.ReferenceNumber.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                          entry.Description.Contains(filter, StringComparison.OrdinalIgnoreCase);
            if (!matches) return false;
        }

        return true;
    }

    private static ShopCustomerLedgerEntryDto BuildOpeningEntry(ShopCustomer customer, DateTime? dateFrom, decimal effectiveOpening, decimal trueOpeningBalance)
    {
        if (dateFrom.HasValue)
        {
            return new ShopCustomerLedgerEntryDto
            {
                TransactionDate = dateFrom.Value,
                CreationTime = customer.CreationTime,
                ReferenceType = ShopCustomerLedgerReferenceType.OpeningBalance,
                ReferenceId = customer.Id,
                ReferenceNumber = "Opening",
                Description = "Balance Brought Forward",
                DebitAmount = 0,
                CreditAmount = 0,
                RunningBalance = effectiveOpening,
                TransactionStatus = "N/A"
            };
        }

        return new ShopCustomerLedgerEntryDto
        {
            TransactionDate = customer.CreationTime,
            CreationTime = customer.CreationTime,
            ReferenceType = ShopCustomerLedgerReferenceType.OpeningBalance,
            ReferenceId = customer.Id,
            ReferenceNumber = "Opening",
            Description = "Opening Balance",
            DebitAmount = trueOpeningBalance,
            CreditAmount = 0,
            RunningBalance = effectiveOpening,
            TransactionStatus = "N/A"
        };
    }

    private async Task<(List<LedgerRow> Rows, List<ShopSale> Sales, List<ShopSaleReturn> SaleReturns)> LoadDataAsync(Guid tenantId, Guid customerId)
    {
        var saleQuery = await _saleRepository.GetQueryableAsync();
        var sales = await AsyncExecuter.ToListAsync(saleQuery.Where(x =>
            x.TenantId == tenantId && x.CustomerId == customerId && x.Status == ShopSaleStatus.Completed));

        var paymentQuery = await _paymentRepository.GetQueryableAsync();
        var payments = await AsyncExecuter.ToListAsync(paymentQuery.Where(x =>
            x.TenantId == tenantId && x.CustomerId == customerId && x.Status == ShopCustomerPaymentStatus.Posted));

        var saleReturnQuery = await _saleReturnRepository.GetQueryableAsync();
        var saleReturns = await AsyncExecuter.ToListAsync(saleReturnQuery.Where(x =>
            x.TenantId == tenantId && x.CustomerId == customerId && x.Status == ShopSaleReturnStatus.Completed));

        var saleIds = sales.Select(x => x.Id).ToList();
        var itemsBySale = new Dictionary<Guid, List<ShopSaleItem>>();
        if (saleIds.Count > 0)
        {
            var saleItemQuery = await _saleItemRepository.GetQueryableAsync();
            var saleItems = await AsyncExecuter.ToListAsync(saleItemQuery.Where(i =>
                i.TenantId == tenantId && saleIds.Contains(i.SaleId)));
            itemsBySale = saleItems.GroupBy(i => i.SaleId).ToDictionary(g => g.Key, g => g.ToList());
        }

        var rows = new List<LedgerRow>();

        foreach (var sale in sales)
        {
            // Every completed sale gets a row, even one fully paid at sale time (Debit == Credit, net
            // zero effect on the running balance) - the ledger is also the customer's purchase/price
            // history, not just a list of what's still owed.
            var debit = Round(sale.GrandTotal);
            var credit = Round(sale.PaidAmount);
            itemsBySale.TryGetValue(sale.Id, out var items);

            rows.Add(new LedgerRow(
                sale.SaleDate, sale.CreationTime, ShopCustomerLedgerReferenceType.Sale,
                sale.Id, sale.SaleNumber, BuildSaleDescription(sale, items),
                debit, credit, "Completed"));
        }

        foreach (var payment in payments)
        {
            rows.Add(new LedgerRow(
                payment.PaymentDate, payment.CreationTime, ShopCustomerLedgerReferenceType.CustomerPayment,
                payment.Id, payment.PaymentNumber, BuildPaymentDescription(payment),
                0, payment.Amount, "Posted"));
        }

        foreach (var saleReturn in saleReturns)
        {
            // Both settlement types offset what the customer owed at sale time by the same amount;
            // RefundAmount vs CustomerCreditAmount is purely informational (see BuildSaleReturnDescription).
            rows.Add(new LedgerRow(
                saleReturn.ReturnDate, saleReturn.CreationTime, ShopCustomerLedgerReferenceType.SaleReturn,
                saleReturn.Id, saleReturn.SaleReturnNumber, BuildSaleReturnDescription(saleReturn),
                0, saleReturn.GrandTotal, "Completed"));
        }

        var orderedRows = rows.OrderBy(x => x.TransactionDate).ThenBy(x => x.CreationTime).ThenBy(x => x.ReferenceNumber).ToList();
        return (orderedRows, sales, saleReturns);
    }

    private static string BuildSaleDescription(ShopSale sale, List<ShopSaleItem>? items)
    {
        var baseText = $"Sale - {sale.SaleType}";
        if (items == null || items.Count == 0) return baseText;

        var summary = string.Join(", ", items.Take(2).Select(i => $"{i.ProductNameSnapshot} x{i.Quantity:0.##}"));
        if (items.Count > 2) summary += $" +{items.Count - 2} more";
        return $"{baseText} ({summary})";
    }

    private static string BuildPaymentDescription(ShopCustomerPayment payment)
    {
        var text = $"Customer Payment - {payment.PaymentMethod}";
        if (!string.IsNullOrWhiteSpace(payment.ReferenceNumber)) text += $" (Ref: {payment.ReferenceNumber})";
        return text;
    }

    private static string BuildSaleReturnDescription(ShopSaleReturn saleReturn) => saleReturn.SettlementType switch
    {
        ShopSaleReturnSettlementType.CashRefund => "Sale Return - Cash Refund",
        ShopSaleReturnSettlementType.Exchange => "Sale Return - Exchange",
        _ => "Sale Return - Customer Credit"
    };

    private static string? CombineAddress(string? line1, string? line2, string? city, string? state, string? postalCode, string? country)
    {
        var parts = new[] { line1, line2, city, state, postalCode, country }.Where(x => !string.IsNullOrWhiteSpace(x));
        var combined = string.Join(", ", parts);
        return string.IsNullOrWhiteSpace(combined) ? null : combined;
    }

    private async Task HideLedgerAmountsIfNotAllowedAsync(ShopCustomerLedgerDto dto)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopCustomerLedger.ViewAmounts)) return;

        dto.OpeningBalance = null;
        dto.TotalSales = null;
        dto.TotalInitialPaid = null;
        dto.TotalAdditionalPayments = null;
        dto.TotalDebit = null;
        dto.TotalCredit = null;
        dto.ClosingBalance = null;
        dto.ReceivableAmount = null;
        dto.AdvanceAmount = null;
        dto.TotalSaleReturns = null;
        dto.TotalRefunds = null;
        dto.TotalCustomerCredits = null;
        foreach (var entry in dto.Entries)
        {
            entry.DebitAmount = null;
            entry.CreditAmount = null;
            entry.RunningBalance = null;
        }
    }

    private async Task HideSummaryAmountsIfNotAllowedAsync(ShopCustomerBalanceSummaryDto dto)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopCustomerLedger.ViewAmounts)) return;

        dto.OpeningBalance = null;
        dto.TotalCompletedSales = null;
        dto.TotalInitialPaidAtSale = null;
        dto.TotalPostedCustomerPayments = null;
        dto.CurrentBalance = null;
        dto.ReceivableAmount = null;
        dto.AdvanceAmount = null;
    }

    private async Task HideStatementAmountsIfNotAllowedAsync(ShopCustomerStatementDto dto)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopCustomerLedger.ViewAmounts)) return;

        dto.OpeningBalance = null;
        dto.TotalDebit = null;
        dto.TotalCredit = null;
        dto.ClosingBalance = null;
        dto.ReceivableAmount = null;
        dto.AdvanceAmount = null;
        dto.TotalSaleReturns = null;
        dto.TotalRefunds = null;
        dto.TotalCustomerCredits = null;
        foreach (var entry in dto.Entries)
        {
            entry.DebitAmount = null;
            entry.CreditAmount = null;
            entry.RunningBalance = null;
        }
    }

    private async Task<ShopCustomer> GetCustomerAsync(Guid customerId, Guid tenantId)
    {
        var query = await _customerRepository.GetQueryableAsync();
        var customer = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == customerId && x.TenantId == tenantId));
        return customer ?? throw new BusinessException("ShopManagement:CustomerLedgerNotFound");
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private sealed record LedgerRow(
        DateTime TransactionDate,
        DateTime CreationTime,
        ShopCustomerLedgerReferenceType ReferenceType,
        Guid ReferenceId,
        string ReferenceNumber,
        string Description,
        decimal Debit,
        decimal Credit,
        string Status);
}
