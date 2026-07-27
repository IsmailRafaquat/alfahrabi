using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.Units;

namespace EHub.ShopManagement.Products;

public class ShopProduct : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid CategoryId { get; protected set; }
    public ShopProductCategory? Category { get; protected set; }

    public Guid UnitId { get; protected set; }
    public ShopUnit? Unit { get; protected set; }

    public string Name { get; protected set; } = string.Empty;
    public string Code { get; protected set; } = string.Empty;
    public string? SKU { get; protected set; }
    public string? Barcode { get; protected set; }
    public string? Description { get; protected set; }
    public string? Brand { get; protected set; }
    public string? Model { get; protected set; }

    public decimal PurchasePrice { get; protected set; }
    public decimal SalePrice { get; protected set; }
    public decimal? WholesalePrice { get; protected set; }
    public decimal? MinimumSalePrice { get; protected set; }
    public decimal TaxPercentage { get; protected set; }

    public decimal CurrentStock { get; protected set; }
    public decimal MinimumStockLevel { get; protected set; }
    public decimal? MaximumStockLevel { get; protected set; }
    public decimal ReorderLevel { get; protected set; }

    public bool TrackBatch { get; protected set; }
    public bool TrackExpiry { get; protected set; }
    public int? ExpiryAlertDays { get; protected set; }
    public bool BlockExpiredSale { get; protected set; } = true;
    public bool TrackSerialNumber { get; protected set; }
    public bool IsTaxable { get; protected set; }
    public bool IsActive { get; protected set; } = true;

    protected ShopProduct() { }

    internal ShopProduct(
        Guid id,
        Guid tenantId,
        Guid categoryId,
        Guid unitId,
        string name,
        string code,
        string? sku,
        string? barcode,
        string? description,
        string? brand,
        string? model,
        decimal purchasePrice,
        decimal salePrice,
        decimal? wholesalePrice,
        decimal? minimumSalePrice,
        decimal taxPercentage,
        decimal minimumStockLevel,
        decimal? maximumStockLevel,
        decimal reorderLevel,
        bool trackBatch,
        bool trackExpiry,
        int? expiryAlertDays,
        bool blockExpiredSale,
        bool trackSerialNumber,
        bool isTaxable,
        bool isActive) : base(id)
    {
        TenantId = tenantId;
        CategoryId = categoryId;
        UnitId = unitId;
        CurrentStock = 0;
        SetValues(name, code, sku, barcode, description, brand, model, purchasePrice, salePrice, wholesalePrice,
            minimumSalePrice, taxPercentage, minimumStockLevel, maximumStockLevel, reorderLevel,
            trackBatch, trackExpiry, expiryAlertDays, blockExpiredSale, trackSerialNumber, isTaxable, isActive);
    }

    internal void IncreaseStock(decimal quantity)
    {
        if (quantity < 0) throw new BusinessException("ShopManagement:ProductStockIncreaseCannotBeNegative");
        CurrentStock += quantity;
    }

    public void DecreaseStock(decimal quantity)
    {
        if (quantity <= 0) throw new BusinessException("ShopManagement:ProductStockDecreaseMustBePositive");
        if (CurrentStock < quantity) throw new BusinessException("ShopManagement:InsufficientProductStock").WithData("Product", Name);
        CurrentStock -= quantity;
    }

    internal void Update(
        Guid categoryId,
        Guid unitId,
        string name,
        string code,
        string? sku,
        string? barcode,
        string? description,
        string? brand,
        string? model,
        decimal purchasePrice,
        decimal salePrice,
        decimal? wholesalePrice,
        decimal? minimumSalePrice,
        decimal taxPercentage,
        decimal minimumStockLevel,
        decimal? maximumStockLevel,
        decimal reorderLevel,
        bool trackBatch,
        bool trackExpiry,
        int? expiryAlertDays,
        bool blockExpiredSale,
        bool trackSerialNumber,
        bool isTaxable,
        bool isActive)
    {
        CategoryId = categoryId;
        UnitId = unitId;
        SetValues(name, code, sku, barcode, description, brand, model, purchasePrice, salePrice, wholesalePrice,
            minimumSalePrice, taxPercentage, minimumStockLevel, maximumStockLevel, reorderLevel,
            trackBatch, trackExpiry, expiryAlertDays, blockExpiredSale, trackSerialNumber, isTaxable, isActive);
    }

    private void SetValues(
        string name,
        string code,
        string? sku,
        string? barcode,
        string? description,
        string? brand,
        string? model,
        decimal purchasePrice,
        decimal salePrice,
        decimal? wholesalePrice,
        decimal? minimumSalePrice,
        decimal taxPercentage,
        decimal minimumStockLevel,
        decimal? maximumStockLevel,
        decimal reorderLevel,
        bool trackBatch,
        bool trackExpiry,
        int? expiryAlertDays,
        bool blockExpiredSale,
        bool trackSerialNumber,
        bool isTaxable,
        bool isActive)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ShopProductConsts.NameMaxLength).Trim();
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ShopProductConsts.CodeMaxLength).Trim();
        SKU = Check.Length(sku?.Trim(), nameof(sku), ShopProductConsts.SkuMaxLength);
        Barcode = Check.Length(barcode?.Trim(), nameof(barcode), ShopProductConsts.BarcodeMaxLength);
        Description = Check.Length(description?.Trim(), nameof(description), ShopProductConsts.DescriptionMaxLength);
        Brand = Check.Length(brand?.Trim(), nameof(brand), ShopProductConsts.BrandMaxLength);
        Model = Check.Length(model?.Trim(), nameof(model), ShopProductConsts.ModelMaxLength);

        if (purchasePrice < 0) throw new BusinessException("ShopManagement:ProductPurchasePriceCannotBeNegative");
        if (salePrice < 0) throw new BusinessException("ShopManagement:ProductSalePriceCannotBeNegative");
        if (wholesalePrice.HasValue && wholesalePrice.Value < 0) throw new BusinessException("ShopManagement:ProductWholesalePriceCannotBeNegative");
        if (minimumSalePrice.HasValue && minimumSalePrice.Value < 0) throw new BusinessException("ShopManagement:ProductMinimumSalePriceCannotBeNegative");
        if (minimumSalePrice.HasValue && minimumSalePrice.Value > salePrice) throw new BusinessException("ShopManagement:MinimumSalePriceCannotExceedSalePrice");
        if (taxPercentage < 0 || taxPercentage > 100) throw new BusinessException("ShopManagement:InvalidTaxPercentage");
        if (minimumStockLevel < 0) throw new BusinessException("ShopManagement:ProductMinimumStockLevelCannotBeNegative");
        if (maximumStockLevel.HasValue && maximumStockLevel.Value < 0) throw new BusinessException("ShopManagement:ProductMaximumStockLevelCannotBeNegative");
        if (reorderLevel < 0) throw new BusinessException("ShopManagement:ProductReorderLevelCannotBeNegative");
        if (maximumStockLevel.HasValue && maximumStockLevel.Value > 0 && maximumStockLevel.Value < minimumStockLevel)
            throw new BusinessException("ShopManagement:MaximumStockCannotBeLowerThanMinimumStock");
        if (trackExpiry && !trackBatch) throw new BusinessException("ShopManagement:ExpiryRequiresBatchTracking");
        if (expiryAlertDays.HasValue && expiryAlertDays.Value < 0) throw new BusinessException("ShopManagement:InvalidExpiryAlertDays");

        PurchasePrice = purchasePrice;
        SalePrice = salePrice;
        WholesalePrice = wholesalePrice;
        MinimumSalePrice = minimumSalePrice;
        IsTaxable = isTaxable;
        TaxPercentage = isTaxable ? taxPercentage : 0;
        MinimumStockLevel = minimumStockLevel;
        MaximumStockLevel = maximumStockLevel;
        ReorderLevel = reorderLevel;
        TrackBatch = trackBatch;
        TrackExpiry = trackExpiry;
        ExpiryAlertDays = trackExpiry ? (expiryAlertDays ?? 30) : null;
        BlockExpiredSale = blockExpiredSale;
        TrackSerialNumber = trackSerialNumber;
        IsActive = isActive;
    }
}
