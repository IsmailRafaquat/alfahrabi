using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.Customers;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Modularity;
using Xunit;

namespace EHub.ShopManagement.Customers;

public abstract class ShopCustomerAppServiceTests<TStartupModule> : EHubApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShopCustomerAppService _customerAppService;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IRepository<ShopCustomer, Guid> _customerRepository;
    private readonly ICurrentTenant _currentTenant;

    protected ShopCustomerAppServiceTests()
    {
        _customerAppService = GetRequiredService<IShopCustomerAppService>();
        _tenantManager = GetRequiredService<ITenantManager>();
        _tenantRepository = GetRequiredService<IRepository<Tenant, Guid>>();
        _customerRepository = GetRequiredService<IRepository<ShopCustomer, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task TenantA_Can_Create_A_Customer()
    {
        var tenantId = await CreateTenantAsync("tenant-create-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var dto = await _customerAppService.CreateAsync(BuildInput("CUS-001", "Ahmed Traders"));
            dto.Id.ShouldNotBe(Guid.Empty);
            dto.Code.ShouldBe("CUS-001");
            dto.Name.ShouldBe("Ahmed Traders");
            dto.CustomerType.ShouldBe(ShopCustomerType.Business);
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Access_TenantB_Customer()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid customerId;
        using (_currentTenant.Change(tenantBId))
        {
            var created = await _customerAppService.CreateAsync(BuildInput("CUS-B1", "Tenant B Customer"));
            customerId = created.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            await Should.ThrowAsync<BusinessException>(() => _customerAppService.GetAsync(customerId));
        }
    }

    [Fact]
    public async Task Customer_Code_Is_Unique_Within_A_Tenant()
    {
        var tenantId = await CreateTenantAsync("tenant-code-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            await _customerAppService.CreateAsync(BuildInput("DUP-CODE", "Customer One"));
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _customerAppService.CreateAsync(BuildInput("DUP-CODE", "Customer Two")));
            exception.Code.ShouldBe("ShopManagement:CustomerCodeAlreadyExists");
        }
    }

    [Fact]
    public async Task Same_Code_Can_Exist_In_Different_Tenants()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        using (_currentTenant.Change(tenantAId))
        {
            await _customerAppService.CreateAsync(BuildInput("SHARED-CODE", "Customer A"));
        }

        using (_currentTenant.Change(tenantBId))
        {
            var dto = await _customerAppService.CreateAsync(BuildInput("SHARED-CODE", "Customer B"));
            dto.Code.ShouldBe("SHARED-CODE");
        }
    }

    [Fact]
    public async Task Empty_Code_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-empty-code-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var input = BuildInput("   ", "Some Customer");
            await Should.ThrowAsync<Exception>(() => _customerAppService.CreateAsync(input));
        }
    }

    [Fact]
    public async Task Empty_Name_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-empty-name-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var input = BuildInput("CUS-EMPTY", "   ");
            await Should.ThrowAsync<Exception>(() => _customerAppService.CreateAsync(input));
        }
    }

    [Fact]
    public async Task Invalid_Email_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-email-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var input = BuildInput("CUS-EMAIL", "Email Customer");
            input.Email = "not-an-email";
            await Should.ThrowAsync<Exception>(() => _customerAppService.CreateAsync(input));
        }
    }

    [Fact]
    public async Task Negative_OpeningBalance_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-neg-balance-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var input = BuildInput("CUS-NEGB", "Negative Balance Customer");
            input.OpeningBalance = -1;
            var exception = await Should.ThrowAsync<BusinessException>(() => _customerAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:CustomerOpeningBalanceCannotBeNegative");
        }
    }

    [Fact]
    public async Task Negative_CreditLimit_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-neg-credit-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var input = BuildInput("CUS-NEGC", "Negative Credit Customer");
            input.CreditLimit = -1;
            var exception = await Should.ThrowAsync<BusinessException>(() => _customerAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:CustomerCreditLimitCannotBeNegative");
        }
    }

    [Fact]
    public async Task Negative_PaymentTermsDays_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-neg-terms-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var input = BuildInput("CUS-NEGT", "Negative Terms Customer");
            input.PaymentTermsDays = -1;
            var exception = await Should.ThrowAsync<BusinessException>(() => _customerAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentTermsCannotBeNegative");
        }
    }

    [Fact]
    public async Task Only_One_WalkIn_Customer_Is_Allowed_Per_Tenant()
    {
        var tenantId = await CreateTenantAsync("tenant-walkin-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var walkIn = BuildInput("WALK-IN", "Walk-in Customer");
            walkIn.CustomerType = ShopCustomerType.WalkIn;
            walkIn.IsWalkInCustomer = true;
            await _customerAppService.CreateAsync(walkIn);

            var secondWalkIn = BuildInput("WALK-IN-2", "Another Walk-in");
            secondWalkIn.CustomerType = ShopCustomerType.WalkIn;
            secondWalkIn.IsWalkInCustomer = true;
            var exception = await Should.ThrowAsync<BusinessException>(() => _customerAppService.CreateAsync(secondWalkIn));
            exception.Code.ShouldBe("ShopManagement:WalkInCustomerAlreadyExists");
        }
    }

    [Fact]
    public async Task Different_Tenants_May_Each_Have_One_WalkIn_Customer()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-walkin-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-walkin-" + Guid.NewGuid().ToString("N"));

        using (_currentTenant.Change(tenantAId))
        {
            var walkIn = BuildInput("WALK-IN", "Walk-in Customer");
            walkIn.CustomerType = ShopCustomerType.WalkIn;
            walkIn.IsWalkInCustomer = true;
            await _customerAppService.CreateAsync(walkIn);
        }

        using (_currentTenant.Change(tenantBId))
        {
            var walkIn = BuildInput("WALK-IN", "Walk-in Customer");
            walkIn.CustomerType = ShopCustomerType.WalkIn;
            walkIn.IsWalkInCustomer = true;
            var dto = await _customerAppService.CreateAsync(walkIn);
            dto.IsWalkInCustomer.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task GetWalkInCustomer_Returns_The_Correct_Tenant_Customer()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-getwalkin-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-getwalkin-" + Guid.NewGuid().ToString("N"));

        using (_currentTenant.Change(tenantBId))
        {
            var walkIn = BuildInput("WALK-IN", "Tenant B Walk-in");
            walkIn.CustomerType = ShopCustomerType.WalkIn;
            walkIn.IsWalkInCustomer = true;
            await _customerAppService.CreateAsync(walkIn);
        }

        using (_currentTenant.Change(tenantAId))
        {
            (await _customerAppService.GetWalkInCustomerAsync()).ShouldBeNull();

            var walkIn = BuildInput("WALK-IN", "Tenant A Walk-in");
            walkIn.CustomerType = ShopCustomerType.WalkIn;
            walkIn.IsWalkInCustomer = true;
            var created = await _customerAppService.CreateAsync(walkIn);

            var found = await _customerAppService.GetWalkInCustomerAsync();
            found.ShouldNotBeNull();
            found!.Id.ShouldBe(created.Id);
            found.Name.ShouldBe("Tenant A Walk-in");
        }
    }

    [Fact]
    public async Task Host_Context_Cannot_Create_A_Customer()
    {
        using (_currentTenant.Change(null))
        {
            var input = BuildInput("HOST-CODE", "Host Customer");
            var exception = await Should.ThrowAsync<BusinessException>(() => _customerAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:TenantRequired");
        }
    }

    [Fact]
    public void TenantId_Cannot_Be_Supplied_Through_Dto()
    {
        typeof(CreateUpdateShopCustomerDto).GetProperty("TenantId").ShouldBeNull();
    }

    [Fact]
    public async Task Lookup_Returns_Active_Customers_Only_For_The_Current_Tenant()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        using (_currentTenant.Change(tenantBId))
        {
            await _customerAppService.CreateAsync(BuildInput("CUS-OTHERB", "Other Tenant Customer"));
        }

        using (_currentTenant.Change(tenantAId))
        {
            var active = await _customerAppService.CreateAsync(BuildInput("CUS-ACTIVE", "Active Customer"));
            var inactiveInput = BuildInput("CUS-INACTIVE", "Inactive Customer");
            inactiveInput.IsActive = false;
            await _customerAppService.CreateAsync(inactiveInput);

            var lookup = await _customerAppService.GetLookupAsync();
            lookup.Items.ShouldContain(x => x.Id == active.Id);
            lookup.Items.Count.ShouldBe(1);
            lookup.Items.ShouldNotContain(x => x.Name == "Other Tenant Customer");
        }
    }

    [Fact]
    public async Task Search_Works_By_Code_Name_ContactPerson_And_Phone()
    {
        var tenantId = await CreateTenantAsync("tenant-search-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var input = BuildInput("SEARCH-001", "Al-Noor Traders");
            input.ContactPerson = "Bilal Ahmed";
            input.Phone = "051-3334455";
            await _customerAppService.CreateAsync(input);

            (await _customerAppService.GetListAsync(new GetShopCustomersInput { Filter = "SEARCH-001" })).TotalCount.ShouldBe(1);
            (await _customerAppService.GetListAsync(new GetShopCustomersInput { Filter = "Al-Noor" })).TotalCount.ShouldBe(1);
            (await _customerAppService.GetListAsync(new GetShopCustomersInput { Filter = "Bilal Ahmed" })).TotalCount.ShouldBe(1);
            (await _customerAppService.GetListAsync(new GetShopCustomersInput { Filter = "051-3334455" })).TotalCount.ShouldBe(1);
            (await _customerAppService.GetListAsync(new GetShopCustomersInput { Filter = "no-match-xyz" })).TotalCount.ShouldBe(0);
        }
    }

    [Fact]
    public async Task CustomerType_City_Country_Active_And_Balance_Filters_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-filters-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var lahoreInput = BuildInput("CUS-LHR", "Lahore Customer");
            lahoreInput.CustomerType = ShopCustomerType.Individual;
            lahoreInput.City = "Lahore";
            lahoreInput.Country = "Pakistan";
            lahoreInput.OpeningBalance = 5000;
            lahoreInput.CreditLimit = 0;
            await _customerAppService.CreateAsync(lahoreInput);

            var islamabadInput = BuildInput("CUS-ISB", "Islamabad Customer");
            islamabadInput.CustomerType = ShopCustomerType.Business;
            islamabadInput.City = "Islamabad";
            islamabadInput.Country = "Pakistan";
            islamabadInput.OpeningBalance = 0;
            islamabadInput.CreditLimit = 10000;
            islamabadInput.IsActive = false;
            await _customerAppService.CreateAsync(islamabadInput);

            (await _customerAppService.GetListAsync(new GetShopCustomersInput { City = "Lahore" })).TotalCount.ShouldBe(1);
            (await _customerAppService.GetListAsync(new GetShopCustomersInput { Country = "Pakistan" })).TotalCount.ShouldBe(2);
            (await _customerAppService.GetListAsync(new GetShopCustomersInput { CustomerType = ShopCustomerType.Individual })).TotalCount.ShouldBe(1);
            (await _customerAppService.GetListAsync(new GetShopCustomersInput { CustomerType = ShopCustomerType.Business })).TotalCount.ShouldBe(1);
            (await _customerAppService.GetListAsync(new GetShopCustomersInput { IsActive = true })).TotalCount.ShouldBe(1);
            (await _customerAppService.GetListAsync(new GetShopCustomersInput { IsActive = false })).TotalCount.ShouldBe(1);
            (await _customerAppService.GetListAsync(new GetShopCustomersInput { HasOpeningBalance = true })).TotalCount.ShouldBe(1);
            (await _customerAppService.GetListAsync(new GetShopCustomersInput { HasCreditLimit = true })).TotalCount.ShouldBe(1);
        }
    }

    [Fact]
    public async Task Paging_And_Sorting_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-paging-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            await _customerAppService.CreateAsync(BuildInput("CUS-P1", "Bravo Customer"));
            await _customerAppService.CreateAsync(BuildInput("CUS-P2", "Alpha Customer"));

            var page = await _customerAppService.GetListAsync(new GetShopCustomersInput { MaxResultCount = 1, SkipCount = 0 });
            page.TotalCount.ShouldBe(2);
            page.Items.Single().Name.ShouldBe("Alpha Customer");
        }
    }

    [Fact]
    public async Task Delete_Uses_Soft_Deletion()
    {
        var tenantId = await CreateTenantAsync("tenant-delete-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var created = await _customerAppService.CreateAsync(BuildInput("CUS-DEL", "Customer To Delete"));
            await _customerAppService.DeleteAsync(created.Id);

            await Should.ThrowAsync<BusinessException>(() => _customerAppService.GetAsync(created.Id));

            await WithUnitOfWorkAsync(async () =>
            {
                using (_currentTenant.Change(tenantId))
                {
                    var deletedEntity = await _customerRepository.FindAsync(created.Id, includeDetails: false);
                    deletedEntity.ShouldBeNull();
                }
            });
        }
    }

    private static CreateUpdateShopCustomerDto BuildInput(string code, string name) => new()
    {
        Code = code,
        Name = name,
        CustomerType = ShopCustomerType.Business,
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
