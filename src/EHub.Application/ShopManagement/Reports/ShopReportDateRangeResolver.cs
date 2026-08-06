using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;

namespace EHub.ShopManagement.Reports;

public class ShopReportDateRangeResolver : IShopReportDateRangeResolver, ITransientDependency
{
    private readonly IClock _clock;

    public ShopReportDateRangeResolver(IClock clock)
    {
        _clock = clock;
    }

    public Task<ShopReportDateRange> ResolveAsync(ShopReportPeriod period, DateTime? dateFrom, DateTime? dateTo)
    {
        var today = _clock.Now.Date;
        DateTime from, to;

        switch (period)
        {
            case ShopReportPeriod.Today:
                from = to = today;
                break;
            case ShopReportPeriod.Yesterday:
                from = to = today.AddDays(-1);
                break;
            case ShopReportPeriod.Last7Days:
                from = today.AddDays(-6);
                to = today;
                break;
            case ShopReportPeriod.Last30Days:
                from = today.AddDays(-29);
                to = today;
                break;
            case ShopReportPeriod.ThisMonth:
                from = new DateTime(today.Year, today.Month, 1);
                to = today;
                break;
            case ShopReportPeriod.LastMonth:
                var firstOfThisMonth = new DateTime(today.Year, today.Month, 1);
                from = firstOfThisMonth.AddMonths(-1);
                to = firstOfThisMonth.AddDays(-1);
                break;
            case ShopReportPeriod.ThisYear:
                from = new DateTime(today.Year, 1, 1);
                to = today;
                break;
            case ShopReportPeriod.ThisQuarter:
                var quarterStartMonth = ((today.Month - 1) / 3) * 3 + 1;
                from = new DateTime(today.Year, quarterStartMonth, 1);
                to = today;
                break;
            case ShopReportPeriod.Custom:
                if (!dateFrom.HasValue || !dateTo.HasValue)
                    throw new BusinessException("ShopManagement:ReportDateRangeRequired");
                from = dateFrom.Value.Date;
                to = dateTo.Value.Date;
                if (from > to) throw new BusinessException("ShopManagement:ReportDateRangeInvalid");
                break;
            default:
                throw new BusinessException("ShopManagement:ReportPeriodInvalid");
        }

        return Task.FromResult(new ShopReportDateRange { From = from, ToExclusive = to.AddDays(1) });
    }
}
