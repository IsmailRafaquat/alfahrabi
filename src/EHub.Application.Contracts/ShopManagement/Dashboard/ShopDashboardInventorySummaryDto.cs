namespace EHub.ShopManagement.Dashboard;

public class ShopDashboardInventorySummaryDto
{
    public decimal InventoryQuantity { get; set; }
    public decimal? InventoryValue { get; set; }
    public int LowStockProductCount { get; set; }
    public int OutOfStockProductCount { get; set; }
    public int NearExpiryBatchCount { get; set; }
    public int ExpiredBatchCount { get; set; }
}
