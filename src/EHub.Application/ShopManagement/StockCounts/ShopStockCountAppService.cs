using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.StockAdjustments;
using EHub.ShopManagement.Units;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.StockCounts;

[Authorize(EHubPermissions.ShopStockCounts.Default)]
public class ShopStockCountAppService : ApplicationService, IShopStockCountAppService
{
    private readonly IRepository<ShopStockCount, Guid> _repository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly IRepository<ShopProductCategory, Guid> _categoryRepository;
    private readonly IRepository<ShopStockAdjustment, Guid> _stockAdjustmentRepository;
    private readonly ShopStockCountManager _manager;

    public ShopStockCountAppService(
        IRepository<ShopStockCount, Guid> repository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        IRepository<ShopProductCategory, Guid> categoryRepository,
        IRepository<ShopStockAdjustment, Guid> stockAdjustmentRepository,
        ShopStockCountManager manager)
    {
        _repository = repository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
        _categoryRepository = categoryRepository;
        _stockAdjustmentRepository = stockAdjustmentRepository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopStockCountDto>> GetListAsync(GetShopStockCountsInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopStockCountDto>(0, new List<ShopStockCountDto>());
        var tenantId = CurrentTenant.Id.Value;

        var query = await _repository.WithDetailsAsync(x => x.Items);
        var filtered = query.Where(x => x.TenantId == tenantId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.Scope.HasValue, x => x.Scope == input.Scope)
            .WhereIf(input.ProductCategoryId.HasValue, x => x.ProductCategoryId == input.ProductCategoryId)
            .WhereIf(input.CountDateFrom.HasValue, x => x.CountDate >= input.CountDateFrom!.Value)
            .WhereIf(input.CountDateTo.HasValue, x => x.CountDate <= input.CountDateTo!.Value)
            .WhereIf(input.ProductId.HasValue, x => x.Items.Any(i => i.ProductId == input.ProductId!.Value))
            .WhereIf(input.HasDifferences == true, x => x.Items.Any(i => i.DifferenceQuantity != 0))
            .WhereIf(input.HasDifferences == false, x => x.Items.All(i => i.DifferenceQuantity == 0));

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            filtered = filtered.Where(x =>
                x.StockCountNumber.Contains(input.Filter!) ||
                (x.Notes != null && x.Notes.Contains(input.Filter!)) ||
                x.Items.Any(i => i.ProductCodeSnapshot.Contains(input.Filter!) || i.ProductNameSnapshot.Contains(input.Filter!)));
        }

        var totalCount = await AsyncExecuter.CountAsync(filtered);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "CountDate desc, CreationTime desc" : input.Sorting!;
        var pagedStockCounts = await AsyncExecuter.ToListAsync(filtered.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = await BuildDtosAsync(pagedStockCounts);
        return new PagedResultDto<ShopStockCountDto>(totalCount, dtos);
    }

    public async Task<ShopStockCountDto> GetAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        var dtos = await BuildDtosAsync(new List<ShopStockCount> { entity });
        return dtos[0];
    }

    public async Task<ListResultDto<ShopStockCountProductLookupDto>> GetProductLookupAsync(string? filter = null)
    {
        var tenantId = RequireTenant();
        var productQuery = await _productRepository.GetQueryableAsync();
        var filtered = productQuery.Where(x => x.TenantId == tenantId && x.IsActive);

        if (!filter.IsNullOrWhiteSpace())
        {
            filtered = filtered.Where(x => x.Name.Contains(filter!) || x.Code.Contains(filter!));
        }

        var products = await AsyncExecuter.ToListAsync(filtered.OrderBy(x => x.Name).Take(200));
        var unitIds = products.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = await _unitRepository.GetQueryableAsync();
        var units = unitQuery.Where(x => unitIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var result = products.Select(p =>
        {
            units.TryGetValue(p.UnitId, out var unit);
            return new ShopStockCountProductLookupDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                CategoryId = p.CategoryId,
                UnitName = unit?.Name ?? string.Empty,
                UnitShortName = unit?.ShortName ?? string.Empty,
                UnitAllowDecimal = unit?.AllowDecimal ?? false,
                CurrentStock = p.CurrentStock,
                TrackBatch = p.TrackBatch,
                TrackExpiry = p.TrackExpiry,
                TrackSerialNumber = p.TrackSerialNumber
            };
        }).ToList();

        return new ListResultDto<ShopStockCountProductLookupDto>(result);
    }

