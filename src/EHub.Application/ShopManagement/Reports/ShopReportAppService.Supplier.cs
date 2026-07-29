using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.PurchaseReturns;
using EHub.ShopManagement.SupplierLedger;
using EHub.ShopManagement.SupplierPayments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Content;

namespace EHub.ShopManagement.Reports;

public partial class ShopReportAppService
{
    // ------------------------------------------------------------------
    // 8. Supplier Payables Report (current balances, mirrors ShopSupplierLedgerAppService's formula)
    // ------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopReports.SupplierPayables)]
    public async Task<ShopSupplierPayableReportResultDto> GetSupplierPayablesReportAsync(GetShopSupplierPayablesReportInput input)
    {
        var allItems = await BuildSupplierPayableItemsAsync(input);
        var filtered = ApplySupplierPayableFilters(allItems, input);

        var totals = ComputeSupplierPayableTotals(filtered);
        var sorted = SortSupplierPayables(filtered, input.Sorting);
        var page = sorted.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

        return new ShopSupplierPayableReportResultDto { Items = page, TotalCount = filtered.Count, Totals = totals };
    }

    [Authorize(EHubPermissions.ShopReports.SupplierPayables), Authorize(EHubPermissions.ShopReports.Export)]
    public async Task<IRemoteStreamContent> ExportSupplierPayablesReportAsync(GetShopSupplierPayablesReportInput input, ShopReportExportFormat format)
    {
        var tenantId = RequireTenant();
        var allItems = await BuildSupplierPayableItemsAsync(input);
        var filtered = ApplySupplierPayableFilters(allItems, input);
        EnsureWithinExportLimit(filtered.Count);

        var sorted = SortSupplierPayables(filtered, input.Sorting);
        var totals = ComputeSupplierPayableTotals(sorted);
        var setting = await GetSettingAsync(tenantId);

        var request = new ShopReportExportRequest
        {
            ReportTitle = "Supplier Payables Report",
            ShopName = setting?.ShopDisplayName,
            ShopAddress = CombineAddress(setting),
            ShopPhone = setting?.Phone,
            GeneratedDate = Clock.Now,
            Columns = new List<ShopReportExportColumn>
            {
                new("Supplier", "supplierName"), new("Phone", "phone"), new("Net Purchases", "netPurchases"),
                new("Paid", "totalPaid"), new("Advance", "supplierAdvance"), new("Outstanding", "outstandingBalance"),
                new("Last Purchase", "lastPurchaseDate"), new("Last Payment", "lastPaymentDate"),
            },
            Rows = sorted.Select(x => new Dictionary<string, object?>
            {
                ["supplierName"] = x.SupplierName, ["phone"] = x.Phone, ["netPurchases"] = x.NetPurchases,
                ["totalPaid"] = x.TotalPaid, ["supplierAdvance"] = x.SupplierAdvance, ["outstandingBalance"] = x.OutstandingBalance,
                ["lastPurchaseDate"] = x.LastPurchaseDate, ["lastPaymentDate"] = x.LastPaymentDate,
            }).ToList(),
            TotalsLines = new List<(string, string)>
            {
                ("Suppliers", totals.SupplierCount.ToString()), ("With Payables", totals.SuppliersWithPayables.ToString()),
                ("Total Payables", totals.TotalPayables.ToString("N2")), ("Total Advance", totals.TotalSupplierAdvance.ToString("N2")),
            },
        };

        return await _exportService.ExportAsync(request, format);
    }

    private async Task<List<ShopSupplierPayableReportItemDto>> BuildSupplierPayableItemsAsync(GetShopSupplierPayablesReportInput input)
    {
        var tenantId = RequireTenant();

        var supplierQuery = (await _supplierRepository.GetQueryableAsync()).AsNoTracking().Where(x => x.TenantId == tenantId);
        if (input.SupplierId.HasValue) supplierQuery = supplierQuery.Where(x => x.Id == input.SupplierId.Value);
        if (!input.IncludeInactiveSuppliers) supplierQuery = supplierQuery.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var term = input.Filter;
            supplierQuery = supplierQuery.Where(x => x.Name.Contains(term) || x.Code.Contains(term) || (x.Phone != null && x.Phone.Contains(term)));
        }

