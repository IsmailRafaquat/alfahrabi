using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.ProductCategories;

public class ShopProductCategory : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public string Name { get; protected set; } = string.Empty;
    public string Code { get; protected set; } = string.Empty;
    public Guid? ParentCategoryId { get; protected set; }
    public ShopProductCategory? ParentCategory { get; protected set; }
    public ICollection<ShopProductCategory> ChildCategories { get; protected set; } = new List<ShopProductCategory>();
    public string? Description { get; protected set; }
    public int DisplayOrder { get; protected set; }
    public bool IsActive { get; protected set; } = true;

    protected ShopProductCategory() { }
    internal ShopProductCategory(Guid id, Guid tenantId, string name, string code, Guid? parentCategoryId,
        string? description, int displayOrder, bool isActive) : base(id)
    {
        TenantId = tenantId;
        SetValues(name, code, parentCategoryId, description, displayOrder, isActive);
    }

    internal void Update(string name, string code, Guid? parentCategoryId, string? description,
        int displayOrder, bool isActive) => SetValues(name, code, parentCategoryId, description, displayOrder, isActive);

    private void SetValues(string name, string code, Guid? parentCategoryId, string? description, int displayOrder, bool isActive)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ShopProductCategoryConsts.NameMaxLength).Trim();
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ShopProductCategoryConsts.CodeMaxLength).Trim().ToUpperInvariant();
        Description = Check.Length(description?.Trim(), nameof(description), ShopProductCategoryConsts.DescriptionMaxLength);
        if (displayOrder < 0) throw new BusinessException("ShopManagement:ProductCategoryInvalidDisplayOrder");
        ParentCategoryId = parentCategoryId;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }
}
