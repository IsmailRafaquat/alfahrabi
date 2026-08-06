using System; using System.Collections.Generic; using System.Linq; using System.Linq.Dynamic.Core; using System.Threading.Tasks;
using EHub.Permissions; using Microsoft.AspNetCore.Authorization; using Volo.Abp; using Volo.Abp.Application.Dtos; using Volo.Abp.Application.Services; using Volo.Abp.Domain.Repositories;
namespace EHub.ShopManagement.Units;
[Authorize(EHubPermissions.ShopUnits.Default)]
public class ShopUnitAppService : ApplicationService, IShopUnitAppService
{
    private readonly IRepository<ShopUnit, Guid> _repository; private readonly ShopUnitManager _manager;
    public ShopUnitAppService(IRepository<ShopUnit, Guid> repository, ShopUnitManager manager) { _repository = repository; _manager = manager; }
    public async Task<PagedResultDto<ShopUnitDto>> GetListAsync(GetShopUnitsInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new(0, new List<ShopUnitDto>());
        var tenantId = CurrentTenant.Id.Value; var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.TenantId == tenantId)
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Filter!) || x.ShortName.Contains(input.Filter!))
            .WhereIf(input.AllowDecimal.HasValue, x => x.AllowDecimal == input.AllowDecimal)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
        var count = await AsyncExecuter.CountAsync(query); var sorting = input.Sorting.IsNullOrWhiteSpace() ? "Name asc" : input.Sorting!;
        var entities = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).PageBy(input));
        return new(count, ObjectMapper.Map<List<ShopUnit>, List<ShopUnitDto>>(entities));
    }
    public async Task<ShopUnitDto> GetAsync(Guid id) => ObjectMapper.Map<ShopUnit, ShopUnitDto>(await FindAsync(id));
    [Authorize(EHubPermissions.ShopUnits.Create)] public async Task<ShopUnitDto> CreateAsync(CreateUpdateShopUnitDto input)
    { var entity = await _manager.CreateAsync(input.Name, input.ShortName, input.AllowDecimal, input.IsActive); await _repository.InsertAsync(entity, true); return ObjectMapper.Map<ShopUnit, ShopUnitDto>(entity); }
    [Authorize(EHubPermissions.ShopUnits.Edit)] public async Task<ShopUnitDto> UpdateAsync(Guid id, CreateUpdateShopUnitDto input)
    { var entity = await FindAsync(id); await _manager.UpdateAsync(entity, input.Name, input.ShortName, input.AllowDecimal, input.IsActive); await _repository.UpdateAsync(entity, true); return ObjectMapper.Map<ShopUnit, ShopUnitDto>(entity); }
    [Authorize(EHubPermissions.ShopUnits.Delete)] public async Task DeleteAsync(Guid id)
    { var entity = await FindAsync(id); await _manager.ValidateDeleteAsync(id); await _repository.DeleteAsync(entity, true); }
    public async Task<ListResultDto<ShopUnitLookupDto>> GetLookupAsync()
    {
        if (!CurrentTenant.Id.HasValue) return new(new List<ShopUnitLookupDto>());
        var tenantId = CurrentTenant.Id.Value; var query = await _repository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(query.Where(x => x.TenantId == tenantId && x.IsActive).OrderBy(x => x.Name));
        return new(ObjectMapper.Map<List<ShopUnit>, List<ShopUnitLookupDto>>(entities));
    }
    private async Task<ShopUnit> FindAsync(Guid id)
    { var tenantId = CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired"); var query = await _repository.GetQueryableAsync(); return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId)) ?? throw new BusinessException("ShopManagement:UnitNotFound"); }
}