        var suppliers = await AsyncExecuter.ToListAsync(supplierQuery);
        if (suppliers.Count == 0) return new List<ShopSupplierPayableReportItemDto>();

        var grQuery = (await _goodsReceiptRepository.GetQueryableAsync()).AsNoTracking();
        var grAgg = await AsyncExecuter.ToListAsync(
            grQuery.Where(x => x.TenantId == tenantId && x.Status == ShopGoodsReceiptStatus.Completed)
                .GroupBy(x => x.SupplierId)
                .Select(g => new { SupplierId = g.Key, Debit = g.Sum(x => x.GrandTotal), LastDate = g.Max(x => (DateTime?)x.ReceiptDate) }));
        var grDict = grAgg.ToDictionary(x => x.SupplierId);

        var paymentQuery = (await _supplierPaymentRepository.GetQueryableAsync()).AsNoTracking();
        var paymentAgg = await AsyncExecuter.ToListAsync(
            paymentQuery.Where(x => x.TenantId == tenantId && x.Status == ShopSupplierPaymentStatus.Posted)
                .GroupBy(x => x.SupplierId)
                .Select(g => new { SupplierId = g.Key, Credit = g.Sum(x => x.Amount), LastDate = g.Max(x => (DateTime?)x.PaymentDate) }));
        var paymentDict = paymentAgg.ToDictionary(x => x.SupplierId);

        var returnQuery = (await _purchaseReturnRepository.GetQueryableAsync()).AsNoTracking();
        var returnAgg = await AsyncExecuter.ToListAsync(
            returnQuery.Where(x => x.TenantId == tenantId && x.Status == ShopPurchaseReturnStatus.Completed)
                .GroupBy(x => x.SupplierId)
                .Select(g => new { SupplierId = g.Key, Credit = g.Sum(x => x.GrandTotal), LastDate = g.Max(x => (DateTime?)x.ReturnDate) }));
        var returnDict = returnAgg.ToDictionary(x => x.SupplierId);

