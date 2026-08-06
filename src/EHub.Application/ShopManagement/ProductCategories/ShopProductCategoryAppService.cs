using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.ProductCategories;

[Authorize(EHubPermissions.ShopProductCategories.Default)]
public class ShopProductCategoryAppService : ApplicationService, IShopProductCategoryAppService
{
    private readonly IRepository<ShopProductCategory, Guid> _repository;
    private readonly ShopProductCategoryManager _manager;
    public ShopProductCategoryAppService(IRepository<ShopProductCategory, Guid> repository, ShopProductCategoryManager manager)
    { _repository = repository; _manager = manager; }

    public async Task<PagedResultDto<ShopProductCategoryDto>> GetListAsync(GetShopProductCategoriesInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopProductCategoryDto>(0, new List<ShopProductCategoryDto>());
        var tenantId = CurrentTenant.Id.Value;
        var source = await _repository.GetQueryableAsync();
        var filtered = source.Where(x => x.TenantId == tenantId)
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!))
            .WhereIf(input.ParentCategoryId.HasValue, x => x.ParentCategoryId == input.ParentCategoryId)
            .WhereIf(input.RootCategoriesOnly, x => x.ParentCategoryId == null)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
        var totalCount = await AsyncExecuter.CountAsync(filtered);
        var projected = from category in filtered
                        join parent in source on category.ParentCategoryId equals parent.Id into parents
                        from parent in parents.DefaultIfEmpty()
                        select new ShopProductCategoryDto
                        {
                            Id = category.Id, Name = category.Name, Code = category.Code,
                            ParentCategoryId = category.ParentCategoryId,
                            ParentCategoryName = parent == null ? null : parent.Name,
                            Description = category.Description, DisplayOrder = category.DisplayOrder,
                            IsActive = category.IsActive, CreationTime = category.CreationTime
                        };
        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "DisplayOrder asc, Name asc" : input.Sorting!;
        var items = await AsyncExecuter.ToListAsync(projected.OrderBy(sorting).PageBy(input));
        return new PagedResultDto<ShopProductCategoryDto>(totalCount, items);
    }

    public async Task<ShopProductCategoryDto> GetAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var source = await _repository.GetQueryableAsync();
        var dto = await AsyncExecuter.FirstOrDefaultAsync(
            from category in source.Where(x => x.Id == id && x.TenantId == tenantId)
            join parent in source on category.ParentCategoryId equals parent.Id into parents
            from parent in parents.DefaultIfEmpty()
            select new ShopProductCategoryDto { Id = category.Id, Name = category.Name, Code = category.Code,
                ParentCategoryId = category.ParentCategoryId, ParentCategoryName = parent == null ? null : parent.Name,
                Description = category.Description, DisplayOrder = category.DisplayOrder, IsActive = category.IsActive,
                CreationTime = category.CreationTime });
        return dto ?? throw new BusinessException("ShopManagement:ProductCategoryNotFound");
    }

    [Authorize(EHubPermissions.ShopProductCategories.Create)]
    public async Task<ShopProductCategoryDto> CreateAsync(CreateUpdateShopProductCategoryDto input)
    {
        var entity = await _manager.CreateAsync(input.Name, input.Code, input.ParentCategoryId,
            input.Description, input.DisplayOrder, input.IsActive);
        await _repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopProductCategories.Edit)]
    public async Task<ShopProductCategoryDto> UpdateAsync(Guid id, CreateUpdateShopProductCategoryDto input)
    {
        var entity = await FindEntityAsync(id);
        await _manager.UpdateAsync(entity, input.Name, input.Code, input.ParentCategoryId,
            input.Description, input.DisplayOrder, input.IsActive);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopProductCategories.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        await _manager.ValidateDeleteAsync(entity.Id);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    public async Task<ListResultDto<ShopProductCategoryLookupDto>> GetLookupAsync()
    {
        if (!CurrentTenant.Id.HasValue) return new ListResultDto<ShopProductCategoryLookupDto>(new List<ShopProductCategoryLookupDto>());
        var tenantId = CurrentTenant.Id.Value;
        var source = await _repository.GetQueryableAsync();
        var query = from category in source.Where(x => x.TenantId == tenantId && x.IsActive)
                    join parent in source on category.ParentCategoryId equals parent.Id into parents
                    from parent in parents.DefaultIfEmpty()
                    orderby category.Name
                    select new ShopProductCategoryLookupDto { Id = category.Id, Name = category.Name, Code = category.Code,
                        ParentCategoryId = category.ParentCategoryId,
                        DisplayName = parent == null ? category.Name : parent.Name + " / " + category.Name };
        return new ListResultDto<ShopProductCategoryLookupDto>(await AsyncExecuter.ToListAsync(query));
    }

    public async Task<List<ShopProductCategoryTreeDto>> GetTreeAsync()
    {
        if (!CurrentTenant.Id.HasValue) return new List<ShopProductCategoryTreeDto>();
        var tenantId = CurrentTenant.Id.Value;
        var source = await _repository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(source.Where(x => x.TenantId == tenantId));
        var nodes = entities.ToDictionary(x => x.Id, x => new ShopProductCategoryTreeDto
        { Id = x.Id, Name = x.Name, Code = x.Code, ParentCategoryId = x.ParentCategoryId, DisplayOrder = x.DisplayOrder, IsActive = x.IsActive });
        foreach (var node in nodes.Values)
            if (node.ParentCategoryId.HasValue && nodes.TryGetValue(node.ParentCategoryId.Value, out var parent)) parent.Children.Add(node);
        void Sort(List<ShopProductCategoryTreeDto> list)
        { list.Sort((a, b) => a.DisplayOrder != b.DisplayOrder ? a.DisplayOrder.CompareTo(b.DisplayOrder) : string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)); foreach (var item in list) Sort(item.Children); }
        var roots = nodes.Values.Where(x => !x.ParentCategoryId.HasValue).ToList(); Sort(roots); return roots;
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
    private async Task<ShopProductCategory> FindEntityAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId))
            ?? throw new BusinessException("ShopManagement:ProductCategoryNotFound");
    }
}
