using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.ProductBatches;

public class ShopProductBatchDto : EntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string UnitShortName { get; set; } = string.Empty;

    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public decimal ReceivedQuantity { get; set; }
    public decimal IssuedQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal? UnitCost { get; set; }

    public ShopProductBatchStatus Status { get; set; }
    public int? DaysToExpiry { get; set; }

    public DateTime? FirstReceivedDate { get; set; }
    public DateTime? LastMovementDate { get; set; }

    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public Guid? GoodsReceiptId { get; set; }
    public string? GoodsReceiptNumber { get; set; }

    public string? Notes { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }

    public DateTime CreationTime { get; set; }
}
