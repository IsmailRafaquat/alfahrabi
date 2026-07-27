using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.StockCounts;

public class CreateShopStockCountDto
{
    [Required]
    public DateTime CountDate { get; set; }

    [Required]
    public ShopStockCountScope Scope { get; set; }

    public Guid? ProductCategoryId { get; set; }

    public List<Guid>? SelectedProductIds { get; set; }

    [StringLength(ShopStockCountConsts.NotesMaxLength)]
    public string? Notes { get; set; }
}