    [Authorize(EHubPermissions.ShopStockCounts.Create)]
    public async Task<ShopStockCountDto> CreateAsync(CreateShopStockCountDto input)
    {
        var entity = await _manager.CreateAsync(input.CountDate, input.Scope, input.ProductCategoryId, input.SelectedProductIds, input.Notes);
        await _repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopStockCounts.Edit)]
    public async Task<ShopStockCountDto> UpdateAsync(Guid id, UpdateShopStockCountDto input)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.UpdateAsync(entity, input.CountDate, input.Scope, input.ProductCategoryId, input.SelectedProductIds, input.Notes);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopStockCounts.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.ValidateDeleteAsync(entity);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    [Authorize(EHubPermissions.ShopStockCounts.Start)]
    public async Task<ShopStockCountDto> StartAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.StartAsync(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopStockCounts.Count)]
    public async Task<ShopStockCountDto> UpdateItemQuantityAsync(Guid id, UpdateShopStockCountItemQuantityDto input)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.UpdateItemQuantityAsync(entity, input.StockCountItemId, input.PhysicalQuantity, input.Notes);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopStockCounts.Complete)]
    public async Task<ShopStockCountDto> CompleteCountAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.CompleteCountAsync(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    public async Task<ShopStockCountPostingPreviewDto> GetPostingPreviewAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var entity = await FindEntityWithItemsAsync(id);

        var productIds = entity.Items.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id) && x.TenantId == tenantId).ToList().ToDictionary(x => x.Id);

        var changedProducts = new List<string>();
        foreach (var item in entity.Items)
        {
            if (products.TryGetValue(item.ProductId, out var product) && product.CurrentStock != item.SystemQuantitySnapshot)
                changedProducts.Add(product.Name);
        }

        var increaseItems = entity.Items.Where(x => x.AdjustmentType == ShopStockAdjustmentType.Increase).ToList();
        var decreaseItems = entity.Items.Where(x => x.AdjustmentType == ShopStockAdjustmentType.Decrease).ToList();

        return new ShopStockCountPostingPreviewDto
        {
            TotalProducts = entity.Items.Count,
            NoDifferenceCount = entity.Items.Count(x => x.DifferenceQuantity == 0),
            IncreaseCount = increaseItems.Count,
            DecreaseCount = decreaseItems.Count,
            TotalIncreaseQuantity = increaseItems.Sum(x => x.DifferenceQuantity),
            TotalDecreaseQuantity = decreaseItems.Sum(x => Math.Abs(x.DifferenceQuantity)),
            CanPost = changedProducts.Count == 0,
            ChangedProducts = changedProducts
        };
    }

    [Authorize(EHubPermissions.ShopStockCounts.Post)]
    public async Task<ShopStockCountDto> PostAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.PostAsync(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopStockCounts.Cancel)]
    public async Task<ShopStockCountDto> CancelAsync(Guid id, CancelShopStockCountDto input)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.CancelAsync(entity, input.CancellationReason);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    private async Task<List<ShopStockCountDto>> BuildDtosAsync(List<ShopStockCount> entities)
    {
        var categoryIds = entities.Where(x => x.ProductCategoryId.HasValue).Select(x => x.ProductCategoryId!.Value).Distinct().ToList();
        var categoryQuery = await _categoryRepository.GetQueryableAsync();
        var categories = categoryQuery.Where(x => categoryIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var adjustmentIds = entities.Where(x => x.GeneratedStockAdjustmentId.HasValue).Select(x => x.GeneratedStockAdjustmentId!.Value).Distinct().ToList();
        var adjustmentQuery = await _stockAdjustmentRepository.GetQueryableAsync();
        var adjustments = adjustmentQuery.Where(x => adjustmentIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var productIds = entities.SelectMany(x => x.Items).Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);
        var unitIds = products.Values.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = await _unitRepository.GetQueryableAsync();
        var units = unitQuery.Where(x => unitIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        return entities.Select(entity =>
        {
            ShopProductCategory? category = null;
            if (entity.ProductCategoryId.HasValue) categories.TryGetValue(entity.ProductCategoryId.Value, out category);

            ShopStockAdjustment? adjustment = null;
            if (entity.GeneratedStockAdjustmentId.HasValue) adjustments.TryGetValue(entity.GeneratedStockAdjustmentId.Value, out adjustment);

            var itemDtos = entity.Items.Select(item =>
            {
                var allowDecimal = false;
                if (products.TryGetValue(item.ProductId, out var product) && units.TryGetValue(product.UnitId, out var unit))
                    allowDecimal = unit.AllowDecimal;

                return new ShopStockCountItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductCode = item.ProductCodeSnapshot,
                    ProductName = item.ProductNameSnapshot,
                    UnitName = item.UnitNameSnapshot,
                    UnitShortName = item.UnitShortNameSnapshot,
                    UnitAllowDecimal = allowDecimal,
                    ProductBatchId = item.ProductBatchId,
                    BatchNumber = item.BatchNumberSnapshot,
                    ExpiryDate = item.ExpiryDateSnapshot,
                    SystemQuantity = item.SystemQuantitySnapshot,
                    PhysicalQuantity = item.PhysicalQuantity,
                    DifferenceQuantity = item.DifferenceQuantity,
                    AdjustmentType = item.AdjustmentType,
                    IsCounted = item.IsCounted,
                    Notes = item.Notes
                };
            }).ToList();

            return new ShopStockCountDto
            {
                Id = entity.Id,
                StockCountNumber = entity.StockCountNumber,
                CountDate = entity.CountDate,
                Scope = entity.Scope,
                ProductCategoryId = entity.ProductCategoryId,
                ProductCategoryName = category?.Name,
                Status = entity.Status,
                Notes = entity.Notes,
                GeneratedStockAdjustmentId = entity.GeneratedStockAdjustmentId,
                GeneratedStockAdjustmentNumber = adjustment?.AdjustmentNumber,
                StartedDate = entity.StartedDate,
                CountedDate = entity.CountedDate,
                PostedDate = entity.PostedDate,
                CancelledDate = entity.CancelledDate,
                CancellationReason = entity.CancellationReason,
                CreationTime = entity.CreationTime,
                TotalItems = itemDtos.Count,
                CountedItems = itemDtos.Count(x => x.IsCounted),
                DifferenceItems = itemDtos.Count(x => x.DifferenceQuantity != 0),
                TotalIncreaseQuantity = itemDtos.Where(x => x.AdjustmentType == ShopStockAdjustmentType.Increase).Sum(x => x.DifferenceQuantity),
                TotalDecreaseQuantity = itemDtos.Where(x => x.AdjustmentType == ShopStockAdjustmentType.Decrease).Sum(x => Math.Abs(x.DifferenceQuantity)),
                Items = itemDtos
            };
        }).ToList();
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopStockCount> FindEntityWithItemsAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.WithDetailsAsync(x => x.Items);
        var entity = query.FirstOrDefault(x => x.Id == id && x.TenantId == tenantId);
        return entity ?? throw new BusinessException("ShopManagement:StockCountNotFound");
    }
}
