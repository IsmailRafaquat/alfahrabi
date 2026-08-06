using System;
using EHub.ShopManagement.ProductBatches;

namespace EHub.ShopManagement.Dashboard;

public class ShopDashboardBatchAlertDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    public Guid ProductBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public int? DaysToExpiry { get; set; }
    public decimal AvailableQuantity { get; set; }
    public ShopProductBatchStatus Status { get; set; }
}
