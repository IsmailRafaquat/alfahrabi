using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.Units;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Modularity;
using Xunit;

namespace EHub.ShopManagement.Products;

public abstract class ShopProductAppServiceTests<TStartupModule> : EHubApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShopProductAppService _productAppService;
    private readonly IShopProductCategoryAppService _categoryAppService;
    private readonly IShopUnitAppService _unitAppService;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly ICurrentTenant _currentTenant;

    protected ShopProductAppServiceTests()
    {
        _productAppService = GetRequiredService<IShopProductAppService>();
        _categoryAppService = GetRequiredService<IShopProductCategoryAppService>();
        _unitAppService = GetRequiredService<IShopUnitAppService>();
        _tenantManager = GetRequiredService<ITenantManager>();
        _tenantRepository = GetRequiredService<IRepository<Tenant, Guid>>();
        _productRepository = GetRequiredService<IRepository<ShopProduct, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task TenantA_Can_Create_A_Product()
    {
        var tenantId = await CreateTenantAsync("tenant-create-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var dto = await _productAppService.CreateAsync(BuildInput(categoryId, unitId, "PVC Pipe 1 Inch", "PVC-PIPE-1IN"));

            dto.Id.ShouldNotBe(Guid.Empty);
            dto.Name.ShouldBe("PVC Pipe 1 Inch");
            dto.Code.ShouldBe("PVC-PIPE-1IN");
            dto.CurrentStock.ShouldBe(0);
        }
    }

    [Fact]
    public async Task TenantA_Can_Update_Its_Product()
    {
        var tenantId = await CreateTenantAsync("tenant-update-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var created = await _productAppService.CreateAsync(BuildInput(categoryId, unitId, "Ball Valve", "BALL-VALVE-05"));

            var input = BuildInput(categoryId, unitId, "Ball Valve 1/2 Inch", "BALL-VALVE-05");
            var updated = await _productAppService.UpdateAsync(created.Id, ToUpdateDto(input));

            updated.Name.ShouldBe("Ball Valve 1/2 Inch");
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Access_TenantB_Product()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid productId;
        using (_currentTenant.Change(tenantBId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var created = await _productAppService.CreateAsync(BuildInput(categoryId, unitId, "Basin Mixer Tap", "BASIN-MIXER-01"));
            productId = created.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            await Should.ThrowAsync<BusinessException>(() => _productAppService.GetAsync(productId));
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Use_TenantB_Category()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid otherTenantCategoryId;
        using (_currentTenant.Change(tenantBId))
        {
            var (categoryId, _) = await CreateCategoryAndUnitAsync();
            otherTenantCategoryId = categoryId;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var (_, unitId) = await CreateCategoryAndUnitAsync();
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _productAppService.CreateAsync(BuildInput(otherTenantCategoryId, unitId, "Thread Seal Tape", "SEAL-TAPE-01")));
            exception.Code.ShouldBe("ShopManagement:ProductCategoryNotFound");
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Use_TenantB_Unit()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid otherTenantUnitId;
        using (_currentTenant.Change(tenantBId))
        {
            var (_, unitId) = await CreateCategoryAndUnitAsync();
            otherTenantUnitId = unitId;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var (categoryId, _) = await CreateCategoryAndUnitAsync();
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _productAppService.CreateAsync(BuildInput(categoryId, otherTenantUnitId, "Thread Seal Tape", "SEAL-TAPE-02")));
            exception.Code.ShouldBe("ShopManagement:ProductUnitNotFound");
        }
    }

    [Fact]
    public async Task Inactive_Category_Cannot_Be_Used_For_New_Products()
    {
        var tenantId = await CreateTenantAsync("tenant-inactive-cat-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var category = await _categoryAppService.CreateAsync(new CreateUpdateShopProductCategoryDto
            { Name = "Pipes", Code = "PIPES", DisplayOrder = 0, IsActive = false });
            var (_, unitId) = await CreateCategoryAndUnitAsync();

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _productAppService.CreateAsync(BuildInput(category.Id, unitId, "PPR Pipe 20mm", "PPR-PIPE-20MM")));
            exception.Code.ShouldBe("ShopManagement:ProductCategoryInactive");
        }
    }

    [Fact]
    public async Task Inactive_Unit_Cannot_Be_Used_For_New_Products()
    {
        var tenantId = await CreateTenantAsync("tenant-inactive-unit-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, _) = await CreateCategoryAndUnitAsync();
            var unit = await _unitAppService.CreateAsync(new CreateUpdateShopUnitDto
            { Name = "Meter", ShortName = "m", AllowDecimal = true, IsActive = false });

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _productAppService.CreateAsync(BuildInput(categoryId, unit.Id, "PPR Pipe 20mm", "PPR-PIPE-20MM-2")));
            exception.Code.ShouldBe("ShopManagement:ProductUnitInactive");
        }
    }

    [Fact]
    public async Task Product_Code_Is_Unique_Within_A_Tenant()
    {
        var tenantId = await CreateTenantAsync("tenant-code-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            await _productAppService.CreateAsync(BuildInput(categoryId, unitId, "Product One", "DUP-CODE"));

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _productAppService.CreateAsync(BuildInput(categoryId, unitId, "Product Two", "DUP-CODE")));
            exception.Code.ShouldBe("ShopManagement:ProductCodeAlreadyExists");
        }
    }

    [Fact]
    public async Task Same_Code_May_Exist_In_Different_Tenants()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        using (_currentTenant.Change(tenantAId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            await _productAppService.CreateAsync(BuildInput(categoryId, unitId, "Product A", "SHARED-CODE"));
        }

        using (_currentTenant.Change(tenantBId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var dto = await _productAppService.CreateAsync(BuildInput(categoryId, unitId, "Product B", "SHARED-CODE"));
            dto.Code.ShouldBe("SHARED-CODE");
        }
    }

    [Fact]
    public async Task Sku_Is_Unique_Within_A_Tenant_When_Provided()
    {
        var tenantId = await CreateTenantAsync("tenant-sku-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var input1 = BuildInput(categoryId, unitId, "Product One", "CODE-1");
            input1.SKU = "SKU-001";
            await _productAppService.CreateAsync(input1);

            var input2 = BuildInput(categoryId, unitId, "Product Two", "CODE-2");
            input2.SKU = "SKU-001";
            var exception = await Should.ThrowAsync<BusinessException>(() => _productAppService.CreateAsync(input2));
            exception.Code.ShouldBe("ShopManagement:ProductSkuAlreadyExists");
        }
    }

    [Fact]
    public async Task Barcode_Is_Unique_Within_A_Tenant_When_Provided()
    {
        var tenantId = await CreateTenantAsync("tenant-barcode-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var input1 = BuildInput(categoryId, unitId, "Product One", "CODE-3");
            input1.Barcode = "1234567890";
            await _productAppService.CreateAsync(input1);

            var input2 = BuildInput(categoryId, unitId, "Product Two", "CODE-4");
            input2.Barcode = "1234567890";
            var exception = await Should.ThrowAsync<BusinessException>(() => _productAppService.CreateAsync(input2));
            exception.Code.ShouldBe("ShopManagement:ProductBarcodeAlreadyExists");
        }
    }

    [Fact]
    public async Task Multiple_Products_May_Have_Null_Sku_And_Barcode()
    {
        var tenantId = await CreateTenantAsync("tenant-null-sku-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var first = await _productAppService.CreateAsync(BuildInput(categoryId, unitId, "Product One", "CODE-5"));
            var second = await _productAppService.CreateAsync(BuildInput(categoryId, unitId, "Product Two", "CODE-6"));

            first.SKU.ShouldBeNull();
            second.SKU.ShouldBeNull();
            first.Barcode.ShouldBeNull();
            second.Barcode.ShouldBeNull();
        }
    }

    [Fact]
    public async Task PurchasePrice_Cannot_Be_Negative()
    {
        var tenantId = await CreateTenantAsync("tenant-neg-purchase-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var input = BuildInput(categoryId, unitId, "Product", "CODE-7");
            input.PurchasePrice = -1;
            await Should.ThrowAsync<BusinessException>(() => _productAppService.CreateAsync(input));
        }
    }

    [Fact]
    public async Task SalePrice_Cannot_Be_Negative()
    {
        var tenantId = await CreateTenantAsync("tenant-neg-sale-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var input = BuildInput(categoryId, unitId, "Product", "CODE-8");
            input.SalePrice = -1;
            await Should.ThrowAsync<BusinessException>(() => _productAppService.CreateAsync(input));
        }
    }

    [Fact]
    public async Task MinimumSalePrice_Cannot_Exceed_SalePrice()
    {
        var tenantId = await CreateTenantAsync("tenant-min-sale-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var input = BuildInput(categoryId, unitId, "Product", "CODE-9");
            input.SalePrice = 100;
            input.MinimumSalePrice = 150;
            var exception = await Should.ThrowAsync<BusinessException>(() => _productAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:MinimumSalePriceCannotExceedSalePrice");
        }
    }

    [Fact]
    public async Task TaxPercentage_Must_Be_Between_Zero_And_100()
    {
        var tenantId = await CreateTenantAsync("tenant-tax-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var input = BuildInput(categoryId, unitId, "Product", "CODE-10");
            input.IsTaxable = true;
            input.TaxPercentage = 150;
            var exception = await Should.ThrowAsync<BusinessException>(() => _productAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:InvalidTaxPercentage");
        }
    }

    [Fact]
    public async Task TaxPercentage_Becomes_Zero_When_IsTaxable_Is_False()
    {
        var tenantId = await CreateTenantAsync("tenant-tax-false-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var input = BuildInput(categoryId, unitId, "Product", "CODE-11");
            input.IsTaxable = false;
            input.TaxPercentage = 17;
            var dto = await _productAppService.CreateAsync(input);
            dto.TaxPercentage.ShouldBe(0);
        }
    }

    [Fact]
    public async Task MaximumStockLevel_Cannot_Be_Lower_Than_MinimumStockLevel()
    {
        var tenantId = await CreateTenantAsync("tenant-stock-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var input = BuildInput(categoryId, unitId, "Product", "CODE-12");
            input.MinimumStockLevel = 10;
            input.MaximumStockLevel = 5;
            var exception = await Should.ThrowAsync<BusinessException>(() => _productAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:MaximumStockCannotBeLowerThanMinimumStock");
        }
    }

    [Fact]
    public async Task CurrentStock_Defaults_To_Zero_And_Is_Not_Editable_Through_UpdateDto()
    {
        var tenantId = await CreateTenantAsync("tenant-current-stock-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var created = await _productAppService.CreateAsync(BuildInput(categoryId, unitId, "Product", "CODE-13"));
            created.CurrentStock.ShouldBe(0);

            typeof(UpdateShopProductDto).GetProperty("CurrentStock").ShouldBeNull();

            var updated = await _productAppService.UpdateAsync(created.Id, ToUpdateDto(BuildInput(categoryId, unitId, "Product Updated", "CODE-13")));
            updated.CurrentStock.ShouldBe(0);
        }
    }

    [Fact]
    public void TenantId_Cannot_Be_Supplied_Through_Create_Or_Update_Dto()
    {
        typeof(CreateShopProductDto).GetProperty("TenantId").ShouldBeNull();
        typeof(UpdateShopProductDto).GetProperty("TenantId").ShouldBeNull();
    }

    [Fact]
    public async Task Host_Context_Cannot_Create_A_Product()
    {
        using (_currentTenant.Change(null))
        {
            var input = BuildInput(Guid.NewGuid(), Guid.NewGuid(), "Product", "HOST-CODE");
            var exception = await Should.ThrowAsync<BusinessException>(() => _productAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:TenantRequired");
        }
    }

    [Fact]
    public async Task Lookup_Returns_Active_Products_Only_For_The_Current_Tenant()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        using (_currentTenant.Change(tenantBId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            await _productAppService.CreateAsync(BuildInput(categoryId, unitId, "Other Tenant Product", "OTHER-CODE"));
        }

        using (_currentTenant.Change(tenantAId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var activeInput = BuildInput(categoryId, unitId, "Active Product", "ACTIVE-CODE");
            var active = await _productAppService.CreateAsync(activeInput);

            var inactiveInput = BuildInput(categoryId, unitId, "Inactive Product", "INACTIVE-CODE");
            inactiveInput.IsActive = false;
            await _productAppService.CreateAsync(inactiveInput);

            var lookup = await _productAppService.GetLookupAsync();
            lookup.Items.ShouldContain(x => x.Id == active.Id);
            lookup.Items.ShouldAllBe(x => x.IsActive);
            lookup.Items.ShouldNotContain(x => x.Name == "Other Tenant Product");
        }
    }

    [Fact]
    public async Task Search_Works_By_Name_Code_Sku_And_Barcode()
    {
        var tenantId = await CreateTenantAsync("tenant-search-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var input = BuildInput(categoryId, unitId, "Basin Mixer Tap", "BASIN-001");
            input.SKU = "SKU-SEARCH";
            input.Barcode = "BARCODE-SEARCH";
            await _productAppService.CreateAsync(input);

            (await _productAppService.GetListAsync(new GetShopProductsInput { Filter = "Basin Mixer" })).TotalCount.ShouldBe(1);
            (await _productAppService.GetListAsync(new GetShopProductsInput { Filter = "BASIN-001" })).TotalCount.ShouldBe(1);
            (await _productAppService.GetListAsync(new GetShopProductsInput { Filter = "SKU-SEARCH" })).TotalCount.ShouldBe(1);
            (await _productAppService.GetListAsync(new GetShopProductsInput { Filter = "BARCODE-SEARCH" })).TotalCount.ShouldBe(1);
            (await _productAppService.GetListAsync(new GetShopProductsInput { Filter = "no-match-xyz" })).TotalCount.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Category_Unit_And_Active_Filters_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-filters-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId1, unitId) = await CreateCategoryAndUnitAsync();
            var category2 = await _categoryAppService.CreateAsync(new CreateUpdateShopProductCategoryDto
            { Name = "Other Category", Code = "OTHER-CAT", DisplayOrder = 0, IsActive = true });

            var p1 = await _productAppService.CreateAsync(BuildInput(categoryId1, unitId, "Product One", "FILT-1"));
            var p2Input = BuildInput(category2.Id, unitId, "Product Two", "FILT-2");
            p2Input.IsActive = false;
            var p2 = await _productAppService.CreateAsync(p2Input);

            (await _productAppService.GetListAsync(new GetShopProductsInput { CategoryId = categoryId1 })).TotalCount.ShouldBe(1);
            (await _productAppService.GetListAsync(new GetShopProductsInput { CategoryId = category2.Id })).TotalCount.ShouldBe(1);
            (await _productAppService.GetListAsync(new GetShopProductsInput { UnitId = unitId })).TotalCount.ShouldBe(2);
            (await _productAppService.GetListAsync(new GetShopProductsInput { IsActive = true })).TotalCount.ShouldBe(1);
            (await _productAppService.GetListAsync(new GetShopProductsInput { IsActive = false })).TotalCount.ShouldBe(1);
        }
    }

    [Fact]
    public async Task LowStockOnly_Filter_Returns_Only_Products_At_Or_Below_Minimum_Stock()
    {
        var tenantId = await CreateTenantAsync("tenant-lowstock-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();

            var lowInput = BuildInput(categoryId, unitId, "Low Stock Product", "LOW-STOCK");
            lowInput.MinimumStockLevel = 5;
            var low = await _productAppService.CreateAsync(lowInput);

            var wellStockedInput = BuildInput(categoryId, unitId, "Well Stocked Product", "WELL-STOCK");
            wellStockedInput.MinimumStockLevel = 5;
            var wellStocked = await _productAppService.CreateAsync(wellStockedInput);

            // Simulate stock movement that will be implemented by a future module.
            await SetCurrentStockDirectlyAsync(wellStocked.Id, 20);

            var result = await _productAppService.GetListAsync(new GetShopProductsInput { LowStockOnly = true });
            result.Items.ShouldContain(x => x.Id == low.Id);
            result.Items.ShouldNotContain(x => x.Id == wellStocked.Id);
        }
    }

    [Fact]
    public async Task Delete_Uses_Soft_Deletion()
    {
        var tenantId = await CreateTenantAsync("tenant-delete-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
            var created = await _productAppService.CreateAsync(BuildInput(categoryId, unitId, "Product To Delete", "DEL-CODE"));
            await _productAppService.DeleteAsync(created.Id);

            await Should.ThrowAsync<BusinessException>(() => _productAppService.GetAsync(created.Id));

            await WithUnitOfWorkAsync(async () =>
            {
                using (_currentTenant.Change(tenantId))
                {
                    var deletedEntity = await _productRepository.FindAsync(created.Id, includeDetails: false);
                    deletedEntity.ShouldBeNull();
                }
            });
        }
    }

    private async Task<(Guid CategoryId, Guid UnitId)> CreateCategoryAndUnitAsync()
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        var category = await _categoryAppService.CreateAsync(new CreateUpdateShopProductCategoryDto
        { Name = "Category " + suffix, Code = "CAT-" + suffix, DisplayOrder = 0, IsActive = true });
        var unit = await _unitAppService.CreateAsync(new CreateUpdateShopUnitDto
        { Name = "Unit " + suffix, ShortName = suffix.Substring(0, 4), AllowDecimal = false, IsActive = true });
        return (category.Id, unit.Id);
    }

    private static CreateShopProductDto BuildInput(Guid categoryId, Guid unitId, string name, string code) => new()
    {
        CategoryId = categoryId,
        UnitId = unitId,
        Name = name,
        Code = code,
        PurchasePrice = 10,
        SalePrice = 20,
        TaxPercentage = 0,
        MinimumStockLevel = 0,
        ReorderLevel = 0,
        IsActive = true
    };

    private static UpdateShopProductDto ToUpdateDto(CreateShopProductDto input) => new()
    {
        CategoryId = input.CategoryId,
        UnitId = input.UnitId,
        Name = input.Name,
        Code = input.Code,
        SKU = input.SKU,
        Barcode = input.Barcode,
        Description = input.Description,
        Brand = input.Brand,
        Model = input.Model,
        PurchasePrice = input.PurchasePrice,
        SalePrice = input.SalePrice,
        WholesalePrice = input.WholesalePrice,
        MinimumSalePrice = input.MinimumSalePrice,
        TaxPercentage = input.TaxPercentage,
        MinimumStockLevel = input.MinimumStockLevel,
        MaximumStockLevel = input.MaximumStockLevel,
        ReorderLevel = input.ReorderLevel,
        TrackBatch = input.TrackBatch,
        TrackExpiry = input.TrackExpiry,
        TrackSerialNumber = input.TrackSerialNumber,
        IsTaxable = input.IsTaxable,
        IsActive = input.IsActive
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

    private async Task SetCurrentStockDirectlyAsync(Guid productId, decimal stock)
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var entity = await _productRepository.GetAsync(productId);
            var property = typeof(ShopProduct).GetProperty(nameof(ShopProduct.CurrentStock), BindingFlags.Public | BindingFlags.Instance)!;
            property.SetValue(entity, stock);
            await _productRepository.UpdateAsync(entity, autoSave: true);
        });
    }
}
