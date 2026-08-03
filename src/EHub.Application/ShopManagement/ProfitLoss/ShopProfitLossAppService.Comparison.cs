using System;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Reports;

namespace EHub.ShopManagement.ProfitLoss;

public partial class ShopProfitLossAppService
{
    public async Task<ShopProfitLossComparisonDto> GetComparisonAsync(GetShopProfitLossInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);
        return await BuildComparisonAsync(tenantId, input.Period, range, currentCore: null, closingInventoryValue: null);
    }

    /// <summary>
    /// <paramref name="currentCore"/>/<paramref name="closingInventoryValue"/> let a caller that
    /// already computed the current period (e.g. GetAsync building the main statement) pass that
    /// work in instead of this method redoing it - closing inventory valuation in particular is a
    /// full product/batch scan that is identical for every period, so it must never be computed more
    /// than once per request.
    /// </summary>
    private async Task<ShopProfitLossComparisonDto> BuildComparisonAsync(
        Guid tenantId, ShopReportPeriod period, ShopReportDateRange range,
        ShopProfitLossCoreResult? currentCore, decimal? closingInventoryValue)
    {
        var (previousFrom, previousToExclusive) = GetPreviousPeriodRange(period, range);

        closingInventoryValue ??= await _calculator.ComputeClosingInventoryValueAsync(tenantId);
        var current = currentCore ?? await _calculator.ComputeAsync(tenantId, range.From, range.ToExclusive, closingInventoryValue);
        var previous = await _calculator.ComputeAsync(tenantId, previousFrom, previousToExclusive, closingInventoryValue);

        var canRevenue = await CanAsync(EHubPermissions.ShopProfitLoss.ViewRevenue);
        var canCost = await CanAsync(EHubPermissions.ShopProfitLoss.ViewCost);
        var canExpenses = await CanAsync(EHubPermissions.ShopProfitLoss.ViewExpenses);

        var dto = new ShopProfitLossComparisonDto
        {
            CurrentNetSales = canRevenue ? current.NetSales : 0,
            PreviousNetSales = canRevenue ? previous.NetSales : 0,
        };

        if (canCost)
        {
            dto.CurrentGrossProfit = current.GrossProfit;
            dto.PreviousGrossProfit = previous.GrossProfit;
        }

        if (canCost && canExpenses)
        {
            dto.CurrentNetProfit = current.NetProfit;
            dto.PreviousNetProfit = previous.NetProfit;
            dto.NetProfitChangeAmount = Math.Round(current.NetProfit - previous.NetProfit, 2);
            dto.NetProfitChangePercentage = previous.NetProfit != 0
                ? Math.Round((current.NetProfit - previous.NetProfit) / Math.Abs(previous.NetProfit) * 100, 2)
                : null;
        }

        return dto;
    }

    /// <summary>
    /// Immediately preceding equivalent period. For "so far this <i>unit</i>" periods (ThisMonth,
    /// ThisQuarter, ThisYear) the comparison window mirrors the same day-count so a partial current
    /// period is never compared against a full prior one. LastMonth (already a complete calendar
    /// month) compares against the complete month before it.
    /// </summary>
    private static (DateTime From, DateTime ToExclusive) GetPreviousPeriodRange(ShopReportPeriod period, ShopReportDateRange current)
    {
        var daySpan = (current.ToExclusive - current.From).Days;

        switch (period)
        {
            case ShopReportPeriod.LastMonth:
                var prevOfLastMonth = current.From.AddMonths(-1);
                return (prevOfLastMonth, current.From);
            case ShopReportPeriod.ThisMonth:
                var prevMonthFirst = current.From.AddMonths(-1);
                return (prevMonthFirst, prevMonthFirst.AddDays(daySpan));
            case ShopReportPeriod.ThisQuarter:
                var prevQuarterStart = current.From.AddMonths(-3);
                return (prevQuarterStart, prevQuarterStart.AddDays(daySpan));
            case ShopReportPeriod.ThisYear:
                var prevYearStart = current.From.AddYears(-1);
                return (prevYearStart, prevYearStart.AddDays(daySpan));
            default:
                return (current.From.AddDays(-daySpan), current.From);
        }
    }
}
