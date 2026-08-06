using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.GoodsReceipts;

public abstract class ShopGoodsReceiptItemEditDtoBase
{
    [Required]
    public Guid PurchaseOrderItemId { get; set; }

    // Quantity, price, and percentage bounds are enforced by ShopGoodsReceiptManager /
    // ShopGoodsReceiptItem as localized business rules rather than field validation
    // attributes, so callers get a friendly ShopManagement:* business exception.
    public decimal ReceivedQuantity { get; set; }
    public decimal BonusQuantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }

    [StringLength(ShopGoodsReceiptConsts.BatchNumberMaxLength)]
    public string? BatchNumber { get; set; }

    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public decimal DiscountPercentage { get; set; }
    public decimal TaxPercentage { get; set; }
}
