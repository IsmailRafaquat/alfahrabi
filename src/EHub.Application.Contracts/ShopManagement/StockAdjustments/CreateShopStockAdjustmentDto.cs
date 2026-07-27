using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.StockAdjustments;

public class CreateShopStockAdjustmentItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public ShopStockAdjustmentType AdjustmentType { get; set; }

    // Must be > 0; enforced by ShopStockAdjustmentItem as a localized BusinessException.
    public decimal AdjustmentQuantity { get; set; }

    [StringLength(ShopStockAdjustmentConsts.BatchNumberMaxLength)]
    public string? BatchNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public ShopStockAdjustmentReason Reason { get; set; }

    [StringLength(ShopStockAdjustmentConsts.ItemNotesMaxLength)]
    public string? Notes { get; set; }
}

public class CreateShopStockAdjustmentDto
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
    public List<CreateShopStockAdjustmentItemDto> Items { get; set; } = new();
}
