using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.GoodsReceipts;

public class ShopGoodsReceiptItemDto : EntityDto<Guid>
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
    public decimal ReceivedQuantity { get; set; }
    public decimal BonusQuantity { get; set; }

    public decimal? PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }

    public string? BatchNumber { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public decimal DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? LineSubTotal { get; set; }
    public decimal? LineTotal { get; set; }
}
