using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.CustomerLedger;
using EHub.ShopManagement.CustomerPayments;
using EHub.ShopManagement.SaleReturns;
using EHub.ShopManagement.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Content;

namespace EHub.ShopManagement.Reports;

public partial class ShopReportAppService
{
    // ------------------------------------------------------------------
    // 6. Customer Receivables Report (current balances - not date-scoped, mirrors
    //    ShopCustomerLedgerAppService's exact formula, computed for all customers at once)
    // ------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopReports.CustomerReceivables)]
    public async Task<ShopCustomerReceivableReportResultDto> GetCustomerReceivablesReportAsync(GetShopCustomerReceivablesReportInput input)
    {
        var allItems = await BuildCustomerReceivableItemsAsync(input);
        var filtered = ApplyCustomerReceivableFilters(allItems, input);

        var totals = ComputeCustomerReceivableTotals(filtered);
        var sorted = SortCustomerReceivables(filtered, input.Sorting);
        var page = sorted.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

        return new ShopCustomerReceivableReportResultDto { Items = page, TotalCount = filtered.Count, Totals = totals };
    }

    [Authorize(EHubPermissions.ShopReports.CustomerReceivables), Authorize(EHubPermissions.ShopReports.Export)]
    public async Task<IRemoteStreamContent> ExportCustomerReceivablesReportAsync(GetShopCustomerReceivablesReportInput input, ShopReportExportFormat format)
    {
        var tenantId = RequireTenant();
        var allItems = await BuildCustomerReceivableItemsAsync(input);
        var filtered = ApplyCustomerReceivableFilters(allItems, input);
        EnsureWithinExportLimit(filtered.Count);

        var sorted = SortCustomerReceivables(filtered, input.Sorting);
        var totals = ComputeCustomerReceivableTotals(sorted);
        var setting = await GetSettingAsync(tenantId);

        var request = new ShopReportExportRequest
        {
            ReportTitle = "Customer Receivables Report",
            ShopName = setting?.ShopDisplayName,
            ShopAddress = CombineAddress(setting),
            ShopPhone = setting?.Phone,
            GeneratedDate = Clock.Now,
            Columns = new List<ShopReportExportColumn>
            {
                new("Customer", "customerName"), new("Phone", "phone"), new("Net Sales", "netSales"),
                new("Received", "totalReceived"), new("Advance", "customerAdvance"), new("Outstanding", "outstandingBalance"),
                new("Last Sale", "lastSaleDate"), new("Last Payment", "lastPaymentDate"),
            },
            Rows = sorted.Select(x => new Dictionary<string, object?>
            {
                ["customerName"] = x.CustomerName, ["phone"] = x.Phone, ["netSales"] = x.NetSales,
                ["totalReceived"] = x.TotalReceived, ["customerAdvance"] = x.CustomerAdvance, ["outstandingBalance"] = x.OutstandingBalance,
                ["lastSaleDate"] = x.LastSaleDate, ["lastPaymentDate"] = x.LastPaymentDate,
            }).ToList(),
            TotalsLines = new List<(string, string)>
            {
                ("Customers", totals.CustomerCount.ToString()), ("With Receivables", totals.CustomersWithReceivables.ToString()),
                ("Total Receivables", totals.TotalReceivables.ToString("N2")), ("Total Advance", totals.TotalCustomerAdvance.ToString("N2")),
            },
        };

        return await _exportService.ExportAsync(request, format);
    }

    private async Task<List<ShopCustomerReceivableReportItemDto>> BuildCustomerReceivableItemsAsync(GetShopCustomerReceivablesReportInput input)
    {
        var tenantId = RequireTenant();

        var customerQuery = (await _customerRepository.GetQueryableAsync()).AsNoTracking().Where(x => x.TenantId == tenantId);
        if (input.CustomerId.HasValue) customerQuery = customerQuery.Where(x => x.Id == input.CustomerId.Value);
        if (!input.IncludeInactiveCustomers) customerQuery = customerQuery.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var term = input.Filter;
            customerQuery = customerQuery.Where(x => x.Name.Contains(term) || x.Code.Contains(term) || (x.Phone != null && x.Phone.Contains(term)));
        }

