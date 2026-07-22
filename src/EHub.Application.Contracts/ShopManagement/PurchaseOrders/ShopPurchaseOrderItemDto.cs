using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.PurchaseOrders;

public class ShopPurchaseOrderItemDto : EntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string UnitShortName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal? UnitPurchasePrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? LineSubTotal { get; set; }
    public decimal? LineTotal { get; set; }
}
