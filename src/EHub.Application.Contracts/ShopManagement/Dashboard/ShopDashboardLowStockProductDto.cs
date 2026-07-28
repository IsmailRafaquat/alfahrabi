using System;

namespace EHub.ShopManagement.Dashboard;

public class ShopDashboardLowStockProductDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;

    public decimal CurrentStock { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal RequiredReorderQuantity { get; set; }
    public bool IsOutOfStock { get; set; }
}
