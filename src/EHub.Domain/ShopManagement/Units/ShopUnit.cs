using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
namespace EHub.ShopManagement.Units;
public class ShopUnit : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public string Name { get; protected set; } = string.Empty;
    public string ShortName { get; protected set; } = string.Empty;
    public bool AllowDecimal { get; protected set; }
    public bool IsActive { get; protected set; } = true;
    protected ShopUnit() { }
    internal ShopUnit(Guid id, Guid tenantId, string name, string shortName, bool allowDecimal, bool isActive) : base(id)
    { TenantId = tenantId; SetValues(name, shortName, allowDecimal, isActive); }
    internal void Update(string name, string shortName, bool allowDecimal, bool isActive) => SetValues(name, shortName, allowDecimal, isActive);
    private void SetValues(string name, string shortName, bool allowDecimal, bool isActive)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ShopUnitConsts.NameMaxLength).Trim();
        ShortName = Check.NotNullOrWhiteSpace(shortName, nameof(shortName), ShopUnitConsts.ShortNameMaxLength).Trim();
        AllowDecimal = allowDecimal; IsActive = isActive;
    }
}
