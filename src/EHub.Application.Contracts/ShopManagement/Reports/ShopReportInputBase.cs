using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Reports;

public abstract class ShopReportInputBase : PagedAndSortedResultRequestDto
{
    public ShopReportPeriod Period { get; set; } = ShopReportPeriod.ThisMonth;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Filter { get; set; }
}
