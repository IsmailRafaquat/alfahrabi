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

namespace EHub.ShopManagement.ExpenseCategories;

[Authorize(EHubPermissions.ShopExpenseCategories.Default)]
public class ShopExpenseCategoryAppService : ApplicationService, IShopExpenseCategoryAppService
{
    private readonly IRepository<ShopExpenseCategory, Guid> _repository;
    private readonly ShopExpenseCategoryManager _manager;

    public ShopExpenseCategoryAppService(IRepository<ShopExpenseCategory, Guid> repository, ShopExpenseCategoryManager manager)
    {
        _repository = repository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopExpenseCategoryDto>> GetListAsync(GetShopExpenseCategoriesInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopExpenseCategoryDto>(0, new List<ShopExpenseCategoryDto>());
        var tenantId = CurrentTenant.Id.Value;

        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.TenantId == tenantId)
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Code.Contains(input.Filter!) || x.Name.Contains(input.Filter!))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "Name asc" : input.Sorting!;
        var entities = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).PageBy(input));
        var items = ObjectMapper.Map<List<ShopExpenseCategory>, List<ShopExpenseCategoryDto>>(entities);
        return new PagedResultDto<ShopExpenseCategoryDto>(totalCount, items);
    }

    public async Task<ShopExpenseCategoryDto> GetAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        return ObjectMapper.Map<ShopExpenseCategory, ShopExpenseCategoryDto>(entity);
    }

    [Authorize(EHubPermissions.ShopExpenseCategories.Create)]
    public async Task<ShopExpenseCategoryDto> CreateAsync(CreateUpdateShopExpenseCategoryDto input)
    {
        var entity = await _manager.CreateAsync(input.Code, input.Name, input.Description, input.IsActive);
        await _repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopExpenseCategories.Edit)]
    public async Task<ShopExpenseCategoryDto> UpdateAsync(Guid id, CreateUpdateShopExpenseCategoryDto input)
    {
        var entity = await FindEntityAsync(id);
        await _manager.UpdateAsync(entity, input.Code, input.Name, input.Description, input.IsActive);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopExpenseCategories.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        await _manager.ValidateDeleteAsync(entity);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    public async Task<ListResultDto<ShopExpenseCategoryLookupDto>> GetLookupAsync(string? filter = null)
    {
        if (!CurrentTenant.Id.HasValue) return new ListResultDto<ShopExpenseCategoryLookupDto>(new List<ShopExpenseCategoryLookupDto>());
        var tenantId = CurrentTenant.Id.Value;

        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.TenantId == tenantId && x.IsActive)
            .WhereIf(!filter.IsNullOrWhiteSpace(), x => x.Code.Contains(filter!) || x.Name.Contains(filter!));

        var entities = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.Name));
        var items = ObjectMapper.Map<List<ShopExpenseCategory>, List<ShopExpenseCategoryLookupDto>>(entities);
        return new ListResultDto<ShopExpenseCategoryLookupDto>(items);
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopExpenseCategory> FindEntityAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId))
            ?? throw new BusinessException("ShopManagement:ExpenseCategoryNotFound");
    }
}
