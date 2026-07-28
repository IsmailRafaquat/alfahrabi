using System;

namespace EHub.ShopManagement.Dashboard;

public class ShopDashboardChartPointDto
{
    public string Label { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public decimal Value { get; set; }
}
