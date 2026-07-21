using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.Units;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.Products;

public class ShopProductManager : DomainService
{
    private readonly IRepository<ShopProduct, Guid> _repository;
    private readonly IRepository<ShopProductCategory, Guid> _categoryRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly ICurrentTenant _currentTenant;

    public ShopProductManager(
        IRepository<ShopProduct, Guid> repository,
        IRepository<ShopProductCategory, Guid> categoryRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        ICurrentTenant currentTenant)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _unitRepository = unitRepository;
        _currentTenant = currentTenant;
    }

    public async Task<ShopProduct> CreateAsync(
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
        bool trackSerialNumber,
        bool isTaxable,
        bool isActive)
    {
        var tenantId = RequireTenant();
        var normalizedName = NormalizeName(name);
        var normalizedCode = NormalizeCode(code);
        var normalizedSku = NormalizeOptional(sku, ShopProductConsts.SkuMaxLength);
        var normalizedBarcode = NormalizeOptional(barcode, ShopProductConsts.BarcodeMaxLength);

        await ValidateCategoryAsync(categoryId, tenantId, requireActive: true);
        await ValidateUnitAsync(unitId, tenantId, requireActive: true);
        await ValidateUniqueAsync(normalizedCode, normalizedSku, normalizedBarcode, tenantId, null);

        return new ShopProduct(GuidGenerator.Create(), tenantId, categoryId, unitId, normalizedName, normalizedCode,
            normalizedSku, normalizedBarcode, description, brand, model, purchasePrice, salePrice, wholesalePrice,
            minimumSalePrice, taxPercentage, minimumStockLevel, maximumStockLevel, reorderLevel,
            trackBatch, trackExpiry, trackSerialNumber, isTaxable, isActive);
    }

    public async Task UpdateAsync(
        ShopProduct product,
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
        bool trackSerialNumber,
        bool isTaxable,
        bool isActive)
    {
        var tenantId = RequireTenant();
        if (product.TenantId != tenantId) throw new BusinessException("ShopManagement:ProductNotFound");

        var normalizedName = NormalizeName(name);
        var normalizedCode = NormalizeCode(code);
        var normalizedSku = NormalizeOptional(sku, ShopProductConsts.SkuMaxLength);
        var normalizedBarcode = NormalizeOptional(barcode, ShopProductConsts.BarcodeMaxLength);

        await ValidateCategoryAsync(categoryId, tenantId, requireActive: false);
        await ValidateUnitAsync(unitId, tenantId, requireActive: false);
        await ValidateUniqueAsync(normalizedCode, normalizedSku, normalizedBarcode, tenantId, product.Id);

        product.Update(categoryId, unitId, normalizedName, normalizedCode, normalizedSku, normalizedBarcode,
            description, brand, model, purchasePrice, salePrice, wholesalePrice, minimumSalePrice, taxPercentage,
            minimumStockLevel, maximumStockLevel, reorderLevel, trackBatch, trackExpiry, trackSerialNumber,
            isTaxable, isActive);
    }

    public Task ValidateDeleteAsync(Guid id)
    {
        RequireTenant();
        return Task.CompletedTask;
    }

    private async Task ValidateCategoryAsync(Guid categoryId, Guid tenantId, bool requireActive)
    {
        var query = await _categoryRepository.GetQueryableAsync();
        var category = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == categoryId && x.TenantId == tenantId));
        if (category == null) throw new BusinessException("ShopManagement:ProductCategoryNotFound");
        if (requireActive && !category.IsActive) throw new BusinessException("ShopManagement:ProductCategoryInactive");
    }

    private async Task ValidateUnitAsync(Guid unitId, Guid tenantId, bool requireActive)
    {
        var query = await _unitRepository.GetQueryableAsync();
        var unit = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == unitId && x.TenantId == tenantId));
        if (unit == null) throw new BusinessException("ShopManagement:ProductUnitNotFound");
        if (requireActive && !unit.IsActive) throw new BusinessException("ShopManagement:ProductUnitInactive");
    }

    private async Task ValidateUniqueAsync(string code, string? sku, string? barcode, Guid tenantId, Guid? excludedId)
    {
        var query = await _repository.GetQueryableAsync();
        if (await AsyncExecuter.AnyAsync(query.Where(x => x.TenantId == tenantId && x.Code == code && (!excludedId.HasValue || x.Id != excludedId))))
            throw new BusinessException("ShopManagement:ProductCodeAlreadyExists").WithData("Code", code);
        if (sku != null && await AsyncExecuter.AnyAsync(query.Where(x => x.TenantId == tenantId && x.SKU == sku && (!excludedId.HasValue || x.Id != excludedId))))
            throw new BusinessException("ShopManagement:ProductSkuAlreadyExists").WithData("SKU", sku);
        if (barcode != null && await AsyncExecuter.AnyAsync(query.Where(x => x.TenantId == tenantId && x.Barcode == barcode && (!excludedId.HasValue || x.Id != excludedId))))
            throw new BusinessException("ShopManagement:ProductBarcodeAlreadyExists").WithData("Barcode", barcode);
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
    private static string NormalizeName(string value) => Check.NotNullOrWhiteSpace(value, nameof(value), ShopProductConsts.NameMaxLength).Trim();
    private static string NormalizeCode(string value) => Check.NotNullOrWhiteSpace(value, nameof(value), ShopProductConsts.CodeMaxLength).Trim();
    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : Check.Length(trimmed, nameof(value), maxLength);
    }
}
