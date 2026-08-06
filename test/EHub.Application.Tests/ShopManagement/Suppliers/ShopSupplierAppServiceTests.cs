using System;
using System.Threading.Tasks;
using EHub.ShopManagement.Suppliers;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Modularity;
using Xunit;

namespace EHub.ShopManagement.Suppliers;

public abstract class ShopSupplierAppServiceTests<TStartupModule> : EHubApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShopSupplierAppService _supplierAppService;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IRepository<ShopSupplier, Guid> _supplierRepository;
    private readonly ICurrentTenant _currentTenant;

    protected ShopSupplierAppServiceTests()
    {
        _supplierAppService = GetRequiredService<IShopSupplierAppService>();
        _tenantManager = GetRequiredService<ITenantManager>();
        _tenantRepository = GetRequiredService<IRepository<Tenant, Guid>>();
        _supplierRepository = GetRequiredService<IRepository<ShopSupplier, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task TenantA_Can_Create_A_Supplier()
    {
        var tenantId = await CreateTenantAsync("tenant-create-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var dto = await _supplierAppService.CreateAsync(BuildInput("SUP-001", "Popular Pipes Industries"));
            dto.Id.ShouldNotBe(Guid.Empty);
            dto.Code.ShouldBe("SUP-001");
            dto.Name.ShouldBe("Popular Pipes Industries");
        }
    }

    [Fact]
    public async Task TenantA_Can_Update_Its_Supplier()
    {
        var tenantId = await CreateTenantAsync("tenant-update-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var created = await _supplierAppService.CreateAsync(BuildInput("SUP-002", "Master Sanitary Store"));
            var input = BuildInput("SUP-002", "Master Sanitary Store Updated");
            var updated = await _supplierAppService.UpdateAsync(created.Id, input);
            updated.Name.ShouldBe("Master Sanitary Store Updated");
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Access_TenantB_Supplier()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid supplierId;
        using (_currentTenant.Change(tenantBId))
        {
            var created = await _supplierAppService.CreateAsync(BuildInput("SUP-B1", "Tenant B Supplier"));
            supplierId = created.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            await Should.ThrowAsync<BusinessException>(() => _supplierAppService.GetAsync(supplierId));
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Update_TenantB_Supplier()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid supplierId;
        using (_currentTenant.Change(tenantBId))
        {
            var created = await _supplierAppService.CreateAsync(BuildInput("SUP-B2", "Tenant B Supplier Two"));
            supplierId = created.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            await Should.ThrowAsync<BusinessException>(() => _supplierAppService.UpdateAsync(supplierId, BuildInput("SUP-B2", "Hijacked Name")));
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Delete_TenantB_Supplier()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid supplierId;
        using (_currentTenant.Change(tenantBId))
        {
            var created = await _supplierAppService.CreateAsync(BuildInput("SUP-B3", "Tenant B Supplier Three"));
            supplierId = created.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            await Should.ThrowAsync<BusinessException>(() => _supplierAppService.DeleteAsync(supplierId));
        }
    }

    [Fact]
    public async Task Supplier_Code_Is_Unique_Within_A_Tenant()
    {
        var tenantId = await CreateTenantAsync("tenant-code-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            await _supplierAppService.CreateAsync(BuildInput("DUP-CODE", "Supplier One"));
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _supplierAppService.CreateAsync(BuildInput("DUP-CODE", "Supplier Two")));
            exception.Code.ShouldBe("ShopManagement:SupplierCodeAlreadyExists");
        }
    }

    [Fact]
    public async Task Same_Code_Can_Exist_In_Different_Tenants()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        using (_currentTenant.Change(tenantAId))
        {
            await _supplierAppService.CreateAsync(BuildInput("SHARED-CODE", "Supplier A"));
        }

        using (_currentTenant.Change(tenantBId))
        {
            var dto = await _supplierAppService.CreateAsync(BuildInput("SHARED-CODE", "Supplier B"));
            dto.Code.ShouldBe("SHARED-CODE");
        }
    }

    [Fact]
    public async Task Supplier_Name_Uniqueness_Is_Enforced_Within_A_Tenant()
    {
        var tenantId = await CreateTenantAsync("tenant-name-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            await _supplierAppService.CreateAsync(BuildInput("SUP-N1", "Duplicate Name Co"));
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _supplierAppService.CreateAsync(BuildInput("SUP-N2", "Duplicate Name Co")));
            exception.Code.ShouldBe("ShopManagement:SupplierNameAlreadyExists");
        }
    }

    [Fact]
    public async Task Leading_And_Trailing_Spaces_Are_Removed()
    {
        var tenantId = await CreateTenantAsync("tenant-trim-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var input = BuildInput("  SUP-TRIM  ", "  Trimmed Supplier  ");
            var dto = await _supplierAppService.CreateAsync(input);
            dto.Code.ShouldBe("SUP-TRIM");
            dto.Name.ShouldBe("Trimmed Supplier");
        }
    }

    [Fact]
    public async Task Empty_Code_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-empty-code-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var input = BuildInput("   ", "Some Supplier");
            await Should.ThrowAsync<Exception>(() => _supplierAppService.CreateAsync(input));
        }
    }

    [Fact]
    public async Task Empty_Name_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-empty-name-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var input = BuildInput("SUP-EMPTY", "   ");
            await Should.ThrowAsync<Exception>(() => _supplierAppService.CreateAsync(input));
        }
    }

    [Fact]
    public async Task Invalid_Email_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-email-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var input = BuildInput("SUP-EMAIL", "Email Supplier");
            input.Email = "not-an-email";
            await Should.ThrowAsync<Exception>(() => _supplierAppService.CreateAsync(input));
        }
    }

    [Fact]
    public async Task Negative_OpeningBalance_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-neg-balance-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var input = BuildInput("SUP-NEGB", "Negative Balance Supplier");
            input.OpeningBalance = -1;
            var exception = await Should.ThrowAsync<BusinessException>(() => _supplierAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:SupplierOpeningBalanceCannotBeNegative");
        }
    }

    [Fact]
    public async Task Negative_CreditLimit_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-neg-credit-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var input = BuildInput("SUP-NEGC", "Negative Credit Supplier");
            input.CreditLimit = -1;
            var exception = await Should.ThrowAsync<BusinessException>(() => _supplierAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:SupplierCreditLimitCannotBeNegative");
        }
    }

    [Fact]
    public async Task Negative_PaymentTermsDays_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-neg-terms-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var input = BuildInput("SUP-NEGT", "Negative Terms Supplier");
            input.PaymentTermsDays = -1;
            var exception = await Should.ThrowAsync<BusinessException>(() => _supplierAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:SupplierPaymentTermsCannotBeNegative");
        }
    }

    [Fact]
    public async Task Host_Context_Cannot_Create_A_Supplier()
    {
        using (_currentTenant.Change(null))
        {
            var input = BuildInput("HOST-CODE", "Host Supplier");
            var exception = await Should.ThrowAsync<BusinessException>(() => _supplierAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:TenantRequired");
        }
    }

    [Fact]
    public void TenantId_Cannot_Be_Supplied_Through_Dto()
    {
        typeof(CreateUpdateShopSupplierDto).GetProperty("TenantId").ShouldBeNull();
    }

    [Fact]
    public async Task Lookup_Returns_Active_Suppliers_Only_For_The_Current_Tenant()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        using (_currentTenant.Change(tenantBId))
        {
            await _supplierAppService.CreateAsync(BuildInput("SUP-OTHERB", "Other Tenant Supplier"));
        }

        using (_currentTenant.Change(tenantAId))
        {
            var active = await _supplierAppService.CreateAsync(BuildInput("SUP-ACTIVE", "Active Supplier"));
            var inactiveInput = BuildInput("SUP-INACTIVE", "Inactive Supplier");
            inactiveInput.IsActive = false;
            await _supplierAppService.CreateAsync(inactiveInput);

            var lookup = await _supplierAppService.GetLookupAsync();
            lookup.Items.ShouldContain(x => x.Id == active.Id);
            lookup.Items.Count.ShouldBe(1);
            lookup.Items.ShouldNotContain(x => x.Name == "Other Tenant Supplier");
        }
    }

    [Fact]
    public async Task Search_Works_By_Code_Name_ContactPerson_And_Phone()
    {
        var tenantId = await CreateTenantAsync("tenant-search-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var input = BuildInput("SEARCH-001", "Al-Noor Hardware Traders");
            input.ContactPerson = "Bilal Ahmed";
            input.Phone = "051-3334455";
            await _supplierAppService.CreateAsync(input);

            (await _supplierAppService.GetListAsync(new GetShopSuppliersInput { Filter = "SEARCH-001" })).TotalCount.ShouldBe(1);
            (await _supplierAppService.GetListAsync(new GetShopSuppliersInput { Filter = "Al-Noor" })).TotalCount.ShouldBe(1);
            (await _supplierAppService.GetListAsync(new GetShopSuppliersInput { Filter = "Bilal Ahmed" })).TotalCount.ShouldBe(1);
            (await _supplierAppService.GetListAsync(new GetShopSuppliersInput { Filter = "051-3334455" })).TotalCount.ShouldBe(1);
            (await _supplierAppService.GetListAsync(new GetShopSuppliersInput { Filter = "no-match-xyz" })).TotalCount.ShouldBe(0);
        }
    }

    [Fact]
    public async Task City_Country_Active_And_Balance_Filters_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-filters-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var lahoreInput = BuildInput("SUP-LHR", "Lahore Supplier");
            lahoreInput.City = "Lahore";
            lahoreInput.Country = "Pakistan";
            lahoreInput.OpeningBalance = 5000;
            lahoreInput.CreditLimit = 0;
            await _supplierAppService.CreateAsync(lahoreInput);

            var islamabadInput = BuildInput("SUP-ISB", "Islamabad Supplier");
            islamabadInput.City = "Islamabad";
            islamabadInput.Country = "Pakistan";
            islamabadInput.OpeningBalance = 0;
            islamabadInput.CreditLimit = 10000;
            islamabadInput.IsActive = false;
            await _supplierAppService.CreateAsync(islamabadInput);

            (await _supplierAppService.GetListAsync(new GetShopSuppliersInput { City = "Lahore" })).TotalCount.ShouldBe(1);
            (await _supplierAppService.GetListAsync(new GetShopSuppliersInput { Country = "Pakistan" })).TotalCount.ShouldBe(2);
            (await _supplierAppService.GetListAsync(new GetShopSuppliersInput { IsActive = true })).TotalCount.ShouldBe(1);
            (await _supplierAppService.GetListAsync(new GetShopSuppliersInput { IsActive = false })).TotalCount.ShouldBe(1);
            (await _supplierAppService.GetListAsync(new GetShopSuppliersInput { HasOpeningBalance = true })).TotalCount.ShouldBe(1);
            (await _supplierAppService.GetListAsync(new GetShopSuppliersInput { HasCreditLimit = true })).TotalCount.ShouldBe(1);
        }
    }

    [Fact]
    public async Task Delete_Uses_Soft_Deletion()
    {
        var tenantId = await CreateTenantAsync("tenant-delete-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var created = await _supplierAppService.CreateAsync(BuildInput("SUP-DEL", "Supplier To Delete"));
            await _supplierAppService.DeleteAsync(created.Id);

            await Should.ThrowAsync<BusinessException>(() => _supplierAppService.GetAsync(created.Id));

            await WithUnitOfWorkAsync(async () =>
            {
                using (_currentTenant.Change(tenantId))
                {
                    var deletedEntity = await _supplierRepository.FindAsync(created.Id, includeDetails: false);
                    deletedEntity.ShouldBeNull();
                }
            });
        }
    }

    private static CreateUpdateShopSupplierDto BuildInput(string code, string name) => new()
    {
        Code = code,
        Name = name,
        OpeningBalance = 0,
        CreditLimit = 0,
        PaymentTermsDays = 0,
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
