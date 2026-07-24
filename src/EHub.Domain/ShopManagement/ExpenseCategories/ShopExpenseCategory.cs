using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.ExpenseCategories;

public class ShopExpenseCategory : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string Code { get; protected set; } = string.Empty;
    public string Name { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }
    public bool IsActive { get; protected set; } = true;

    protected ShopExpenseCategory() { }

    internal ShopExpenseCategory(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        string? description,
        bool isActive) : base(id)
    {
        TenantId = tenantId;
        SetValues(code, name, description, isActive);
    }

    internal void Update(string code, string name, string? description, bool isActive) =>
        SetValues(code, name, description, isActive);

    private void SetValues(string code, string name, string? description, bool isActive)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ShopExpenseCategoryConsts.CodeMaxLength).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ShopExpenseCategoryConsts.NameMaxLength).Trim();
        Description = Check.Length(description?.Trim(), nameof(description), ShopExpenseCategoryConsts.DescriptionMaxLength);
        IsActive = isActive;
    }
}
