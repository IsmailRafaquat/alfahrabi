using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.PurchaseOrders;

public class ShopPurchaseOrderDto : EntityDto<Guid>
{
    public string PurchaseOrderNumber { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public ShopPurchaseOrderStatus Status { get; set; }
    public string? SupplierReference { get; set; }

    public decimal? SubTotal { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? ShippingCharges { get; set; }
    public decimal? OtherCharges { get; set; }
    public decimal? GrandTotal { get; set; }
    public string? Notes { get; set; }

    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public Guid? RejectedByUserId { get; set; }
    public DateTime? RejectedDate { get; set; }
    public string? RejectionReason { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreationTime { get; set; }

    public List<ShopPurchaseOrderItemDto> Items { get; set; } = new();
}
