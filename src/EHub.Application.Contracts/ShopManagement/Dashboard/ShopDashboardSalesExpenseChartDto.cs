using System.Collections.Generic;

namespace EHub.ShopManagement.Dashboard;

public class ShopDashboardSalesExpenseChartDto
{
    /// <summary>"Day" or "Month" - tells the frontend how each point's Label/PeriodStart should be formatted.</summary>
    public string GroupBy { get; set; } = "Day";

    public List<ShopDashboardChartPointDto> NetSalesPoints { get; set; } = new();
    public List<ShopDashboardChartPointDto> ExpensePoints { get; set; } = new();
}