        var customers = await AsyncExecuter.ToListAsync(customerQuery);
        if (customers.Count == 0) return new List<ShopCustomerReceivableReportItemDto>();

        var saleQuery = (await _saleRepository.GetQueryableAsync()).AsNoTracking();
        var saleAgg = await AsyncExecuter.ToListAsync(
            saleQuery.Where(x => x.TenantId == tenantId && x.Status == ShopSaleStatus.Completed)
                .GroupBy(x => x.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    Gross = g.Sum(x => x.GrandTotal),
                    Debit = g.Sum(x => (x.GrandTotal - x.PaidAmount) > 0 ? (x.GrandTotal - x.PaidAmount) : 0),
                    LastDate = g.Max(x => (DateTime?)x.SaleDate),
                }));
        var saleDict = saleAgg.ToDictionary(x => x.CustomerId);

        var paymentQuery = (await _customerPaymentRepository.GetQueryableAsync()).AsNoTracking();
        var paymentAgg = await AsyncExecuter.ToListAsync(
            paymentQuery.Where(x => x.TenantId == tenantId && x.Status == ShopCustomerPaymentStatus.Posted)
                .GroupBy(x => x.CustomerId)
                .Select(g => new { CustomerId = g.Key, Credit = g.Sum(x => x.Amount), LastDate = g.Max(x => (DateTime?)x.PaymentDate) }));
        var paymentDict = paymentAgg.ToDictionary(x => x.CustomerId);

        var returnQuery = (await _saleReturnRepository.GetQueryableAsync()).AsNoTracking();
        var returnAgg = await AsyncExecuter.ToListAsync(
            returnQuery.Where(x => x.TenantId == tenantId && x.Status == ShopSaleReturnStatus.Completed)
                .GroupBy(x => x.CustomerId)
                .Select(g => new { CustomerId = g.Key, Credit = g.Sum(x => x.GrandTotal), LastDate = g.Max(x => (DateTime?)x.ReturnDate) }));
        var returnDict = returnAgg.ToDictionary(x => x.CustomerId);

        return customers.Select(customer =>
        {
            saleDict.TryGetValue(customer.Id, out var sale);
            paymentDict.TryGetValue(customer.Id, out var payment);
            returnDict.TryGetValue(customer.Id, out var ret);

            var gross = sale?.Gross ?? 0;
            var returns = ret?.Credit ?? 0;
            var debit = sale?.Debit ?? 0;
            var credit = (payment?.Credit ?? 0) + returns;
            var balance = Math.Round(customer.OpeningBalance + debit - credit, 2);

            var lastTransaction = new[] { sale?.LastDate, payment?.LastDate, ret?.LastDate }.Where(x => x.HasValue).Select(x => x!.Value).DefaultIfEmpty().Max();

            return new ShopCustomerReceivableReportItemDto
            {
                CustomerId = customer.Id,
                CustomerCode = customer.Code,
                CustomerName = customer.Name,
                Phone = customer.Phone,
                TotalSales = gross,
                SaleReturns = returns,
                NetSales = Math.Round(gross - returns, 2),
                TotalReceived = payment?.Credit ?? 0,
                CustomerAdvance = Math.Max(-balance, 0),
                OutstandingBalance = Math.Max(balance, 0),
                LastSaleDate = sale?.LastDate,
                LastPaymentDate = payment?.LastDate,
                LastTransactionDate = lastTransaction == default ? null : lastTransaction,
                IsActive = customer.IsActive,
            };
        }).ToList();
    }

    private static List<ShopCustomerReceivableReportItemDto> ApplyCustomerReceivableFilters(List<ShopCustomerReceivableReportItemDto> items, GetShopCustomerReceivablesReportInput input)
    {
        IEnumerable<ShopCustomerReceivableReportItemDto> result = items;
        if (input.HasOutstandingBalance == true) result = result.Where(x => x.OutstandingBalance > 0);
        if (input.HasOutstandingBalance == false) result = result.Where(x => x.OutstandingBalance <= 0);
        if (input.HasAdvanceBalance == true) result = result.Where(x => x.CustomerAdvance > 0);
        if (input.HasAdvanceBalance == false) result = result.Where(x => x.CustomerAdvance <= 0);
        if (input.MinimumBalance.HasValue) result = result.Where(x => x.OutstandingBalance >= input.MinimumBalance.Value);
        if (input.MaximumBalance.HasValue) result = result.Where(x => x.OutstandingBalance <= input.MaximumBalance.Value);
        if (input.LastTransactionFrom.HasValue) result = result.Where(x => x.LastTransactionDate.HasValue && x.LastTransactionDate.Value.Date >= input.LastTransactionFrom.Value.Date);
        if (input.LastTransactionTo.HasValue) result = result.Where(x => x.LastTransactionDate.HasValue && x.LastTransactionDate.Value.Date <= input.LastTransactionTo.Value.Date);
        return result.ToList();
    }

    private static List<ShopCustomerReceivableReportItemDto> SortCustomerReceivables(List<ShopCustomerReceivableReportItemDto> items, string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting)) return items.OrderByDescending(x => x.OutstandingBalance).ToList();
        return sorting.Trim().ToLowerInvariant() switch
        {
            "outstandingbalance" => items.OrderBy(x => x.OutstandingBalance).ToList(),
            "outstandingbalance desc" => items.OrderByDescending(x => x.OutstandingBalance).ToList(),
            "customername" => items.OrderBy(x => x.CustomerName).ToList(),
            "customername desc" => items.OrderByDescending(x => x.CustomerName).ToList(),
            _ => items.OrderByDescending(x => x.OutstandingBalance).ToList(),
        };
    }

    private static ShopCustomerReceivableReportTotalsDto ComputeCustomerReceivableTotals(List<ShopCustomerReceivableReportItemDto> items) => new()
    {
        CustomerCount = items.Count,
        CustomersWithReceivables = items.Count(x => x.OutstandingBalance > 0),
        CustomersWithAdvance = items.Count(x => x.CustomerAdvance > 0),
        TotalSales = items.Sum(x => x.TotalSales),
        TotalSaleReturns = items.Sum(x => x.SaleReturns),
        TotalReceived = items.Sum(x => x.TotalReceived),
        TotalReceivables = items.Sum(x => x.OutstandingBalance),
        TotalCustomerAdvance = items.Sum(x => x.CustomerAdvance),
    };

    // ------------------------------------------------------------------
    // 7. Customer Transaction Report (single customer's full ledger, replays the same
    //    Sale/CustomerPayment/SaleReturn rows ShopCustomerLedgerAppService already builds)
    // ------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopReports.CustomerTransactions)]
    public async Task<ShopCustomerTransactionReportResultDto> GetCustomerTransactionReportAsync(GetShopCustomerTransactionReportInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);

        var customerQuery = (await _customerRepository.GetQueryableAsync()).AsNoTracking();
        var customer = await AsyncExecuter.FirstOrDefaultAsync(customerQuery.Where(x => x.TenantId == tenantId && x.Id == input.CustomerId));
        if (customer == null) return new ShopCustomerTransactionReportResultDto();

        var allRows = await BuildCustomerLedgerRowsAsync(tenantId, input.CustomerId);

        if (input.TransactionType.HasValue) allRows = allRows.Where(x => x.TransactionType == input.TransactionType.Value).ToList();
        if (!string.IsNullOrWhiteSpace(input.ReferenceNumber)) allRows = allRows.Where(x => x.ReferenceNumber.Contains(input.ReferenceNumber, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(input.Filter)) allRows = allRows.Where(x => x.Description.Contains(input.Filter, StringComparison.OrdinalIgnoreCase) || x.ReferenceNumber.Contains(input.Filter, StringComparison.OrdinalIgnoreCase)).ToList();

        var beforeRange = allRows.Where(x => x.TransactionDate.Date < range.From.Date).ToList();
        var openingBalance = Math.Round(customer.OpeningBalance + beforeRange.Sum(x => x.Debit - x.Credit), 2);

        var inRange = allRows.Where(x => x.TransactionDate >= range.From && x.TransactionDate < range.ToExclusive)
            .OrderBy(x => x.TransactionDate).ThenBy(x => x.CreationTime).ToList();

        var running = openingBalance;
        foreach (var row in inRange)
        {
            running = Math.Round(running + row.Debit - row.Credit, 2);
            row.RunningBalance = running;
        }

        var totalDebit = inRange.Sum(x => x.Debit);
        var totalCredit = inRange.Sum(x => x.Credit);

        var totalCount = inRange.Count;
        var page = inRange.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

        return new ShopCustomerTransactionReportResultDto
        {
            Items = page,
            TotalCount = totalCount,
            Totals = new ShopCustomerTransactionReportTotalsDto
            {
                OpeningBalance = openingBalance,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                ClosingBalance = Math.Round(openingBalance + totalDebit - totalCredit, 2),
            },
        };
    }

    private async Task<List<ShopCustomerTransactionReportItemDto>> BuildCustomerLedgerRowsAsync(Guid tenantId, Guid customerId)
    {
        var rows = new List<ShopCustomerTransactionReportItemDto>();

        var saleQuery = (await _saleRepository.GetQueryableAsync()).AsNoTracking();
        var sales = await AsyncExecuter.ToListAsync(saleQuery.Where(x => x.TenantId == tenantId && x.CustomerId == customerId && x.Status == ShopSaleStatus.Completed));
        foreach (var sale in sales)
        {
            var debit = Math.Round(Math.Max(0, sale.GrandTotal - sale.PaidAmount), 2);
            if (debit <= 0) continue;
            rows.Add(new ShopCustomerTransactionReportItemDto
            {
                CustomerLedgerId = sale.Id, TransactionDate = sale.SaleDate, CustomerId = customerId,
                TransactionType = ShopCustomerLedgerReferenceType.Sale, ReferenceId = sale.Id, ReferenceNumber = sale.SaleNumber,
                Debit = debit, Credit = 0, Description = $"Sale - {sale.SaleType}", CreationTime = sale.CreationTime,
            });
        }

        var paymentQuery = (await _customerPaymentRepository.GetQueryableAsync()).AsNoTracking();
        var payments = await AsyncExecuter.ToListAsync(paymentQuery.Where(x => x.TenantId == tenantId && x.CustomerId == customerId && x.Status == ShopCustomerPaymentStatus.Posted));
        foreach (var payment in payments)
        {
            rows.Add(new ShopCustomerTransactionReportItemDto
            {
                CustomerLedgerId = payment.Id, TransactionDate = payment.PaymentDate, CustomerId = customerId,
                TransactionType = ShopCustomerLedgerReferenceType.CustomerPayment, ReferenceId = payment.Id, ReferenceNumber = payment.PaymentNumber,
                Debit = 0, Credit = payment.Amount, Description = $"Customer Payment - {payment.PaymentMethod}", CreationTime = payment.CreationTime,
            });
        }

        var returnQuery = (await _saleReturnRepository.GetQueryableAsync()).AsNoTracking();
        var returns = await AsyncExecuter.ToListAsync(returnQuery.Where(x => x.TenantId == tenantId && x.CustomerId == customerId && x.Status == ShopSaleReturnStatus.Completed));
        foreach (var ret in returns)
        {
            rows.Add(new ShopCustomerTransactionReportItemDto
            {
                CustomerLedgerId = ret.Id, TransactionDate = ret.ReturnDate, CustomerId = customerId,
                TransactionType = ShopCustomerLedgerReferenceType.SaleReturn, ReferenceId = ret.Id, ReferenceNumber = ret.SaleReturnNumber,
                Debit = 0, Credit = ret.GrandTotal, Description = "Sale Return", CreationTime = ret.CreationTime,
            });
        }

        var customerQuery = (await _customerRepository.GetQueryableAsync()).AsNoTracking();
        var customer = await AsyncExecuter.FirstOrDefaultAsync(customerQuery.Where(x => x.Id == customerId));
        if (customer != null) foreach (var row in rows) row.CustomerName = customer.Name;

        return rows.OrderBy(x => x.TransactionDate).ThenBy(x => x.CreationTime).ToList();
    }
}
