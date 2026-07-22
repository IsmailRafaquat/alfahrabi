using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.Suppliers;
using EHub.ShopManagement.Units;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Modularity;
using Xunit;

namespace EHub.ShopManagement.PurchaseOrders;

public abstract class ShopPurchaseOrderAppServiceTests<TStartupModule> : EHubApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShopPurchaseOrderAppService _poAppService;
    private readonly IShopSupplierAppService _supplierAppService;
    private readonly IShopProductCategoryAppService _categoryAppService;
    private readonly IShopUnitAppService _unitAppService;
    private readonly IShopProductAppService _productAppService;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IRepository<ShopPurchaseOrder, Guid> _poRepository;
    private readonly ICurrentTenant _currentTenant;

    protected ShopPurchaseOrderAppServiceTests()
    {
        _poAppService = GetRequiredService<IShopPurchaseOrderAppService>();
        _supplierAppService = GetRequiredService<IShopSupplierAppService>();
        _categoryAppService = GetRequiredService<IShopProductCategoryAppService>();
        _unitAppService = GetRequiredService<IShopUnitAppService>();
        _productAppService = GetRequiredService<IShopProductAppService>();
        _tenantManager = GetRequiredService<ITenantManager>();
        _tenantRepository = GetRequiredService<IRepository<Tenant, Guid>>();
        _poRepository = GetRequiredService<IRepository<ShopPurchaseOrder, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task TenantA_Can_Create_A_Purchase_Order()
    {
        var tenantId = await CreateTenantAsync("tenant-create-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-1");
            var product = await CreateProductAsync(allowDecimal: false);

            var dto = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 10, 100, 0, 0)));

            dto.Id.ShouldNotBe(Guid.Empty);
            dto.PurchaseOrderNumber.ShouldStartWith("PO-");
            dto.Status.ShouldBe(ShopPurchaseOrderStatus.Draft);
            dto.Items.Count.ShouldBe(1);
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Access_TenantB_Purchase_Order()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid poId;
        using (_currentTenant.Change(tenantBId))
        {
            var supplier = await CreateSupplierAsync("SUP-B");
            var product = await CreateProductAsync(allowDecimal: false);
            var created = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 5, 10, 0, 0)));
            poId = created.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            await Should.ThrowAsync<BusinessException>(() => _poAppService.GetAsync(poId));
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Use_TenantB_Supplier()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid otherTenantSupplierId;
        using (_currentTenant.Change(tenantBId))
        {
            var supplier = await CreateSupplierAsync("SUP-B2");
            otherTenantSupplierId = supplier.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var product = await CreateProductAsync(allowDecimal: false);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _poAppService.CreateAsync(BuildInput(otherTenantSupplierId, (product.Id, 5, 10, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderSupplierNotFound");
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Use_TenantB_Product()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid otherTenantProductId;
        using (_currentTenant.Change(tenantBId))
        {
            var product = await CreateProductAsync(allowDecimal: false);
            otherTenantProductId = product.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var supplier = await CreateSupplierAsync("SUP-A2");
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _poAppService.CreateAsync(BuildInput(supplier.Id, (otherTenantProductId, 5, 10, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderProductNotFound");
        }
    }

    [Fact]
    public async Task Inactive_Supplier_Cannot_Be_Used()
    {
        var tenantId = await CreateTenantAsync("tenant-inactive-sup-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-INACTIVE", isActive: false);
            var product = await CreateProductAsync(allowDecimal: false);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 5, 10, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderSupplierInactive");
        }
    }

    [Fact]
    public async Task Inactive_Product_Cannot_Be_Used()
    {
        var tenantId = await CreateTenantAsync("tenant-inactive-prod-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-3");
            var product = await CreateProductAsync(allowDecimal: false, isActive: false);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 5, 10, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderProductInactive");
        }
    }

    [Fact]
    public async Task Purchase_Order_Requires_At_Least_One_Item()
    {
        var tenantId = await CreateTenantAsync("tenant-no-items-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-4");
            var input = new CreateShopPurchaseOrderDto { SupplierId = supplier.Id, OrderDate = DateTime.Today, Items = new List<CreateShopPurchaseOrderItemDto>() };
            await Should.ThrowAsync<Exception>(() => _poAppService.CreateAsync(input));
        }
    }

    [Fact]
    public async Task OrderedQuantity_Must_Be_Greater_Than_Zero()
    {
        var tenantId = await CreateTenantAsync("tenant-zero-qty-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-5");
            var product = await CreateProductAsync(allowDecimal: false);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 0, 10, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderInvalidQuantity");
        }
    }

    [Fact]
    public async Task WholeNumber_Unit_Rejects_Decimal_Quantity()
    {
        var tenantId = await CreateTenantAsync("tenant-whole-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-6");
            var product = await CreateProductAsync(allowDecimal: false);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 2.5m, 10, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderWholeQuantityRequired");
        }
    }

    [Fact]
    public async Task Decimal_Unit_Accepts_Decimal_Quantity()
    {
        var tenantId = await CreateTenantAsync("tenant-decimal-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-7");
            var product = await CreateProductAsync(allowDecimal: true);
            var dto = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 12.75m, 10, 0, 0)));
            dto.Items.Single().OrderedQuantity.ShouldBe(12.75m);
        }
    }

    [Fact]
    public async Task Duplicate_ProductId_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-dup-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-8");
            var product = await CreateProductAsync(allowDecimal: false);
            var input = new CreateShopPurchaseOrderDto
            {
                SupplierId = supplier.Id,
                OrderDate = DateTime.Today,
                Items = new List<CreateShopPurchaseOrderItemDto>
                {
                    new() { ProductId = product.Id, OrderedQuantity = 1, UnitPurchasePrice = 10 },
                    new() { ProductId = product.Id, OrderedQuantity = 2, UnitPurchasePrice = 10 },
                }
            };
            var exception = await Should.ThrowAsync<BusinessException>(() => _poAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderDuplicateProduct");
        }
    }

    [Fact]
    public async Task ExpectedDeliveryDate_Before_OrderDate_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-expdate-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-9");
            var product = await CreateProductAsync(allowDecimal: false);
            var input = BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0));
            input.OrderDate = DateTime.Today;
            input.ExpectedDeliveryDate = DateTime.Today.AddDays(-1);
            var exception = await Should.ThrowAsync<BusinessException>(() => _poAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderInvalidExpectedDeliveryDate");
        }
    }

    [Fact]
    public async Task Negative_ShippingCharges_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-neg-ship-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-10");
            var product = await CreateProductAsync(allowDecimal: false);
            var input = BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0));
            input.ShippingCharges = -1;
            var exception = await Should.ThrowAsync<BusinessException>(() => _poAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderInvalidShippingCharges");
        }
    }

    [Fact]
    public async Task Negative_OtherCharges_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-neg-other-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-11");
            var product = await CreateProductAsync(allowDecimal: false);
            var input = BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0));
            input.OtherCharges = -1;
            var exception = await Should.ThrowAsync<BusinessException>(() => _poAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderInvalidOtherCharges");
        }
    }

    [Fact]
    public async Task DiscountPercentage_Outside_Range_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-disc-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-12");
            var product = await CreateProductAsync(allowDecimal: false);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 150, 0))));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderInvalidDiscountPercentage");
        }
    }

    [Fact]
    public async Task TaxPercentage_Outside_Range_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-tax-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-13");
            var product = await CreateProductAsync(allowDecimal: false);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 150))));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderInvalidTaxPercentage");
        }
    }

    [Fact]
    public async Task Totals_Are_Recalculated_On_The_Server()
    {
        var tenantId = await CreateTenantAsync("tenant-totals-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-14");
            var product1 = await CreateProductAsync(allowDecimal: true);
            var product2 = await CreateProductAsync(allowDecimal: true);
            var product3 = await CreateProductAsync(allowDecimal: true);

            var input = new CreateShopPurchaseOrderDto
            {
                SupplierId = supplier.Id,
                OrderDate = DateTime.Today,
                ShippingCharges = 1500,
                OtherCharges = 500,
                Items = new List<CreateShopPurchaseOrderItemDto>
                {
                    new() { ProductId = product1.Id, OrderedQuantity = 100, UnitPurchasePrice = 180 },
                    new() { ProductId = product2.Id, OrderedQuantity = 200, UnitPurchasePrice = 25 },
                    new() { ProductId = product3.Id, OrderedQuantity = 100, UnitPurchasePrice = 35, DiscountPercentage = 5 },
                }
            };

            var dto = await _poAppService.CreateAsync(input);

            dto.SubTotal.ShouldBe(26500m);
            dto.DiscountAmount.ShouldBe(175m);
            dto.TaxAmount.ShouldBe(0m);
            dto.GrandTotal.ShouldBe(28325m);

            var thirdItem = dto.Items.Single(x => x.ProductId == product3.Id);
            thirdItem.LineSubTotal.ShouldBe(3500m);
            thirdItem.DiscountAmount.ShouldBe(175m);
            thirdItem.LineTotal.ShouldBe(3325m);
        }
    }

    [Fact]
    public void Frontend_Provided_Totals_Are_Ignored()
    {
        typeof(CreateShopPurchaseOrderDto).GetProperty("SubTotal").ShouldBeNull();
        typeof(CreateShopPurchaseOrderDto).GetProperty("GrandTotal").ShouldBeNull();
        typeof(CreateShopPurchaseOrderItemDto).GetProperty("LineTotal").ShouldBeNull();
    }

    [Fact]
    public void TenantId_And_PurchaseOrderNumber_Cannot_Be_Supplied_Through_Dto()
    {
        typeof(CreateShopPurchaseOrderDto).GetProperty("TenantId").ShouldBeNull();
        typeof(CreateShopPurchaseOrderDto).GetProperty("PurchaseOrderNumber").ShouldBeNull();
        typeof(CreateShopPurchaseOrderDto).GetProperty("Status").ShouldBeNull();
        typeof(CreateShopPurchaseOrderItemDto).GetProperty("ReceivedQuantity").ShouldBeNull();
    }

    [Fact]
    public async Task CurrentStock_Is_Unchanged_After_Create_Submit_And_Approve()
    {
        var tenantId = await CreateTenantAsync("tenant-stock-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-15");
            var product = await CreateProductAsync(allowDecimal: false);
            var stockBefore = (await _productAppService.GetAsync(product.Id)).CurrentStock;

            var created = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 10, 100, 0, 0)));
            (await _productAppService.GetAsync(product.Id)).CurrentStock.ShouldBe(stockBefore);

            await _poAppService.SubmitAsync(created.Id);
            (await _productAppService.GetAsync(product.Id)).CurrentStock.ShouldBe(stockBefore);

            await _poAppService.ApproveAsync(created.Id);
            (await _productAppService.GetAsync(product.Id)).CurrentStock.ShouldBe(stockBefore);
        }
    }

    [Fact]
    public async Task Supplier_Balance_Is_Unchanged_After_Purchase_Order_Lifecycle()
    {
        var tenantId = await CreateTenantAsync("tenant-balance-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-16");
            var balanceBefore = (await _supplierAppService.GetAsync(supplier.Id)).OpeningBalance;

            var product = await CreateProductAsync(allowDecimal: false);
            var created = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 10, 100, 0, 0)));
            await _poAppService.SubmitAsync(created.Id);
            await _poAppService.ApproveAsync(created.Id);

            (await _supplierAppService.GetAsync(supplier.Id)).OpeningBalance.ShouldBe(balanceBefore);
        }
    }

    [Fact]
    public async Task PurchaseOrderNumber_Is_Unique_Per_Tenant_And_Server_Generated()
    {
        var tenantId = await CreateTenantAsync("tenant-numbering-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-17");
            var product = await CreateProductAsync(allowDecimal: false);

            var first = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0)));
            var second = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0)));

            first.PurchaseOrderNumber.ShouldNotBe(second.PurchaseOrderNumber);
            first.PurchaseOrderNumber.ShouldBe("PO-000001");
            second.PurchaseOrderNumber.ShouldBe("PO-000002");
        }
    }

    [Fact]
    public async Task Number_Sequence_Is_Isolated_Per_Tenant()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        string firstNumberA;
        using (_currentTenant.Change(tenantAId))
        {
            var supplier = await CreateSupplierAsync("SUP-A3");
            var product = await CreateProductAsync(allowDecimal: false);
            firstNumberA = (await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0)))).PurchaseOrderNumber;
        }

        using (_currentTenant.Change(tenantBId))
        {
            var supplier = await CreateSupplierAsync("SUP-B3");
            var product = await CreateProductAsync(allowDecimal: false);
            var firstNumberB = (await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0)))).PurchaseOrderNumber;
            firstNumberB.ShouldBe(firstNumberA);
        }
    }

    [Fact]
    public async Task Draft_Purchase_Order_Can_Be_Edited()
    {
        var tenantId = await CreateTenantAsync("tenant-edit-draft-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-18");
            var product = await CreateProductAsync(allowDecimal: false);
            var created = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0)));

            var updateInput = BuildUpdateInput(supplier.Id, (product.Id, 5, 20, 0, 0));
            var updated = await _poAppService.UpdateAsync(created.Id, updateInput);
            updated.Items.Single().OrderedQuantity.ShouldBe(5);
        }
    }

    [Fact]
    public async Task PendingApproval_Purchase_Order_Cannot_Be_Edited()
    {
        var tenantId = await CreateTenantAsync("tenant-edit-pending-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-19");
            var product = await CreateProductAsync(allowDecimal: false);
            var created = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0)));
            await _poAppService.SubmitAsync(created.Id);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _poAppService.UpdateAsync(created.Id, BuildUpdateInput(supplier.Id, (product.Id, 5, 20, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderCannotBeEdited");
        }
    }

    [Fact]
    public async Task Approved_Purchase_Order_Cannot_Be_Edited()
    {
        var tenantId = await CreateTenantAsync("tenant-edit-approved-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-20");
            var product = await CreateProductAsync(allowDecimal: false);
            var created = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0)));
            await _poAppService.SubmitAsync(created.Id);
            await _poAppService.ApproveAsync(created.Id);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _poAppService.UpdateAsync(created.Id, BuildUpdateInput(supplier.Id, (product.Id, 5, 20, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderCannotBeEdited");
        }
    }

    [Fact]
    public async Task Draft_Can_Be_Submitted_And_Then_Approved()
    {
        var tenantId = await CreateTenantAsync("tenant-submit-approve-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-21");
            var product = await CreateProductAsync(allowDecimal: false);
            var created = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0)));

            var submitted = await _poAppService.SubmitAsync(created.Id);
            submitted.Status.ShouldBe(ShopPurchaseOrderStatus.PendingApproval);

            var approved = await _poAppService.ApproveAsync(created.Id);
            approved.Status.ShouldBe(ShopPurchaseOrderStatus.Approved);
            approved.ApprovedByUserId.ShouldNotBeNull();
            approved.ApprovedDate.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task PendingApproval_Can_Be_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-reject-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-22");
            var product = await CreateProductAsync(allowDecimal: false);
            var created = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0)));
            await _poAppService.SubmitAsync(created.Id);

            var rejected = await _poAppService.RejectAsync(created.Id, new RejectShopPurchaseOrderDto { RejectionReason = "Price too high" });
            rejected.Status.ShouldBe(ShopPurchaseOrderStatus.Rejected);
            rejected.RejectedByUserId.ShouldNotBeNull();
            rejected.RejectedDate.ShouldNotBeNull();
            rejected.RejectionReason.ShouldBe("Price too high");
        }
    }

    [Fact]
    public async Task Invalid_Status_Transitions_Are_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-invalid-transition-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-23");
            var product = await CreateProductAsync(allowDecimal: false);
            var created = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0)));

            var exception = await Should.ThrowAsync<BusinessException>(() => _poAppService.ApproveAsync(created.Id));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderCannotBeApproved");
        }
    }

    [Fact]
    public async Task Draft_Purchase_Order_Can_Be_Deleted()
    {
        var tenantId = await CreateTenantAsync("tenant-delete-draft-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-24");
            var product = await CreateProductAsync(allowDecimal: false);
            var created = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0)));

            await _poAppService.DeleteAsync(created.Id);
            await Should.ThrowAsync<BusinessException>(() => _poAppService.GetAsync(created.Id));
        }
    }

    [Fact]
    public async Task Submitted_Purchase_Order_Cannot_Be_Deleted()
    {
        var tenantId = await CreateTenantAsync("tenant-delete-submitted-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-25");
            var product = await CreateProductAsync(allowDecimal: false);
            var created = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0)));
            await _poAppService.SubmitAsync(created.Id);

            var exception = await Should.ThrowAsync<BusinessException>(() => _poAppService.DeleteAsync(created.Id));
            exception.Code.ShouldBe("ShopManagement:PurchaseOrderCannotBeDeleted");
        }
    }

    [Fact]
    public async Task Cancellation_Stores_User_Date_And_Reason()
    {
        var tenantId = await CreateTenantAsync("tenant-cancel-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-26");
            var product = await CreateProductAsync(allowDecimal: false);
            var created = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0)));

            var cancelled = await _poAppService.CancelAsync(created.Id, new CancelShopPurchaseOrderDto { CancellationReason = "Supplier unavailable" });
            cancelled.Status.ShouldBe(ShopPurchaseOrderStatus.Cancelled);
            cancelled.CancelledByUserId.ShouldNotBeNull();
            cancelled.CancelledDate.ShouldNotBeNull();
            cancelled.CancellationReason.ShouldBe("Supplier unavailable");
        }
    }

    [Fact]
    public async Task Product_And_Unit_Snapshots_Are_Stored()
    {
        var tenantId = await CreateTenantAsync("tenant-snapshot-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-27");
            var product = await CreateProductAsync(allowDecimal: false, name: "Original Product Name");
            var created = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0)));

            var item = created.Items.Single();
            item.ProductName.ShouldBe("Original Product Name");
            item.ProductCode.ShouldBe(product.Code);
            item.UnitName.ShouldNotBeNullOrEmpty();
            item.UnitShortName.ShouldNotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task Renaming_A_Product_Does_Not_Change_An_Old_PO_Item_Snapshot()
    {
        var tenantId = await CreateTenantAsync("tenant-rename-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-28");
            var product = await CreateProductAsync(allowDecimal: false, name: "Before Rename");
            var created = await _poAppService.CreateAsync(BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0)));

            var fullProduct = await _productAppService.GetAsync(product.Id);
            var updateProductInput = new UpdateShopProductDto
            {
                CategoryId = fullProduct.CategoryId,
                UnitId = fullProduct.UnitId,
                Name = "After Rename",
                Code = fullProduct.Code,
                PurchasePrice = fullProduct.PurchasePrice ?? 0,
                SalePrice = fullProduct.SalePrice,
                MinimumStockLevel = fullProduct.MinimumStockLevel,
                ReorderLevel = fullProduct.ReorderLevel,
                IsActive = fullProduct.IsActive,
            };
            await _productAppService.UpdateAsync(product.Id, updateProductInput);

            var reloaded = await _poAppService.GetAsync(created.Id);
            reloaded.Items.Single().ProductName.ShouldBe("Before Rename");
        }
    }

    [Fact]
    public async Task Host_Context_Cannot_Create_A_Purchase_Order()
    {
        using (_currentTenant.Change(null))
        {
            var input = new CreateShopPurchaseOrderDto
            {
                SupplierId = Guid.NewGuid(),
                OrderDate = DateTime.Today,
                Items = new List<CreateShopPurchaseOrderItemDto> { new() { ProductId = Guid.NewGuid(), OrderedQuantity = 1, UnitPurchasePrice = 10 } }
            };
            var exception = await Should.ThrowAsync<BusinessException>(() => _poAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:TenantRequired");
        }
    }

    [Fact]
    public async Task Search_And_Filters_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-filters-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplierOne = await CreateSupplierAsync("SUP-29");
            var supplierTwo = await CreateSupplierAsync("SUP-30");
            var product = await CreateProductAsync(allowDecimal: false);

            var input1 = BuildInput(supplierOne.Id, (product.Id, 1, 10, 0, 0));
            input1.OrderDate = new DateTime(2026, 1, 10);
            var po1 = await _poAppService.CreateAsync(input1);

            var input2 = BuildInput(supplierTwo.Id, (product.Id, 1, 10, 0, 0));
            input2.OrderDate = new DateTime(2026, 2, 10);
            await _poAppService.CreateAsync(input2);

            (await _poAppService.GetListAsync(new GetShopPurchaseOrdersInput { Filter = po1.PurchaseOrderNumber })).TotalCount.ShouldBe(1);
            (await _poAppService.GetListAsync(new GetShopPurchaseOrdersInput { SupplierId = supplierOne.Id })).TotalCount.ShouldBe(1);
            (await _poAppService.GetListAsync(new GetShopPurchaseOrdersInput { Status = ShopPurchaseOrderStatus.Draft })).TotalCount.ShouldBe(2);
            (await _poAppService.GetListAsync(new GetShopPurchaseOrdersInput { OrderDateFrom = new DateTime(2026, 2, 1) })).TotalCount.ShouldBe(1);
            (await _poAppService.GetListAsync(new GetShopPurchaseOrdersInput { OrderDateTo = new DateTime(2026, 1, 31) })).TotalCount.ShouldBe(1);
        }
    }

    [Fact]
    public async Task Paging_And_Default_Sorting_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-paging-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync("SUP-31");
            var product = await CreateProductAsync(allowDecimal: false);

            var input1 = BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0));
            input1.OrderDate = new DateTime(2026, 1, 1);
            await _poAppService.CreateAsync(input1);

            var input2 = BuildInput(supplier.Id, (product.Id, 1, 10, 0, 0));
            input2.OrderDate = new DateTime(2026, 3, 1);
            await _poAppService.CreateAsync(input2);

            var page1 = await _poAppService.GetListAsync(new GetShopPurchaseOrdersInput { MaxResultCount = 1, SkipCount = 0 });
            page1.Items.Single().OrderDate.ShouldBe(new DateTime(2026, 3, 1));
            page1.TotalCount.ShouldBe(2);
        }
    }

    private async Task<(Guid CategoryId, Guid UnitId)> CreateCategoryAndUnitAsync(bool allowDecimal)
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        var category = await _categoryAppService.CreateAsync(new CreateUpdateShopProductCategoryDto
        { Name = "Category " + suffix, Code = "CAT-" + suffix, DisplayOrder = 0, IsActive = true });
        var unit = await _unitAppService.CreateAsync(new CreateUpdateShopUnitDto
        { Name = "Unit " + suffix, ShortName = suffix.Substring(0, 4), AllowDecimal = allowDecimal, IsActive = true });
        return (category.Id, unit.Id);
    }

    private async Task<ShopProductDto> CreateProductAsync(bool allowDecimal, bool isActive = true, string? name = null)
    {
        var (categoryId, unitId) = await CreateCategoryAndUnitAsync(allowDecimal);
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        var input = new CreateShopProductDto
        {
            CategoryId = categoryId,
            UnitId = unitId,
            Name = name ?? ("Product " + suffix),
            Code = "PRD-" + suffix,
            PurchasePrice = 10,
            SalePrice = 20,
            IsActive = isActive,
        };
        return await _productAppService.CreateAsync(input);
    }

    private async Task<ShopSupplierDto> CreateSupplierAsync(string codeSuffix, bool isActive = true)
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
        return await _supplierAppService.CreateAsync(new CreateUpdateShopSupplierDto
        {
            Code = codeSuffix + "-" + suffix,
            Name = "Supplier " + codeSuffix + "-" + suffix,
            IsActive = isActive,
        });
    }

    private static CreateShopPurchaseOrderDto BuildInput(Guid supplierId, (Guid ProductId, decimal Quantity, decimal Price, decimal Discount, decimal Tax) item) => new()
    {
        SupplierId = supplierId,
        OrderDate = DateTime.Today,
        Items = new List<CreateShopPurchaseOrderItemDto>
        {
            new() { ProductId = item.ProductId, OrderedQuantity = item.Quantity, UnitPurchasePrice = item.Price, DiscountPercentage = item.Discount, TaxPercentage = item.Tax }
        }
    };

    private static UpdateShopPurchaseOrderDto BuildUpdateInput(Guid supplierId, (Guid ProductId, decimal Quantity, decimal Price, decimal Discount, decimal Tax) item) => new()
    {
        SupplierId = supplierId,
        OrderDate = DateTime.Today,
        Items = new List<UpdateShopPurchaseOrderItemDto>
        {
            new() { ProductId = item.ProductId, OrderedQuantity = item.Quantity, UnitPurchasePrice = item.Price, DiscountPercentage = item.Discount, TaxPercentage = item.Tax }
        }
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
