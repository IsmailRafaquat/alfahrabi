using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.StockCounts;

public class UpdateShopStockCountItemQuantityDto
{
    [Required]
    public Guid StockCountItemId { get; set; }

    [Required]
    public decimal PhysicalQuantity { get; set; }

    [StringLength(ShopStockCountConsts.ItemNotesMaxLength)]
    public string? Notes { get; set; }
}
