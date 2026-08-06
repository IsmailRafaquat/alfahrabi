using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.ProductCategories;

public class ShopProductCategoryManager : DomainService
{
    private readonly IRepository<ShopProductCategory, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    public ShopProductCategoryManager(IRepository<ShopProductCategory, Guid> repository, ICurrentTenant currentTenant)
    { _repository = repository; _currentTenant = currentTenant; }

    public async Task<ShopProductCategory> CreateAsync(string name, string code, Guid? parentCategoryId,
        string? description, int displayOrder, bool isActive)
    {
        var tenantId = RequireTenant();
        var normalizedName = NormalizeName(name); var normalizedCode = NormalizeCode(code);
        await ValidateParentAsync(parentCategoryId, tenantId);
        await ValidateUniqueAsync(normalizedName, normalizedCode, parentCategoryId, tenantId, null);
        return new ShopProductCategory(GuidGenerator.Create(), tenantId, normalizedName, normalizedCode,
            parentCategoryId, description, displayOrder, isActive);
    }

    public async Task UpdateAsync(ShopProductCategory category, string name, string code, Guid? parentCategoryId,
        string? description, int displayOrder, bool isActive)
    {
        var tenantId = RequireTenant();
        if (category.TenantId != tenantId) throw new BusinessException("ShopManagement:ProductCategoryNotFound");
        if (parentCategoryId == category.Id) throw new BusinessException("ShopManagement:ProductCategoryCannotBeItsOwnParent");
        await ValidateParentAsync(parentCategoryId, tenantId);
        await ValidateNoCircularHierarchyAsync(category.Id, parentCategoryId, tenantId);
        var normalizedName = NormalizeName(name); var normalizedCode = NormalizeCode(code);
        await ValidateUniqueAsync(normalizedName, normalizedCode, parentCategoryId, tenantId, category.Id);
        category.Update(normalizedName, normalizedCode, parentCategoryId, description, displayOrder, isActive);
    }

    public async Task ValidateDeleteAsync(Guid categoryId)
    {
        var tenantId = RequireTenant();
        if (await _repository.AnyAsync(x => x.TenantId == tenantId && x.ParentCategoryId == categoryId))
            throw new BusinessException("ShopManagement:ProductCategoryHasChildren");
    }

    private async Task ValidateParentAsync(Guid? parentId, Guid tenantId)
    {
        if (parentId.HasValue && !await _repository.AnyAsync(x => x.Id == parentId && x.TenantId == tenantId))
            throw new BusinessException("ShopManagement:ProductCategoryNotFound");
    }

    private async Task ValidateUniqueAsync(string name, string code, Guid? parentId, Guid tenantId, Guid? excludedId)
    {
        var query = await _repository.GetQueryableAsync();
        if (await AsyncExecuter.AnyAsync(query.Where(x => x.TenantId == tenantId && x.Code == code && (!excludedId.HasValue || x.Id != excludedId))))
            throw new BusinessException("ShopManagement:ProductCategoryCodeAlreadyExists").WithData("Code", code);
        if (await AsyncExecuter.AnyAsync(query.Where(x => x.TenantId == tenantId && x.ParentCategoryId == parentId && x.Name == name && (!excludedId.HasValue || x.Id != excludedId))))
            throw new BusinessException("ShopManagement:ProductCategoryNameAlreadyExists").WithData("Name", name);
    }

    private async Task ValidateNoCircularHierarchyAsync(Guid categoryId, Guid? parentId, Guid tenantId)
    {
        if (!parentId.HasValue) return;
        var query = await _repository.GetQueryableAsync();
        var byId = (await AsyncExecuter.ToListAsync(query.Where(x => x.TenantId == tenantId))).ToDictionary(x => x.Id);
        var current = parentId;
        while (current.HasValue && byId.TryGetValue(current.Value, out var parent))
        { if (parent.Id == categoryId) throw new BusinessException("ShopManagement:ProductCategoryCircularHierarchy"); current = parent.ParentCategoryId; }
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
    private static string NormalizeName(string value) => Check.NotNullOrWhiteSpace(value, nameof(value), ShopProductCategoryConsts.NameMaxLength).Trim();
    private static string NormalizeCode(string value) => Check.NotNullOrWhiteSpace(value, nameof(value), ShopProductCategoryConsts.CodeMaxLength).Trim().ToUpperInvariant();
}
