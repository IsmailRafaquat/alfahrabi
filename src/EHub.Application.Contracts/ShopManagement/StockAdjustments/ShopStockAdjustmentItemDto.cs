using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.StockAdjustments;

public class ShopStockAdjustmentItemDto : EntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string UnitShortName { get; set; } = string.Empty;
    public bool UnitAllowDecimal { get; set; }

    public ShopStockAdjustmentType AdjustmentType { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal AdjustmentQuantity { get; set; }
    public decimal FinalQuantity { get; set; }
    public decimal? UnitCostSnapshot { get; set; }

    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public ShopStockAdjustmentReason Reason { get; set; }
    public string? Notes { get; set; }
}
