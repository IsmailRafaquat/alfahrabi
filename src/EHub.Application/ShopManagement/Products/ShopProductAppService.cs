using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.Units;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.Products;

[Authorize(EHubPermissions.ShopProducts.Default)]
public class ShopProductAppService : ApplicationService, IShopProductAppService
{
    private readonly IRepository<ShopProduct, Guid> _repository;
    private readonly IRepository<ShopProductCategory, Guid> _categoryRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly ShopProductManager _manager;

    public ShopProductAppService(
        IRepository<ShopProduct, Guid> repository,
        IRepository<ShopProductCategory, Guid> categoryRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        ShopProductManager manager)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _unitRepository = unitRepository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopProductDto>> GetListAsync(GetShopProductsInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopProductDto>(0, new List<ShopProductDto>());
        var tenantId = CurrentTenant.Id.Value;

        var products = await _repository.GetQueryableAsync();
        var categories = await _categoryRepository.GetQueryableAsync();
        var units = await _unitRepository.GetQueryableAsync();

        var filtered = products.Where(x => x.TenantId == tenantId)
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x =>
                x.Name.Contains(input.Filter!) ||
                x.Code.Contains(input.Filter!) ||
                (x.SKU != null && x.SKU.Contains(input.Filter!)) ||
                (x.Barcode != null && x.Barcode.Contains(input.Filter!)) ||
                (x.Brand != null && x.Brand.Contains(input.Filter!)) ||
                (x.Model != null && x.Model.Contains(input.Filter!)))
            .WhereIf(input.CategoryId.HasValue, x => x.CategoryId == input.CategoryId)
            .WhereIf(input.UnitId.HasValue, x => x.UnitId == input.UnitId)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive)
            .WhereIf(input.IsTaxable.HasValue, x => x.IsTaxable == input.IsTaxable)
            .WhereIf(input.TrackBatch.HasValue, x => x.TrackBatch == input.TrackBatch)
            .WhereIf(input.TrackExpiry.HasValue, x => x.TrackExpiry == input.TrackExpiry)
            .WhereIf(input.LowStockOnly, x => x.CurrentStock <= x.MinimumStockLevel);

        var totalCount = await AsyncExecuter.CountAsync(filtered);

        var projected = from product in filtered
                        join category in categories on product.CategoryId equals category.Id
                        join unit in units on product.UnitId equals unit.Id
                        select new ShopProductDto
                        {
                            Id = product.Id,
                            CategoryId = product.CategoryId,
                            CategoryName = category.Name,
                            UnitId = product.UnitId,
                            UnitName = unit.Name,
                            UnitShortName = unit.ShortName,
                            UnitAllowDecimal = unit.AllowDecimal,
                            Name = product.Name,
                            Code = product.Code,
                            SKU = product.SKU,
                            Barcode = product.Barcode,
                            Description = product.Description,
                            Brand = product.Brand,
                            Model = product.Model,
                            PurchasePrice = product.PurchasePrice,
                            SalePrice = product.SalePrice,
                            WholesalePrice = product.WholesalePrice,
                            MinimumSalePrice = product.MinimumSalePrice,
                            TaxPercentage = product.TaxPercentage,
                            CurrentStock = product.CurrentStock,
                            MinimumStockLevel = product.MinimumStockLevel,
                            MaximumStockLevel = product.MaximumStockLevel,
                            ReorderLevel = product.ReorderLevel,
                            TrackBatch = product.TrackBatch,
                            TrackExpiry = product.TrackExpiry,
                            TrackSerialNumber = product.TrackSerialNumber,
                            IsTaxable = product.IsTaxable,
                            IsActive = product.IsActive,
                            CreationTime = product.CreationTime
                        };

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "Name asc" : input.Sorting!;
        var items = await AsyncExecuter.ToListAsync(projected.OrderBy(sorting).PageBy(input));

        await HideCostIfNotAllowedAsync(items);
        return new PagedResultDto<ShopProductDto>(totalCount, items);
    }

    public async Task<ShopProductDto> GetAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var dto = await FindDtoAsync(id, tenantId) ?? throw new BusinessException("ShopManagement:ProductNotFound");
        await HideCostIfNotAllowedAsync(new List<ShopProductDto> { dto });
        return dto;
    }

    [Authorize(EHubPermissions.ShopProducts.Create)]
    public async Task<ShopProductDto> CreateAsync(CreateShopProductDto input)
    {
        var entity = await _manager.CreateAsync(input.CategoryId, input.UnitId, input.Name, input.Code, input.SKU,
            input.Barcode, input.Description, input.Brand, input.Model, input.PurchasePrice, input.SalePrice,
            input.WholesalePrice, input.MinimumSalePrice, input.TaxPercentage, input.MinimumStockLevel,
            input.MaximumStockLevel, input.ReorderLevel, input.TrackBatch, input.TrackExpiry,
            input.TrackSerialNumber, input.IsTaxable, input.IsActive);
        await _repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopProducts.Edit)]
    public async Task<ShopProductDto> UpdateAsync(Guid id, UpdateShopProductDto input)
    {
        var entity = await FindEntityAsync(id);
        await _manager.UpdateAsync(entity, input.CategoryId, input.UnitId, input.Name, input.Code, input.SKU,
            input.Barcode, input.Description, input.Brand, input.Model, input.PurchasePrice, input.SalePrice,
            input.WholesalePrice, input.MinimumSalePrice, input.TaxPercentage, input.MinimumStockLevel,
            input.MaximumStockLevel, input.ReorderLevel, input.TrackBatch, input.TrackExpiry,
            input.TrackSerialNumber, input.IsTaxable, input.IsActive);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopProducts.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        await _manager.ValidateDeleteAsync(entity.Id);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    public async Task<ListResultDto<ShopProductLookupDto>> GetLookupAsync(string? filter = null, Guid? categoryId = null)
    {
        if (!CurrentTenant.Id.HasValue) return new ListResultDto<ShopProductLookupDto>(new List<ShopProductLookupDto>());
        var tenantId = CurrentTenant.Id.Value;

        var products = await _repository.GetQueryableAsync();
        var categories = await _categoryRepository.GetQueryableAsync();
        var units = await _unitRepository.GetQueryableAsync();

        var query = from product in products.Where(x => x.TenantId == tenantId && x.IsActive)
                        .WhereIf(!filter.IsNullOrWhiteSpace(), x => x.Name.Contains(filter!) || x.Code.Contains(filter!))
                        .WhereIf(categoryId.HasValue, x => x.CategoryId == categoryId)
                    join category in categories on product.CategoryId equals category.Id
                    join unit in units on product.UnitId equals unit.Id
                    orderby product.Name
                    select new ShopProductLookupDto
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Code = product.Code,
                        Barcode = product.Barcode,
                        CategoryName = category.Name,
                        UnitName = unit.Name,
                        UnitShortName = unit.ShortName,
                        UnitAllowDecimal = unit.AllowDecimal,
                        SalePrice = product.SalePrice,
                        CurrentStock = product.CurrentStock,
                        IsActive = product.IsActive
                    };

        return new ListResultDto<ShopProductLookupDto>(await AsyncExecuter.ToListAsync(query));
    }

    private async Task HideCostIfNotAllowedAsync(List<ShopProductDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopProducts.ViewCost)) return;
        foreach (var item in items)
        {
            item.PurchasePrice = null;
            item.MinimumSalePrice = null;
        }
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopProduct> FindEntityAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId))
            ?? throw new BusinessException("ShopManagement:ProductNotFound");
    }

    private async Task<ShopProductDto?> FindDtoAsync(Guid id, Guid tenantId)
    {
        var products = await _repository.GetQueryableAsync();
        var categories = await _categoryRepository.GetQueryableAsync();
        var units = await _unitRepository.GetQueryableAsync();

        var query = from product in products.Where(x => x.Id == id && x.TenantId == tenantId)
                    join category in categories on product.CategoryId equals category.Id
                    join unit in units on product.UnitId equals unit.Id
                    select new ShopProductDto
                    {
                        Id = product.Id,
                        CategoryId = product.CategoryId,
                        CategoryName = category.Name,
                        UnitId = product.UnitId,
                        UnitName = unit.Name,
                        UnitShortName = unit.ShortName,
                        UnitAllowDecimal = unit.AllowDecimal,
                        Name = product.Name,
                        Code = product.Code,
                        SKU = product.SKU,
                        Barcode = product.Barcode,
                        Description = product.Description,
                        Brand = product.Brand,
                        Model = product.Model,
                        PurchasePrice = product.PurchasePrice,
                        SalePrice = product.SalePrice,
                        WholesalePrice = product.WholesalePrice,
                        MinimumSalePrice = product.MinimumSalePrice,
                        TaxPercentage = product.TaxPercentage,
                        CurrentStock = product.CurrentStock,
                        MinimumStockLevel = product.MinimumStockLevel,
                        MaximumStockLevel = product.MaximumStockLevel,
                        ReorderLevel = product.ReorderLevel,
                        TrackBatch = product.TrackBatch,
                        TrackExpiry = product.TrackExpiry,
                        TrackSerialNumber = product.TrackSerialNumber,
                        IsTaxable = product.IsTaxable,
                        IsActive = product.IsActive,
                        CreationTime = product.CreationTime
                    };

        return await AsyncExecuter.FirstOrDefaultAsync(query);
    }
}
