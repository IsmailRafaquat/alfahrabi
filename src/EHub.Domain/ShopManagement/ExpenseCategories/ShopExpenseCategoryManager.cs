using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.ExpenseCategories;

public class ShopExpenseCategoryManager : DomainService
{
    private readonly IRepository<ShopExpenseCategory, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;

    public ShopExpenseCategoryManager(IRepository<ShopExpenseCategory, Guid> repository, ICurrentTenant currentTenant)
    {
        _repository = repository;
        _currentTenant = currentTenant;
    }

    public async Task<ShopExpenseCategory> CreateAsync(string code, string name, string? description, bool isActive)
    {
        var tenantId = RequireTenant();
        var normalizedCode = NormalizeCode(code);
        await ValidateUniqueCodeAsync(normalizedCode, tenantId, null);

        return new ShopExpenseCategory(GuidGenerator.Create(), tenantId, normalizedCode, name, description, isActive);
    }

    public async Task UpdateAsync(ShopExpenseCategory category, string code, string name, string? description, bool isActive)
    {
        var tenantId = RequireTenantOwnership(category);
        var normalizedCode = NormalizeCode(code);
        await ValidateUniqueCodeAsync(normalizedCode, tenantId, category.Id);

        category.Update(normalizedCode, name, description, isActive);
    }

    public Task ValidateDeleteAsync(ShopExpenseCategory category)
    {
        RequireTenantOwnership(category);
        return Task.CompletedTask;
    }

    private async Task ValidateUniqueCodeAsync(string code, Guid tenantId, Guid? excludedId)
    {
        var query = await _repository.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(query.Where(x =>
            x.TenantId == tenantId && x.Code == code && (!excludedId.HasValue || x.Id != excludedId)));
        if (exists) throw new BusinessException("ShopManagement:ExpenseCategoryCodeAlreadyExists").WithData("Code", code);
    }

    private Guid RequireTenantOwnership(ShopExpenseCategory category)
    {
        var tenantId = RequireTenant();
        if (category.TenantId != tenantId) throw new BusinessException("ShopManagement:ExpenseCategoryNotFound");
        return tenantId;
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private static string NormalizeCode(string value) =>
        Check.NotNullOrWhiteSpace(value, nameof(value), ShopExpenseCategoryConsts.CodeMaxLength).Trim();
}
