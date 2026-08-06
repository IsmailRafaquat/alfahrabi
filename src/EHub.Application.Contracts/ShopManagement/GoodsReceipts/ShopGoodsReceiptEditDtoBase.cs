using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.GoodsReceipts;

public abstract class ShopGoodsReceiptEditDtoBase<TItem> where TItem : ShopGoodsReceiptItemEditDtoBase
{
    [Required]
    public Guid PurchaseOrderId { get; set; }

    [StringLength(ShopGoodsReceiptConsts.SupplierInvoiceNumberMaxLength)]
    public string? SupplierInvoiceNumber { get; set; }

    [Required]
    public DateTime ReceiptDate { get; set; }

    // Negative values are rejected by ShopGoodsReceipt as localized business rules.
    public decimal ShippingCharges { get; set; }
    public decimal OtherCharges { get; set; }

    [StringLength(ShopGoodsReceiptConsts.NotesMaxLength)]
    public string? Notes { get; set; }

    [Required, MinLength(1)]
    public List<TItem> Items { get; set; } = new();
}
