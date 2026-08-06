using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.ProductBatches;

/// <summary>Only safe metadata may be edited directly; quantities always flow through inventory documents.</summary>
public class UpdateShopProductBatchDto
{
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    [StringLength(ShopProductBatchConsts.NotesMaxLength)]
    public string? Notes { get; set; }
}
