using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.StockCounts;

public class ShopStockCountDto : EntityDto<Guid>
{
    public string StockCountNumber { get; set; } = string.Empty;
    public DateTime CountDate { get; set; }
    public ShopStockCountScope Scope { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public string? ProductCategoryName { get; set; }
    public ShopStockCountStatus Status { get; set; }
    public string? Notes { get; set; }
    public Guid? GeneratedStockAdjustmentId { get; set; }
    public string? GeneratedStockAdjustmentNumber { get; set; }

    public DateTime? StartedDate { get; set; }
    public DateTime? CountedDate { get; set; }
    public DateTime? PostedDate { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreationTime { get; set; }

    public int TotalItems { get; set; }
    public int CountedItems { get; set; }
    public int DifferenceItems { get; set; }
    public decimal TotalIncreaseQuantity { get; set; }
    public decimal TotalDecreaseQuantity { get; set; }

    public List<ShopStockCountItemDto> Items { get; set; } = new();
}
