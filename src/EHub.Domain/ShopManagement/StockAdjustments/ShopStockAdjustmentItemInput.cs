using System;

namespace EHub.ShopManagement.StockAdjustments;

/// <summary>
/// Domain-layer input for a stock adjustment line, decoupled from the application layer's DTOs
/// so that <see cref="ShopStockAdjustmentManager"/> does not depend on EHub.Application.Contracts.
/// </summary>
public class ShopStockAdjustmentItemInput
{
    public Guid ProductId { get; set; }
    public ShopStockAdjustmentType AdjustmentType { get; set; }
    public decimal AdjustmentQuantity { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public Guid? ProductBatchId { get; set; }
    public ShopStockAdjustmentReason Reason { get; set; }
    public string? Notes { get; set; }
}
