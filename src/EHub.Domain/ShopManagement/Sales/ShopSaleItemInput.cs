using System;

namespace EHub.ShopManagement.Sales;

/// <summary>
/// Domain-layer input for a sale line, decoupled from the application layer's DTOs
/// so that <see cref="ShopSaleManager"/> does not depend on EHub.Application.Contracts.
/// </summary>
public class ShopSaleItemInput
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitSalePrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TaxPercentage { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
