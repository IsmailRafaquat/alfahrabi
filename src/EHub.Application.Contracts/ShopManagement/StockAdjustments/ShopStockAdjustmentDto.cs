using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.StockAdjustments;

public class ShopStockAdjustmentDto : EntityDto<Guid>
{
    public string AdjustmentNumber { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public ShopStockAdjustmentStatus Status { get; set; }
    public ShopStockAdjustmentReason Reason { get; set; }
    public string? ReasonDetails { get; set; }
    public string? Notes { get; set; }

    public Guid? PostedByUserId { get; set; }
    public DateTime? PostedDate { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreationTime { get; set; }

    public List<ShopStockAdjustmentItemDto> Items { get; set; } = new();
}
