using System;

namespace EHub.ShopManagement.ProductBatches;

/// <summary>Read-only preview/result of a batch allocation (planned or already applied).</summary>
public class ShopBatchAllocationDto
{
    public Guid ProductBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
}
