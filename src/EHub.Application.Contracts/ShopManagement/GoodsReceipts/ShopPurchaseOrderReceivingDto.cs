using System;
using System.Collections.Generic;
using EHub.ShopManagement.PurchaseOrders;

namespace EHub.ShopManagement.GoodsReceipts;

public class ShopPurchaseOrderReceivingDto
{
    public Guid PurchaseOrderId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public ShopPurchaseOrderStatus Status { get; set; }

    public List<ShopPurchaseOrderReceivingItemDto> Items { get; set; } = new();
}
