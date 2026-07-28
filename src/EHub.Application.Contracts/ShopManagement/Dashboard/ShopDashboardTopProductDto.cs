using System;

namespace EHub.ShopManagement.Dashboard;

public class ShopDashboardTopProductDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;

    public decimal QuantitySold { get; set; }
    public decimal GrossSalesAmount { get; set; }
    public decimal ReturnQuantity { get; set; }
    public decimal NetQuantitySold { get; set; }
    public decimal NetSalesAmount { get; set; }
}
