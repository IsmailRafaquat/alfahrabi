using System;

namespace EHub.ShopManagement.GoodsReceipts;

public class ShopPurchaseOrderReceivingItemDto
{
    public Guid PurchaseOrderItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string UnitShortName { get; set; } = string.Empty;
    public bool UnitAllowDecimal { get; set; }
    public bool TrackBatch { get; set; }
    public bool TrackExpiry { get; set; }
    public bool TrackSerialNumber { get; set; }

    public decimal OrderedQuantity { get; set; }
    public decimal PreviouslyReceivedQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }

    public decimal? DefaultPurchasePrice { get; set; }
    public decimal? DefaultSalePrice { get; set; }
}
