using System;

namespace EHub.ShopManagement.PurchaseOrders;

/// <summary>
/// Domain-layer input for a purchase order line, decoupled from the application layer's DTOs
/// so that <see cref="ShopPurchaseOrderManager"/> does not depend on EHub.Application.Contracts.
/// </summary>
public class ShopPurchaseOrderItemInput
{
    public Guid ProductId { get; set; }
    public string? Description { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal UnitPurchasePrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TaxPercentage { get; set; }
}
