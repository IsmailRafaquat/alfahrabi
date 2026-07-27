using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.StockAdjustments;

public class UpdateShopStockAdjustmentItemDto : CreateShopStockAdjustmentItemDto
{
}

public class UpdateShopStockAdjustmentDto
{
    [Required]
    public DateTime AdjustmentDate { get; set; }

    [Required]
    public ShopStockAdjustmentReason Reason { get; set; }

    [StringLength(ShopStockAdjustmentConsts.ReasonDetailsMaxLength)]
    public string? ReasonDetails { get; set; }

    [StringLength(ShopStockAdjustmentConsts.NotesMaxLength)]
    public string? Notes { get; set; }

    [MinLength(1)]
    public List<UpdateShopStockAdjustmentItemDto> Items { get; set; } = new();
}
