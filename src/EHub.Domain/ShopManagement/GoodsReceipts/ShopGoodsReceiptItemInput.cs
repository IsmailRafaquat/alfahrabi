using System;

namespace EHub.ShopManagement.GoodsReceipts;

/// <summary>
/// Domain-layer input for a goods receipt line, decoupled from the application layer's DTOs
/// so that <see cref="ShopGoodsReceiptManager"/> does not depend on EHub.Application.Contracts.
/// </summary>
public class ShopGoodsReceiptItemInput
{
    public Guid PurchaseOrderItemId { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal BonusQuantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TaxPercentage { get; set; }
}
