using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.Units;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.StockAdjustments;

[Authorize(EHubPermissions.ShopStockAdjustments.Default)]
public class ShopStockAdjustmentAppService : ApplicationService, IShopStockAdjustmentAppService
{
    private readonly IRepository<ShopStockAdjustment, Guid> _repository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly ShopStockAdjustmentManager _manager;

    public ShopStockAdjustmentAppService(
        IRepository<ShopStockAdjustment, Guid> repository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        ShopStockAdjustmentManager manager)
    {
        _repository = repository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopStockAdjustmentDto>> GetListAsync(GetShopStockAdjustmentsInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopStockAdjustmentDto>(0, new List<ShopStockAdjustmentDto>());
        var tenantId = CurrentTenant.Id.Value;

        var query = await _repository.WithDetailsAsync(x => x.Items);
        var filtered = query.Where(x => x.TenantId == tenantId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.Reason.HasValue, x => x.Reason == input.Reason)
            .WhereIf(input.AdjustmentDateFrom.HasValue, x => x.AdjustmentDate >= input.AdjustmentDateFrom!.Value)
            .WhereIf(input.AdjustmentDateTo.HasValue, x => x.AdjustmentDate <= input.AdjustmentDateTo!.Value)
            .WhereIf(input.ProductId.HasValue, x => x.Items.Any(i => i.ProductId == input.ProductId!.Value))
            .WhereIf(input.AdjustmentType.HasValue, x => x.Items.Any(i => i.AdjustmentType == input.AdjustmentType!.Value));

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            filtered = filtered.Where(x => x.AdjustmentNumber.Contains(input.Filter!));
        }

        var totalCount = await AsyncExecuter.CountAsync(filtered);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "AdjustmentDate desc, CreationTime desc" : input.Sorting!;
        var pagedAdjustments = await AsyncExecuter.ToListAsync(filtered.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = await BuildDtosAsync(pagedAdjustments);
        return new PagedResultDto<ShopStockAdjustmentDto>(totalCount, dtos);
    }

    public async Task<ShopStockAdjustmentDto> GetAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        var dtos = await BuildDtosAsync(new List<ShopStockAdjustment> { entity });
        return dtos[0];
    }

    public async Task<ListResultDto<ShopStockAdjustmentProductLookupDto>> GetProductLookupAsync(string? filter = null)
    {
        var tenantId = RequireTenant();
        var productQuery = await _productRepository.GetQueryableAsync();
        var filtered = productQuery.Where(x => x.TenantId == tenantId && x.IsActive);

        if (!filter.IsNullOrWhiteSpace())
        {
            filtered = filtered.Where(x => x.Name.Contains(filter!) || x.Code.Contains(filter!));
        }

        var products = await AsyncExecuter.ToListAsync(filtered.OrderBy(x => x.Name).Take(50));
        var unitIds = products.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = await _unitRepository.GetQueryableAsync();
        var units = unitQuery.Where(x => unitIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var result = products.Select(p =>
        {
            units.TryGetValue(p.UnitId, out var unit);
            return new ShopStockAdjustmentProductLookupDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                UnitId = p.UnitId,
                UnitName = unit?.Name ?? string.Empty,
                UnitShortName = unit?.ShortName ?? string.Empty,
                UnitAllowDecimal = unit?.AllowDecimal ?? false,
                CurrentStock = p.CurrentStock,
                TrackBatch = p.TrackBatch,
                TrackExpiry = p.TrackExpiry,
                TrackSerialNumber = p.TrackSerialNumber
            };
        }).ToList();

        return new ListResultDto<ShopStockAdjustmentProductLookupDto>(result);
    }

