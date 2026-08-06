using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.PurchaseOrders;

public abstract class ShopPurchaseOrderEditDtoBase<TItem> where TItem : ShopPurchaseOrderItemEditDtoBase
{
    [Required]
    public Guid SupplierId { get; set; }

    [Required]
    public DateTime OrderDate { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    [StringLength(ShopPurchaseOrderConsts.SupplierReferenceMaxLength)]
    public string? SupplierReference { get; set; }

    // Negative values are rejected by ShopPurchaseOrder as localized business rules.
    public decimal ShippingCharges { get; set; }
    public decimal OtherCharges { get; set; }

    [StringLength(ShopPurchaseOrderConsts.NotesMaxLength)]
    public string? Notes { get; set; }

    [Required, MinLength(1)]
    public List<TItem> Items { get; set; } = new();
}
