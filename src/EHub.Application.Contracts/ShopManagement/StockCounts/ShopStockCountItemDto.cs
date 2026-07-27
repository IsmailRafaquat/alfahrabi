using System;
using EHub.ShopManagement.StockAdjustments;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.StockCounts;

public class ShopStockCountItemDto : EntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string UnitShortName { get; set; } = string.Empty;
    public bool UnitAllowDecimal { get; set; }

    public Guid? ProductBatchId { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public decimal SystemQuantity { get; set; }
    public decimal? PhysicalQuantity { get; set; }
    public decimal DifferenceQuantity { get; set; }
    public ShopStockAdjustmentType? AdjustmentType { get; set; }
    public bool IsCounted { get; set; }
    public string? Notes { get; set; }
}