    [Authorize(EHubPermissions.ShopStockAdjustments.Create)]
    public async Task<ShopStockAdjustmentDto> CreateAsync(CreateShopStockAdjustmentDto input)
    {
        var entity = await _manager.CreateAsync(input.AdjustmentDate, input.Reason, input.ReasonDetails, input.Notes, ToItemInputs(input.Items));
        await _repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopStockAdjustments.Edit)]
    public async Task<ShopStockAdjustmentDto> UpdateAsync(Guid id, UpdateShopStockAdjustmentDto input)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.UpdateAsync(entity, input.AdjustmentDate, input.Reason, input.ReasonDetails, input.Notes, ToItemInputs(input.Items));
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopStockAdjustments.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.ValidateDeleteAsync(entity);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    [Authorize(EHubPermissions.ShopStockAdjustments.Post)]
    public async Task<ShopStockAdjustmentDto> PostAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.PostAsync(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopStockAdjustments.Cancel)]
    public async Task<ShopStockAdjustmentDto> CancelAsync(Guid id, CancelShopStockAdjustmentDto input)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.CancelAsync(entity, input.CancellationReason);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    private static List<ShopStockAdjustmentItemInput> ToItemInputs(IEnumerable<CreateShopStockAdjustmentItemDto> items) =>
        items.Select(x => new ShopStockAdjustmentItemInput
        {
            ProductId = x.ProductId,
            AdjustmentType = x.AdjustmentType,
            AdjustmentQuantity = x.AdjustmentQuantity,
            BatchNumber = x.BatchNumber,
            ExpiryDate = x.ExpiryDate,
            Reason = x.Reason,
            Notes = x.Notes
        }).ToList();

    private async Task<List<ShopStockAdjustmentDto>> BuildDtosAsync(List<ShopStockAdjustment> entities)
    {
        var dtos = entities.Select(MapToDto).ToList();
        await PopulateUnitAllowDecimalAsync(dtos);
        await HideCostIfNotAllowedAsync(dtos);
        return dtos;
    }

    private static ShopStockAdjustmentDto MapToDto(ShopStockAdjustment entity) => new()
    {
        Id = entity.Id,
        AdjustmentNumber = entity.AdjustmentNumber,
        AdjustmentDate = entity.AdjustmentDate,
        Status = entity.Status,
        Reason = entity.Reason,
        ReasonDetails = entity.ReasonDetails,
        Notes = entity.Notes,
        PostedByUserId = entity.PostedByUserId,
        PostedDate = entity.PostedDate,
        CancelledByUserId = entity.CancelledByUserId,
        CancelledDate = entity.CancelledDate,
        CancellationReason = entity.CancellationReason,
        CreationTime = entity.CreationTime,
        Items = entity.Items.Select(MapItem).ToList()
    };

    private static ShopStockAdjustmentItemDto MapItem(ShopStockAdjustmentItem item) => new()
    {
        Id = item.Id,
        ProductId = item.ProductId,
        ProductCode = item.ProductCodeSnapshot,
        ProductName = item.ProductNameSnapshot,
        UnitName = item.UnitNameSnapshot,
        UnitShortName = item.UnitShortNameSnapshot,
        AdjustmentType = item.AdjustmentType,
        SystemQuantity = item.SystemQuantitySnapshot,
        AdjustmentQuantity = item.AdjustmentQuantity,
        FinalQuantity = item.FinalQuantity,
        UnitCostSnapshot = item.UnitCostSnapshot,
        BatchNumber = item.BatchNumber,
        ExpiryDate = item.ExpiryDate,
        Reason = item.Reason,
        Notes = item.Notes
    };

    private async Task PopulateUnitAllowDecimalAsync(List<ShopStockAdjustmentDto> dtos)
    {
        var productIds = dtos.SelectMany(x => x.Items).Select(x => x.ProductId).Distinct().ToList();
        if (productIds.Count == 0) return;

        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);
        var unitIds = products.Values.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = await _unitRepository.GetQueryableAsync();
        var units = unitQuery.Where(x => unitIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        foreach (var item in dtos.SelectMany(x => x.Items))
        {
            if (products.TryGetValue(item.ProductId, out var product) && units.TryGetValue(product.UnitId, out var unit))
            {
                item.UnitAllowDecimal = unit.AllowDecimal;
            }
        }
    }

    private async Task HideCostIfNotAllowedAsync(List<ShopStockAdjustmentDto> dtos)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopStockAdjustments.ViewCost)) return;
        foreach (var dto in dtos)
            foreach (var item in dto.Items)
                item.UnitCostSnapshot = null;
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopStockAdjustment> FindEntityWithItemsAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.WithDetailsAsync(x => x.Items);
        var entity = query.FirstOrDefault(x => x.Id == id && x.TenantId == tenantId);
        return entity ?? throw new BusinessException("ShopManagement:StockAdjustmentNotFound");
    }
}
