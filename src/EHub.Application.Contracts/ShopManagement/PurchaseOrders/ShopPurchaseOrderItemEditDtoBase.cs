using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.PurchaseOrders;

public abstract class ShopPurchaseOrderItemEditDtoBase
{
    [Required]
    public Guid ProductId { get; set; }

    [StringLength(ShopPurchaseOrderConsts.DescriptionMaxLength)]
    public string? Description { get; set; }

    // Quantity, price, and percentage bounds are enforced by ShopPurchaseOrderManager /
    // ShopPurchaseOrderItem as localized business rules rather than field validation
    // attributes, so callers get a friendly ShopManagement:* business exception.
    public decimal OrderedQuantity { get; set; }
    public decimal UnitPurchasePrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TaxPercentage { get; set; }
}
