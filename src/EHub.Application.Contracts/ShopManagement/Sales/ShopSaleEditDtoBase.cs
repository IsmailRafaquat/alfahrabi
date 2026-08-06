using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.Sales;

public abstract class ShopSaleEditDtoBase<TItem> where TItem : ShopSaleItemEditDtoBase
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public DateTime SaleDate { get; set; }

    public DateTime? DueDate { get; set; }

    public ShopSaleType SaleType { get; set; }
    public ShopSalePaymentMethod PaymentMethod { get; set; }

    // Bounds (non-negative, cannot exceed GrandTotal) are enforced by ShopSale as a
    // localized business rule rather than a field validation attribute.
    public decimal PaidAmount { get; set; }

    [StringLength(ShopSaleConsts.ReferenceNumberMaxLength)]
    public string? ReferenceNumber { get; set; }

    public decimal OtherCharges { get; set; }

    [StringLength(ShopSaleConsts.NotesMaxLength)]
    public string? Notes { get; set; }

    [Required, MinLength(1)]
    public List<TItem> Items { get; set; } = new();
}
