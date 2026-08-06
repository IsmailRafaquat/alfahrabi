using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.Products;

public abstract class ShopProductEditDtoBase
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public Guid UnitId { get; set; }

    [Required, StringLength(ShopProductConsts.NameMaxLength)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(ShopProductConsts.CodeMaxLength)]
    public string Code { get; set; } = string.Empty;

    [StringLength(ShopProductConsts.SkuMaxLength)]
    public string? SKU { get; set; }

    [StringLength(ShopProductConsts.BarcodeMaxLength)]
    public string? Barcode { get; set; }

    [StringLength(ShopProductConsts.DescriptionMaxLength)]
    public string? Description { get; set; }

    [StringLength(ShopProductConsts.BrandMaxLength)]
    public string? Brand { get; set; }

    [StringLength(ShopProductConsts.ModelMaxLength)]
    public string? Model { get; set; }

    // Numeric bounds (non-negative, tax 0-100, min/max stock relationship, etc.) are enforced
    // by ShopProductManager as localized business rules rather than field validation attributes,
    // so callers get a friendly ShopManagement:* business exception instead of a generic
    // validation error.
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? MinimumSalePrice { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal MinimumStockLevel { get; set; }
    public decimal? MaximumStockLevel { get; set; }
    public decimal ReorderLevel { get; set; }

    public bool TrackBatch { get; set; }
    public bool TrackExpiry { get; set; }
    public int? ExpiryAlertDays { get; set; }
    public bool BlockExpiredSale { get; set; } = true;
    public bool TrackSerialNumber { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsActive { get; set; } = true;
}
