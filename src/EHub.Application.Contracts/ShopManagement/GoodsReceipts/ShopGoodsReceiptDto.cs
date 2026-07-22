using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.GoodsReceipts;

public class ShopGoodsReceiptDto : EntityDto<Guid>
{
    public string GoodsReceiptNumber { get; set; } = string.Empty;

    public Guid PurchaseOrderId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;

    public string? SupplierInvoiceNumber { get; set; }
    public DateTime ReceiptDate { get; set; }
    public ShopGoodsReceiptStatus Status { get; set; }

    public decimal? SubTotal { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? ShippingCharges { get; set; }
    public decimal? OtherCharges { get; set; }
    public decimal? GrandTotal { get; set; }
    public decimal? PaidAmount { get; set; }
    public decimal? ReturnAmount { get; set; }
    public decimal? PendingAmount { get; set; }
    public ShopGoodsReceiptPaymentStatus PaymentStatus { get; set; }
    public string? Notes { get; set; }

    public Guid? ReceivedByUserId { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreationTime { get; set; }

    public List<ShopGoodsReceiptItemDto> Items { get; set; } = new();
}
