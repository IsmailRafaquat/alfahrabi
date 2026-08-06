using System;

namespace EHub.ShopManagement.SaleReturns;

/// <summary>
/// Domain-layer input for a sale return line, decoupled from the application layer's DTOs
/// so that <see cref="ShopSaleReturnManager"/> does not depend on EHub.Application.Contracts.
/// </summary>
public class ShopSaleReturnItemInput
{
    public Guid SaleItemId { get; set; }
    public decimal ReturnQuantity { get; set; }
    public ShopSaleReturnReason Reason { get; set; }
    public string? Notes { get; set; }
}
