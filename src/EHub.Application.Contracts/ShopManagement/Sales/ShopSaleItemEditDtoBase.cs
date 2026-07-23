using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.Sales;

public abstract class ShopSaleItemEditDtoBase
{
    [Required]
    public Guid ProductId { get; set; }

    // Quantity, price, and percentage bounds are enforced by ShopSaleManager / ShopSaleItem
    // as localized business rules rather than field validation attributes, so callers get a
    // friendly ShopManagement:* business exception.
    public decimal Quantity { get; set; }
    public decimal UnitSalePrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TaxPercentage { get; set; }

    [StringLength(ShopSaleConsts.BatchNumberMaxLength)]
    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }
}