        return suppliers.Select(supplier =>
        {
            grDict.TryGetValue(supplier.Id, out var gr);
            paymentDict.TryGetValue(supplier.Id, out var payment);
            returnDict.TryGetValue(supplier.Id, out var ret);

            var purchases = gr?.Debit ?? 0;
            var returns = ret?.Credit ?? 0;
            var credit = (payment?.Credit ?? 0) + returns;
            var balance = Math.Round(supplier.OpeningBalance + purchases - credit, 2);

            var lastTransaction = new[] { gr?.LastDate, payment?.LastDate, ret?.LastDate }.Where(x => x.HasValue).Select(x => x!.Value).DefaultIfEmpty().Max();

            return new ShopSupplierPayableReportItemDto
            {
                SupplierId = supplier.Id,
                SupplierCode = supplier.Code,
                SupplierName = supplier.Name,
                Phone = supplier.Phone,
                TotalPurchases = purchases,
                PurchaseReturns = returns,
                NetPurchases = Math.Round(purchases - returns, 2),
                TotalPaid = payment?.Credit ?? 0,
                SupplierAdvance = Math.Max(-balance, 0),
                OutstandingBalance = Math.Max(balance, 0),
                LastPurchaseDate = gr?.LastDate,
                LastPaymentDate = payment?.LastDate,
                LastTransactionDate = lastTransaction == default ? null : lastTransaction,
                IsActive = supplier.IsActive,
            };
        }).ToList();
    }

    private static List<ShopSupplierPayableReportItemDto> ApplySupplierPayableFilters(List<ShopSupplierPayableReportItemDto> items, GetShopSupplierPayablesReportInput input)
    {
        IEnumerable<ShopSupplierPayableReportItemDto> result = items;
        if (input.HasOutstandingBalance == true) result = result.Where(x => x.OutstandingBalance > 0);
        if (input.HasOutstandingBalance == false) result = result.Where(x => x.OutstandingBalance <= 0);
        if (input.HasAdvanceBalance == true) result = result.Where(x => x.SupplierAdvance > 0);
        if (input.HasAdvanceBalance == false) result = result.Where(x => x.SupplierAdvance <= 0);
        if (input.MinimumBalance.HasValue) result = result.Where(x => x.OutstandingBalance >= input.MinimumBalance.Value);
        if (input.MaximumBalance.HasValue) result = result.Where(x => x.OutstandingBalance <= input.MaximumBalance.Value);
        if (input.LastTransactionFrom.HasValue) result = result.Where(x => x.LastTransactionDate.HasValue && x.LastTransactionDate.Value.Date >= input.LastTransactionFrom.Value.Date);
        if (input.LastTransactionTo.HasValue) result = result.Where(x => x.LastTransactionDate.HasValue && x.LastTransactionDate.Value.Date <= input.LastTransactionTo.Value.Date);
        return result.ToList();
    }

    private static List<ShopSupplierPayableReportItemDto> SortSupplierPayables(List<ShopSupplierPayableReportItemDto> items, string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting)) return items.OrderByDescending(x => x.OutstandingBalance).ToList();
        return sorting.Trim().ToLowerInvariant() switch
        {
            "outstandingbalance" => items.OrderBy(x => x.OutstandingBalance).ToList(),
            "outstandingbalance desc" => items.OrderByDescending(x => x.OutstandingBalance).ToList(),
            "suppliername" => items.OrderBy(x => x.SupplierName).ToList(),
            "suppliername desc" => items.OrderByDescending(x => x.SupplierName).ToList(),
            _ => items.OrderByDescending(x => x.OutstandingBalance).ToList(),
        };
    }

    private static ShopSupplierPayableReportTotalsDto ComputeSupplierPayableTotals(List<ShopSupplierPayableReportItemDto> items) => new()
    {
        SupplierCount = items.Count,
        SuppliersWithPayables = items.Count(x => x.OutstandingBalance > 0),
        SuppliersWithAdvance = items.Count(x => x.SupplierAdvance > 0),
        TotalPurchases = items.Sum(x => x.TotalPurchases),
        TotalPurchaseReturns = items.Sum(x => x.PurchaseReturns),
        TotalPaid = items.Sum(x => x.TotalPaid),
        TotalPayables = items.Sum(x => x.OutstandingBalance),
        TotalSupplierAdvance = items.Sum(x => x.SupplierAdvance),
    };

    // ------------------------------------------------------------------
    // 9. Supplier Transaction Report (single supplier's full ledger)
    // ------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopReports.SupplierTransactions)]
    public async Task<ShopSupplierTransactionReportResultDto> GetSupplierTransactionReportAsync(GetShopSupplierTransactionReportInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);

        var supplierQuery = (await _supplierRepository.GetQueryableAsync()).AsNoTracking();
        var supplier = await AsyncExecuter.FirstOrDefaultAsync(supplierQuery.Where(x => x.TenantId == tenantId && x.Id == input.SupplierId));
        if (supplier == null) return new ShopSupplierTransactionReportResultDto();

        var allRows = await BuildSupplierLedgerRowsAsync(tenantId, input.SupplierId);

        if (input.TransactionType.HasValue) allRows = allRows.Where(x => x.TransactionType == input.TransactionType.Value).ToList();
        if (!string.IsNullOrWhiteSpace(input.ReferenceNumber)) allRows = allRows.Where(x => x.ReferenceNumber.Contains(input.ReferenceNumber, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(input.Filter)) allRows = allRows.Where(x => x.Description.Contains(input.Filter, StringComparison.OrdinalIgnoreCase) || x.ReferenceNumber.Contains(input.Filter, StringComparison.OrdinalIgnoreCase)).ToList();

        var beforeRange = allRows.Where(x => x.TransactionDate.Date < range.From.Date).ToList();
        var openingBalance = Math.Round(supplier.OpeningBalance + beforeRange.Sum(x => x.Debit - x.Credit), 2);

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
        var page = inRange.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

        return new ShopSupplierTransactionReportResultDto
        {
            Items = page,
            TotalCount = inRange.Count,
            Totals = new ShopSupplierTransactionReportTotalsDto
            {
                OpeningBalance = openingBalance,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                ClosingBalance = Math.Round(openingBalance + totalDebit - totalCredit, 2),
            },
        };
    }

    private async Task<List<ShopSupplierTransactionReportItemDto>> BuildSupplierLedgerRowsAsync(Guid tenantId, Guid supplierId)
    {
        var rows = new List<ShopSupplierTransactionReportItemDto>();

        var grQuery = (await _goodsReceiptRepository.GetQueryableAsync()).AsNoTracking();
        var receipts = await AsyncExecuter.ToListAsync(grQuery.Where(x => x.TenantId == tenantId && x.SupplierId == supplierId && x.Status == ShopGoodsReceiptStatus.Completed));
        foreach (var gr in receipts)
        {
            rows.Add(new ShopSupplierTransactionReportItemDto
            {
                SupplierLedgerId = gr.Id, TransactionDate = gr.ReceiptDate, SupplierId = supplierId,
                TransactionType = ShopSupplierLedgerReferenceType.GoodsReceipt, ReferenceId = gr.Id, ReferenceNumber = gr.GoodsReceiptNumber,
                Debit = gr.GrandTotal, Credit = 0, Description = $"Goods Receipt - {gr.GoodsReceiptNumber}", CreationTime = gr.CreationTime,
            });
        }

        var paymentQuery = (await _supplierPaymentRepository.GetQueryableAsync()).AsNoTracking();
        var payments = await AsyncExecuter.ToListAsync(paymentQuery.Where(x => x.TenantId == tenantId && x.SupplierId == supplierId && x.Status == ShopSupplierPaymentStatus.Posted));
        foreach (var payment in payments)
        {
            rows.Add(new ShopSupplierTransactionReportItemDto
            {
                SupplierLedgerId = payment.Id, TransactionDate = payment.PaymentDate, SupplierId = supplierId,
                TransactionType = ShopSupplierLedgerReferenceType.SupplierPayment, ReferenceId = payment.Id, ReferenceNumber = payment.PaymentNumber,
                Debit = 0, Credit = payment.Amount, Description = $"Supplier Payment - {payment.PaymentMethod}", CreationTime = payment.CreationTime,
            });
        }

        var returnQuery = (await _purchaseReturnRepository.GetQueryableAsync()).AsNoTracking();
        var returns = await AsyncExecuter.ToListAsync(returnQuery.Where(x => x.TenantId == tenantId && x.SupplierId == supplierId && x.Status == ShopPurchaseReturnStatus.Completed));
        foreach (var ret in returns)
        {
            rows.Add(new ShopSupplierTransactionReportItemDto
            {
                SupplierLedgerId = ret.Id, TransactionDate = ret.ReturnDate, SupplierId = supplierId,
                TransactionType = ShopSupplierLedgerReferenceType.PurchaseReturn, ReferenceId = ret.Id, ReferenceNumber = ret.PurchaseReturnNumber,
                Debit = 0, Credit = ret.GrandTotal, Description = "Purchase Return", CreationTime = ret.CreationTime,
            });
        }

        var supplierQuery = (await _supplierRepository.GetQueryableAsync()).AsNoTracking();
        var supplier = await AsyncExecuter.FirstOrDefaultAsync(supplierQuery.Where(x => x.Id == supplierId));
        if (supplier != null) foreach (var row in rows) row.SupplierName = supplier.Name;

        return rows.OrderBy(x => x.TransactionDate).ThenBy(x => x.CreationTime).ToList();
    }
}
