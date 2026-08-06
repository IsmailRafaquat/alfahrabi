using System;

namespace EHub.ShopManagement.ProductBatches;

/// <summary>
/// The outcome of planning a FEFO/FIFO allocation for a required quantity: which batch to draw from
/// and how much. Produced by <see cref="ShopProductBatchManager.AllocateAsync"/>; the caller still
/// has to call <see cref="ShopProductBatchManager.RemoveStockAsync"/> for each result to commit it.
/// </summary>
public class ShopBatchAllocationResult
{
    public Guid ProductBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}
