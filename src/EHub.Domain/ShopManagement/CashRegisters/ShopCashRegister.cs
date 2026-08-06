using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.CashRegisters;

public class ShopCashRegister : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string Code { get; protected set; } = string.Empty;
    public string Name { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }
    public bool IsDefault { get; protected set; }
    public bool IsActive { get; protected set; } = true;

    protected ShopCashRegister() { }

    internal ShopCashRegister(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        string? description,
        bool isDefault,
        bool isActive) : base(id)
    {
        TenantId = tenantId;
        SetValues(code, name, description, isDefault, isActive);
    }

    internal void Update(string code, string name, string? description, bool isDefault, bool isActive) =>
        SetValues(code, name, description, isDefault, isActive);

    internal void SetAsNotDefault()
    {
        IsDefault = false;
    }

    private void SetValues(string code, string name, string? description, bool isDefault, bool isActive)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ShopCashRegisterConsts.CodeMaxLength).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ShopCashRegisterConsts.NameMaxLength).Trim();
        Description = Check.Length(description?.Trim(), nameof(description), ShopCashRegisterConsts.DescriptionMaxLength);
        IsDefault = isDefault;
        IsActive = isActive;
    }
}
