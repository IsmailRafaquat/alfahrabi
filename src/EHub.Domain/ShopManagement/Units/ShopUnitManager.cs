using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
namespace EHub.ShopManagement.Units;
public class ShopUnitManager : DomainService
{
    private readonly IRepository<ShopUnit, Guid> _repository; private readonly ICurrentTenant _currentTenant;
    public ShopUnitManager(IRepository<ShopUnit, Guid> repository, ICurrentTenant currentTenant) { _repository = repository; _currentTenant = currentTenant; }
    public async Task<ShopUnit> CreateAsync(string name, string shortName, bool allowDecimal, bool isActive)
    {
        var tenantId = RequireTenant(); var normalizedName = NormalizeName(name); var normalizedShortName = NormalizeShortName(shortName);
        await ValidateUniqueAsync(normalizedName, normalizedShortName, tenantId, null);
        return new ShopUnit(GuidGenerator.Create(), tenantId, normalizedName, normalizedShortName, allowDecimal, isActive);
    }
    public async Task UpdateAsync(ShopUnit unit, string name, string shortName, bool allowDecimal, bool isActive)
    {
        var tenantId = RequireTenant(); if (unit.TenantId != tenantId) throw new BusinessException("ShopManagement:UnitNotFound");
        var normalizedName = NormalizeName(name); var normalizedShortName = NormalizeShortName(shortName);
        await ValidateUniqueAsync(normalizedName, normalizedShortName, tenantId, unit.Id);
        unit.Update(normalizedName, normalizedShortName, allowDecimal, isActive);
    }
    public Task ValidateDeleteAsync(Guid id) { RequireTenant(); return Task.CompletedTask; }
    private async Task ValidateUniqueAsync(string name, string shortName, Guid tenantId, Guid? excludedId)
    {
        var query = await _repository.GetQueryableAsync();
        if (await AsyncExecuter.AnyAsync(query.Where(x => x.TenantId == tenantId && x.Name == name && (!excludedId.HasValue || x.Id != excludedId))))
            throw new BusinessException("ShopManagement:UnitNameAlreadyExists").WithData("Name", name);
        if (await AsyncExecuter.AnyAsync(query.Where(x => x.TenantId == tenantId && x.ShortName == shortName && (!excludedId.HasValue || x.Id != excludedId))))
            throw new BusinessException("ShopManagement:UnitShortNameAlreadyExists").WithData("ShortName", shortName);
    }
    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
    private static string NormalizeName(string value) => Check.NotNullOrWhiteSpace(value, nameof(value), ShopUnitConsts.NameMaxLength).Trim();
    private static string NormalizeShortName(string value) => Check.NotNullOrWhiteSpace(value, nameof(value), ShopUnitConsts.ShortNameMaxLength).Trim();
}
