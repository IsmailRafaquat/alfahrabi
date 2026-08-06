using System;
using System.Threading.Tasks;

namespace EHub.ShopManagement.Reports;

public class ShopReportDateRange
{
    public DateTime From { get; set; }

    /// <summary>Exclusive upper bound (DateTo.Date + 1 day) - used directly in "x &lt; ToExclusive" filters.</summary>
    public DateTime ToExclusive { get; set; }
}

public interface IShopReportDateRangeResolver
{
    Task<ShopReportDateRange> ResolveAsync(ShopReportPeriod period, DateTime? dateFrom, DateTime? dateTo);
}
