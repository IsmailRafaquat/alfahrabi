using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.CashRegisters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Content;

namespace EHub.ShopManagement.Reports;

public partial class ShopReportAppService
{
    // ------------------------------------------------------------------
    // 11. Cash Report
    // ------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopReports.Cash)]
    public async Task<ShopCashReportResultDto> GetCashReportAsync(GetShopCashReportInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);

        var (items, totals) = await BuildCashReportItemsAndTotalsAsync(tenantId, range, input);
        var sorted = SortCashItems(items, input.Sorting);
        var page = sorted.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

        var dailySummaries = await BuildCashDailySummariesAsync(tenantId, range, input.CashRegisterId);

        return new ShopCashReportResultDto { Items = page, TotalCount = sorted.Count, Totals = totals, DailySummaries = dailySummaries };
    }

    [Authorize(EHubPermissions.ShopReports.Cash), Authorize(EHubPermissions.ShopReports.Export)]
    public async Task<IRemoteStreamContent> ExportCashReportAsync(GetShopCashReportInput input, ShopReportExportFormat format)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);

        var (items, totals) = await BuildCashReportItemsAndTotalsAsync(tenantId, range, input);
        EnsureWithinExportLimit(items.Count);
        var sorted = SortCashItems(items, input.Sorting);

        var setting = await GetSettingAsync(tenantId);
        var request = new ShopReportExportRequest
        {
            ReportTitle = "Cash Report",
            ShopName = setting?.ShopDisplayName,
            ShopAddress = CombineAddress(setting),
            ShopPhone = setting?.Phone,
            DateFrom = range.From,
            DateTo = range.ToExclusive.AddDays(-1),
            GeneratedDate = Clock.Now,
            Columns = new List<ShopReportExportColumn>
            {
                new("Date", "transactionDate"), new("Register", "cashRegisterName"), new("Type", "transactionType"),
                new("Reference", "referenceNumber"), new("Cash In", "cashIn"), new("Cash Out", "cashOut"),
                new("Balance", "balanceAfterTransaction"), new("Description", "description"),
            },
            Rows = sorted.Select(x => new Dictionary<string, object?>
            {
                ["transactionDate"] = x.TransactionDate, ["cashRegisterName"] = x.CashRegisterName, ["transactionType"] = x.TransactionType.ToString(),
                ["referenceNumber"] = x.ReferenceNumber, ["cashIn"] = x.CashIn, ["cashOut"] = x.CashOut,
                ["balanceAfterTransaction"] = x.BalanceAfterTransaction, ["description"] = x.Description,
            }).ToList(),
            TotalsLines = new List<(string, string)>
            {
                ("Opening Cash", totals.OpeningCash.ToString("N2")), ("Total Cash In", totals.TotalCashIn.ToString("N2")),
                ("Total Cash Out", totals.TotalCashOut.ToString("N2")), ("Expected Closing Cash", totals.ExpectedClosingCash.ToString("N2")),
            },
        };

        return await _exportService.ExportAsync(request, format);
    }

    private async Task<(List<ShopCashReportItemDto> Items, ShopCashReportTotalsDto Totals)> BuildCashReportItemsAndTotalsAsync(
        Guid tenantId, ShopReportDateRange range, GetShopCashReportInput input)
    {
        var txQuery = (await _cashTransactionRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (input.CashRegisterId.HasValue) txQuery = txQuery.Where(x => x.CashRegisterId == input.CashRegisterId.Value);
        if (input.TransactionType.HasValue) txQuery = txQuery.Where(x => x.TransactionType == input.TransactionType.Value);
        if (input.ReferenceType.HasValue) txQuery = txQuery.Where(x => x.ReferenceType == input.ReferenceType.Value);
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var term = input.Filter;
            txQuery = txQuery.Where(x => x.ReferenceNumber.Contains(term) || (x.Description != null && x.Description.Contains(term)));
        }

        var inRangeQuery = txQuery.Where(x => x.TransactionDate >= range.From && x.TransactionDate < range.ToExclusive);
        var inRangeRows = await AsyncExecuter.ToListAsync(
            inRangeQuery.OrderBy(x => x.CashRegisterId).ThenBy(x => x.TransactionDate).ThenBy(x => x.CreationTime));

        if (inRangeRows.Count == 0)
        {
            return (new List<ShopCashReportItemDto>(), new ShopCashReportTotalsDto());
        }

        var registerIds = inRangeRows.Select(x => x.CashRegisterId).Distinct().ToList();

        var beforeRangeAgg = await AsyncExecuter.ToListAsync(
            txQuery.Where(x => x.TransactionDate < range.From && registerIds.Contains(x.CashRegisterId))
                .GroupBy(x => x.CashRegisterId)
                .Select(g => new { CashRegisterId = g.Key, Balance = g.Sum(x => x.Direction == ShopCashDirection.In ? x.Amount : -x.Amount) }));
        var openingBalances = beforeRangeAgg.ToDictionary(x => x.CashRegisterId, x => x.Balance);

        var registerQuery = (await _cashRegisterRepository.GetQueryableAsync()).AsNoTracking();
        var registers = await AsyncExecuter.ToListAsync(registerQuery.Where(x => registerIds.Contains(x.Id)).Select(x => new { x.Id, x.Name }));
        var registerNames = registers.ToDictionary(x => x.Id, x => x.Name);

        var userNames = await GetUserNamesAsync(inRangeRows.Where(x => x.CreatedByUserId.HasValue).Select(x => x.CreatedByUserId!.Value));

        var items = new List<ShopCashReportItemDto>();
        decimal totalIn = 0, totalOut = 0;

        foreach (var group in inRangeRows.GroupBy(x => x.CashRegisterId))
        {
            var running = openingBalances.GetValueOrDefault(group.Key, 0);
            foreach (var tx in group)
            {
                var cashIn = tx.Direction == ShopCashDirection.In ? tx.Amount : 0;
                var cashOut = tx.Direction == ShopCashDirection.Out ? tx.Amount : 0;
                running = Math.Round(running + cashIn - cashOut, 2);
                totalIn += cashIn;
                totalOut += cashOut;

                items.Add(new ShopCashReportItemDto
                {
                    CashTransactionId = tx.Id,
                    TransactionDate = tx.TransactionDate,
                    CashRegisterId = tx.CashRegisterId,
                    CashRegisterName = registerNames.GetValueOrDefault(tx.CashRegisterId, string.Empty),
                    TransactionType = tx.TransactionType,
                    ReferenceType = tx.ReferenceType,
                    ReferenceId = tx.ReferenceId,
                    ReferenceNumber = tx.ReferenceNumber,
                    CashIn = cashIn,
                    CashOut = cashOut,
                    BalanceAfterTransaction = running,
                    Description = tx.Description,
                    CreatedByUserName = tx.CreatedByUserId.HasValue ? userNames.GetValueOrDefault(tx.CreatedByUserId.Value) : null,
                    CreationTime = tx.CreationTime,
                });
            }
        }

        var openingCash = registerIds.Sum(id => openingBalances.GetValueOrDefault(id, 0));

        decimal? actualClosingCash = null;
        decimal? difference = null;
        if (input.CashRegisterId.HasValue)
        {
            var closingQuery = (await _cashClosingRepository.GetQueryableAsync()).AsNoTracking();
            var lastClosing = await AsyncExecuter.FirstOrDefaultAsync(
                closingQuery.Where(x => x.TenantId == tenantId && x.CashRegisterId == input.CashRegisterId.Value
                    && x.Status == ShopCashClosingStatus.Closed
                    && x.BusinessDate >= range.From && x.BusinessDate < range.ToExclusive)
                    .OrderByDescending(x => x.BusinessDate));
            actualClosingCash = lastClosing?.ActualClosingCash;
            difference = lastClosing?.DifferenceAmount;
        }

        var totals = new ShopCashReportTotalsDto
        {
            OpeningCash = openingCash,
            TotalCashIn = totalIn,
            TotalCashOut = totalOut,
            ExpectedClosingCash = Math.Round(openingCash + totalIn - totalOut, 2),
            ActualClosingCash = actualClosingCash,
            Difference = difference,
        };

        return (items, totals);
    }

    private static List<ShopCashReportItemDto> SortCashItems(List<ShopCashReportItemDto> items, string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting)) return items.OrderByDescending(x => x.TransactionDate).ToList();
        return sorting.Trim().ToLowerInvariant() switch
        {
            "transactiondate" => items.OrderBy(x => x.TransactionDate).ToList(),
            "transactiondate desc" => items.OrderByDescending(x => x.TransactionDate).ToList(),
            _ => items.OrderByDescending(x => x.TransactionDate).ToList(),
        };
    }

    private async Task<List<ShopCashDailySummaryDto>> BuildCashDailySummariesAsync(Guid tenantId, ShopReportDateRange range, Guid? cashRegisterId)
    {
        var closingQuery = (await _cashClosingRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status != ShopCashClosingStatus.Cancelled
                && x.BusinessDate >= range.From && x.BusinessDate < range.ToExclusive);

        if (cashRegisterId.HasValue) closingQuery = closingQuery.Where(x => x.CashRegisterId == cashRegisterId.Value);

        var closings = await AsyncExecuter.ToListAsync(closingQuery.OrderBy(x => x.BusinessDate));
        if (closings.Count == 0) return new List<ShopCashDailySummaryDto>();

        var registerIds = closings.Select(x => x.CashRegisterId).Distinct().ToList();
        var registerQuery = (await _cashRegisterRepository.GetQueryableAsync()).AsNoTracking();
        var registerNames = (await AsyncExecuter.ToListAsync(registerQuery.Where(x => registerIds.Contains(x.Id)).Select(x => new { x.Id, x.Name })))
            .ToDictionary(x => x.Id, x => x.Name);

        return closings.Select(c => new ShopCashDailySummaryDto
        {
            BusinessDate = c.BusinessDate,
            CashRegisterId = c.CashRegisterId,
            CashRegisterName = registerNames.GetValueOrDefault(c.CashRegisterId, string.Empty),
            OpeningCash = c.OpeningCash,
            TotalCashIn = c.CashSales + c.CustomerCashPayments + c.ManualCashIn,
            TotalCashOut = c.SupplierCashPayments + c.CashExpenses + c.CustomerRefunds + c.ManualCashOut,
            ExpectedClosingCash = c.ExpectedClosingCash,
            ActualClosingCash = c.ActualClosingCash,
            Difference = c.DifferenceAmount,
        }).ToList();
    }
}
