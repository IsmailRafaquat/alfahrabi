using System;
using System.Threading.Tasks;
using EHub.ShopManagement.ExpenseCategories;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Modularity;
using Xunit;

namespace EHub.ShopManagement.ExpenseCategories;

public abstract class ShopExpenseCategoryAppServiceTests<TStartupModule> : EHubApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShopExpenseCategoryAppService _categoryAppService;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IRepository<ShopExpenseCategory, Guid> _categoryRepository;
    private readonly ICurrentTenant _currentTenant;

    protected ShopExpenseCategoryAppServiceTests()
    {
        _categoryAppService = GetRequiredService<IShopExpenseCategoryAppService>();
        _tenantManager = GetRequiredService<ITenantManager>();
        _tenantRepository = GetRequiredService<IRepository<Tenant, Guid>>();
        _categoryRepository = GetRequiredService<IRepository<ShopExpenseCategory, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task TenantA_Can_Create_An_Expense_Category()
    {
        var tenantId = await CreateTenantAsync("tenant-create-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var dto = await _categoryAppService.CreateAsync(BuildInput("RENT", "Rent"));
            dto.Id.ShouldNotBe(Guid.Empty);
            dto.Code.ShouldBe("RENT");
            dto.Name.ShouldBe("Rent");
            dto.IsActive.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task Category_Code_Is_Unique_Within_A_Tenant()
    {
        var tenantId = await CreateTenantAsync("tenant-code-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            await _categoryAppService.CreateAsync(BuildInput("UTIL", "Utilities"));
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _categoryAppService.CreateAsync(BuildInput("UTIL", "Utilities Duplicate")));
            exception.Code.ShouldBe("ShopManagement:ExpenseCategoryCodeAlreadyExists");
        }
    }

    [Fact]
    public async Task Same_Code_Can_Exist_In_Different_Tenants()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        using (_currentTenant.Change(tenantAId))
        {
            await _categoryAppService.CreateAsync(BuildInput("SHARED", "Shared A"));
        }

        using (_currentTenant.Change(tenantBId))
        {
            var dto = await _categoryAppService.CreateAsync(BuildInput("SHARED", "Shared B"));
            dto.Code.ShouldBe("SHARED");
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Access_TenantB_Category()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid categoryId;
        using (_currentTenant.Change(tenantBId))
        {
            var created = await _categoryAppService.CreateAsync(BuildInput("TRANSPORT", "Transport"));
            categoryId = created.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() => _categoryAppService.GetAsync(categoryId));
            exception.Code.ShouldBe("ShopManagement:ExpenseCategoryNotFound");
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Update_Or_Delete_TenantB_Category()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid categoryId;
        using (_currentTenant.Change(tenantBId))
        {
            var created = await _categoryAppService.CreateAsync(BuildInput("MAINT", "Maintenance"));
            categoryId = created.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            await Should.ThrowAsync<BusinessException>(() => _categoryAppService.UpdateAsync(categoryId, BuildInput("MAINT", "Hijacked")));
            await Should.ThrowAsync<BusinessException>(() => _categoryAppService.DeleteAsync(categoryId));
        }
    }

    [Fact]
    public async Task Host_Context_Cannot_Create_A_Category()
    {
        using (_currentTenant.Change(null))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() => _categoryAppService.CreateAsync(BuildInput("HOST", "Host")));
            exception.Code.ShouldBe("ShopManagement:TenantRequired");
        }
    }

    [Fact]
    public void TenantId_Is_Not_Accepted_Through_Dto()
    {
        typeof(CreateUpdateShopExpenseCategoryDto).GetProperty("TenantId").ShouldBeNull();
    }

    [Fact]
    public async Task Leading_And_Trailing_Spaces_Are_Trimmed()
    {
        var tenantId = await CreateTenantAsync("tenant-trim-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var dto = await _categoryAppService.CreateAsync(BuildInput("  OFFICE  ", "  Office Supplies  "));
            dto.Code.ShouldBe("OFFICE");
            dto.Name.ShouldBe("Office Supplies");
        }
    }

    [Fact]
    public async Task Empty_Code_And_Name_Are_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-empty-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            await Should.ThrowAsync<Exception>(() => _categoryAppService.CreateAsync(BuildInput("   ", "Some Category")));
            await Should.ThrowAsync<Exception>(() => _categoryAppService.CreateAsync(BuildInput("SOME", "   ")));
        }
    }

    [Fact]
    public async Task Lookup_Returns_Active_Current_Tenant_Categories_Only()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        using (_currentTenant.Change(tenantBId))
        {
            await _categoryAppService.CreateAsync(BuildInput("MISC", "Miscellaneous"));
        }

        using (_currentTenant.Change(tenantAId))
        {
            var active = await _categoryAppService.CreateAsync(BuildInput("SALARY", "Staff Salary"));
            var inactiveInput = BuildInput("INACTIVE", "Inactive Category");
            inactiveInput.IsActive = false;
            await _categoryAppService.CreateAsync(inactiveInput);

            var lookup = await _categoryAppService.GetLookupAsync();
            lookup.Items.ShouldContain(x => x.Id == active.Id);
            lookup.Items.Count.ShouldBe(1);
            lookup.Items.ShouldNotContain(x => x.Name == "Miscellaneous");
        }
    }

    [Fact]
    public async Task Search_And_Active_Filter_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-search-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            await _categoryAppService.CreateAsync(BuildInput("RENT2", "Rent Expense"));
            var inactiveInput = BuildInput("OLD", "Old Category");
            inactiveInput.IsActive = false;
            await _categoryAppService.CreateAsync(inactiveInput);

            (await _categoryAppService.GetListAsync(new GetShopExpenseCategoriesInput { Filter = "RENT2" })).TotalCount.ShouldBe(1);
            (await _categoryAppService.GetListAsync(new GetShopExpenseCategoriesInput { Filter = "Rent Expense" })).TotalCount.ShouldBe(1);
            (await _categoryAppService.GetListAsync(new GetShopExpenseCategoriesInput { IsActive = true })).TotalCount.ShouldBe(1);
            (await _categoryAppService.GetListAsync(new GetShopExpenseCategoriesInput { IsActive = false })).TotalCount.ShouldBe(1);
        }
    }

    [Fact]
    public async Task Delete_Uses_Soft_Deletion()
    {
        var tenantId = await CreateTenantAsync("tenant-delete-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var created = await _categoryAppService.CreateAsync(BuildInput("DEL", "To Delete"));
            await _categoryAppService.DeleteAsync(created.Id);

            await Should.ThrowAsync<BusinessException>(() => _categoryAppService.GetAsync(created.Id));

            await WithUnitOfWorkAsync(async () =>
            {
                using (_currentTenant.Change(tenantId))
                {
                    var deletedEntity = await _categoryRepository.FindAsync(created.Id, includeDetails: false);
                    deletedEntity.ShouldBeNull();
                }
            });
        }
    }

    private static CreateUpdateShopExpenseCategoryDto BuildInput(string code, string name) => new()
    {
        Code = code,
        Name = name,
        IsActive = true,
    };

    private async Task<Guid> CreateTenantAsync(string name)
    {
        return await WithUnitOfWorkAsync(async () =>
        {
            var tenant = await _tenantManager.CreateAsync(name);
            await _tenantRepository.InsertAsync(tenant, autoSave: true);
            return tenant.Id;
        });
    }
}
