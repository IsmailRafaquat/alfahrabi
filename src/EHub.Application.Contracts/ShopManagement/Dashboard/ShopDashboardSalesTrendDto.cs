namespace EHub.ShopManagement.Dashboard;

public class ShopDashboardSalesTrendDto
{
    public decimal CurrentPeriodNetSales { get; set; }
    public decimal PreviousPeriodNetSales { get; set; }
    public decimal SalesGrowthAmount { get; set; }

    /// <summary>Null when PreviousPeriodNetSales is zero - percentage growth is not meaningful in that case.</summary>
    public decimal? SalesGrowthPercentage { get; set; }
}
