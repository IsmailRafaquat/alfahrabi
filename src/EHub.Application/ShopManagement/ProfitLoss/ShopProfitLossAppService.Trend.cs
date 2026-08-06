using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Reports;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.ProfitLoss;

public partial class ShopProfitLossAppService
{
    public async Task<ListResultDto<ShopProfitLossTrendPointDto>> GetTrendAsync(GetShopProfitLossInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);
        var computation = await _calculator.ComputeDetailedAsync(tenantId, range.From, range.ToExclusive);
        var trend = await BuildTrendAsync(tenantId, input.Period, range, computation);
        return new ListResultDto<ShopProfitLossTrendPointDto>(trend);
    }

    /// <summary>
    /// Groups the already-loaded sales/returns/expenses for this period (see
    /// <see cref="ShopProfitLossCalculator.ComputeDetailedAsync"/>) by day or month bucket in
    /// memory - mirrors the existing grouping approach used by
    /// ShopReportAppService.BuildSalesGroupsAsync/BuildExpenseGroupsAsync rather than issuing one
    /// aggregate query per bucket, and reuses data the caller already fetched instead of re-querying.
    /// </summary>
    private async Task<List<ShopProfitLossTrendPointDto>> BuildTrendAsync(Guid tenantId, ShopReportPeriod period, ShopReportDateRange range, ShopProfitLossComputation computation)
    {
        var groupByMonth = period is ShopReportPeriod.ThisQuarter or ShopReportPeriod.ThisYear
            || (period == ShopReportPeriod.Custom && (range.ToExclusive - range.From).TotalDays > 60);

        var canCost = await CanAsync(EHubPermissions.ShopProfitLoss.ViewCost);
        var canExpenses = await CanAsync(EHubPermissions.ShopProfitLoss.ViewExpenses);

        var cogsBySale = computation.SaleItems
            .GroupBy(x => x.SaleId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity * x.UnitCostSnapshot));

        var cogsReversalByReturn = computation.SaleReturnItems
            .GroupBy(x => x.SaleReturnId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.ReturnQuantity * x.UnitCostSnapshot));

        string BucketKey(DateTime date) => groupByMonth ? new DateTime(date.Year, date.Month, 1).ToString("yyyy-MM") : date.Date.ToString("yyyy-MM-dd");
        DateTime BucketStart(DateTime date) => groupByMonth ? new DateTime(date.Year, date.Month, 1) : date.Date;
        string BucketLabel(DateTime date) => groupByMonth ? date.ToString("MMM yyyy") : date.ToString("MMM dd");

        var buckets = new SortedDictionary<string, ShopProfitLossTrendPointDto>(StringComparer.Ordinal);
        var cogsAccumulator = new Dictionary<string, decimal>();
        var expenseAccumulator = new Dictionary<string, decimal>();

        ShopProfitLossTrendPointDto GetBucket(DateTime date)
        {
            var key = BucketKey(date);
            if (!buckets.TryGetValue(key, out var bucket))
            {
                bucket = new ShopProfitLossTrendPointDto { Label = BucketLabel(date), PeriodStart = BucketStart(date) };
                buckets[key] = bucket;
                cogsAccumulator[key] = 0;
                expenseAccumulator[key] = 0;
            }

            return bucket;
        }

        var cursor = range.From;
        while (cursor < range.ToExclusive)
        {
            GetBucket(cursor);
            cursor = groupByMonth ? cursor.AddMonths(1) : cursor.AddDays(1);
        }
        if (buckets.Count == 0) GetBucket(range.From);

        foreach (var sale in computation.Sales)
        {
            var key = BucketKey(sale.SaleDate);
            var bucket = GetBucket(sale.SaleDate);
            bucket.NetSales += sale.SubTotal - sale.DiscountAmount;
            cogsAccumulator[key] += cogsBySale.GetValueOrDefault(sale.Id);
        }

        foreach (var ret in computation.SaleReturns)
        {
            var key = BucketKey(ret.ReturnDate);
            var bucket = GetBucket(ret.ReturnDate);
            bucket.NetSales -= ret.GrandTotal;
            cogsAccumulator[key] -= cogsReversalByReturn.GetValueOrDefault(ret.Id);
        }

        foreach (var expense in computation.Expenses)
        {
            var key = BucketKey(expense.ExpenseDate);
            GetBucket(expense.ExpenseDate);
            expenseAccumulator[key] += expense.Amount;
        }

        foreach (var (key, bucket) in buckets)
        {
            bucket.NetSales = Math.Round(bucket.NetSales, 2);

            if (canCost)
            {
                bucket.CostOfGoodsSold = Math.Round(cogsAccumulator[key], 2);
                bucket.GrossProfit = Math.Round(bucket.NetSales - bucket.CostOfGoodsSold.Value, 2);
            }

            if (canExpenses)
            {
                bucket.OperatingExpenses = Math.Round(expenseAccumulator[key], 2);
            }

            if (canCost && canExpenses)
            {
                bucket.NetProfit = Math.Round(bucket.GrossProfit!.Value - bucket.OperatingExpenses!.Value, 2);
            }
        }

        return buckets.Values.OrderBy(x => x.PeriodStart).ToList();
    }
}
