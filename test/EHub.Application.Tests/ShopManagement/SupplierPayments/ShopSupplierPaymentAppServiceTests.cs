using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.Suppliers;
using EHub.ShopManagement.Units;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Modularity;
using Xunit;

namespace EHub.ShopManagement.SupplierPayments;

public abstract class ShopSupplierPaymentAppServiceTests<TStartupModule> : EHubApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShopSupplierPaymentAppService _paymentAppService;
    private readonly IShopGoodsReceiptAppService _grAppService;
    private readonly IShopPurchaseOrderAppService _poAppService;
    private readonly IShopSupplierAppService _supplierAppService;
    private readonly IShopProductCategoryAppService _categoryAppService;
    private readonly IShopUnitAppService _unitAppService;
    private readonly IShopProductAppService _productAppService;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly ICurrentTenant _currentTenant;

    protected ShopSupplierPaymentAppServiceTests()
    {
        _paymentAppService = GetRequiredService<IShopSupplierPaymentAppService>();
        _grAppService = GetRequiredService<IShopGoodsReceiptAppService>();
        _poAppService = GetRequiredService<IShopPurchaseOrderAppService>();
        _supplierAppService = GetRequiredService<IShopSupplierAppService>();
        _categoryAppService = GetRequiredService<IShopProductCategoryAppService>();
        _unitAppService = GetRequiredService<IShopUnitAppService>();
        _productAppService = GetRequiredService<IShopProductAppService>();
        _tenantManager = GetRequiredService<ITenantManager>();
        _tenantRepository = GetRequiredService<IRepository<Tenant, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task TenantA_Can_Create_A_Draft_Payment_With_Allocation()
    {
        var tenantId = await CreateTenantAsync("tenant-create-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync();
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 700, allocations: new[] { (gr.Id, 700m) }));

            payment.Id.ShouldNotBe(Guid.Empty);
            payment.PaymentNumber.ShouldStartWith("SP-");
            payment.Status.ShouldBe(ShopSupplierPaymentStatus.Draft);
            payment.Allocations.Single().AllocatedAmount.ShouldBe(700);
            payment.AllocatedAmount.ShouldBe(700);
            payment.UnallocatedAmount.ShouldBe(0);
        }
    }

    [Fact]
    public async Task PaymentNumber_Is_Server_Generated_And_Sequential_Per_Tenant()
    {
        var tenantId = await CreateTenantAsync("tenant-numbering-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(quantity: 10, price: 100);
            var first = await _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 500, allocations: new[] { (gr.Id, 500m) }));
            var second = await _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 300, ShopSupplierPaymentType.Advance));

            first.PaymentNumber.ShouldBe("SP-000001");
            second.PaymentNumber.ShouldBe("SP-000002");
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Access_TenantB_Payment()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid paymentId;
        using (_currentTenant.Change(tenantBId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync();
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 500, allocations: new[] { (gr.Id, 500m) }));
            paymentId = payment.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            await Should.ThrowAsync<BusinessException>(() => _paymentAppService.GetAsync(paymentId));
        }
    }

    [Fact]
    public async Task Host_Context_Cannot_Create_A_Payment()
    {
        using (_currentTenant.Change(null))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(Guid.NewGuid(), 100, ShopSupplierPaymentType.Advance)));
            exception.Code.ShouldBe("ShopManagement:TenantRequired");
        }
    }

    [Fact]
    public async Task Amount_Must_Be_Greater_Than_Zero()
    {
        var tenantId = await CreateTenantAsync("tenant-invalid-amount-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync();
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 0, ShopSupplierPaymentType.Advance)));
            exception.Code.ShouldBe("ShopManagement:SupplierPaymentInvalidAmount");
        }
    }

    [Fact]
    public async Task Cheque_Method_Requires_ChequeNumber()
    {
        var tenantId = await CreateTenantAsync("tenant-cheque-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync();
            var input = BuildPaymentInput(supplier.Id, 100, ShopSupplierPaymentType.Advance, ShopSupplierPaymentMethod.Cheque);
            input.BankName = "Test Bank";
            var exception = await Should.ThrowAsync<BusinessException>(() => _paymentAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:SupplierPaymentChequeNumberRequired");
        }
    }

    [Fact]
    public async Task BankTransfer_Method_Requires_BankName()
    {
        var tenantId = await CreateTenantAsync("tenant-bank-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync();
            var input = BuildPaymentInput(supplier.Id, 100, ShopSupplierPaymentType.Advance, ShopSupplierPaymentMethod.BankTransfer);
            var exception = await Should.ThrowAsync<BusinessException>(() => _paymentAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:SupplierPaymentBankNameRequired");
        }
    }

    [Fact]
    public async Task InvoicePayment_Requires_At_Least_One_Allocation()
    {
        var tenantId = await CreateTenantAsync("tenant-requires-alloc-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync();
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 100)));
            exception.Code.ShouldBe("ShopManagement:SupplierPaymentRequiresAllocation");
        }
    }

    [Fact]
    public async Task Advance_Payment_Allows_Zero_Allocations()
    {
        var tenantId = await CreateTenantAsync("tenant-advance-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync();
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 500, ShopSupplierPaymentType.Advance));
            payment.Allocations.ShouldBeEmpty();
            payment.AllocatedAmount.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Allocation_Total_Cannot_Exceed_Payment_Amount()
    {
        var tenantId = await CreateTenantAsync("tenant-exceeds-amount-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(quantity: 10, price: 100);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 500, allocations: new[] { (gr.Id, 600m) })));
            exception.Code.ShouldBe("ShopManagement:SupplierPaymentAllocationExceedsAmount");
        }
    }

    [Fact]
    public async Task Allocation_Cannot_Exceed_GoodsReceipt_Pending_Amount()
    {
        var tenantId = await CreateTenantAsync("tenant-exceeds-pending-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(quantity: 10, price: 100);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 2000, allocations: new[] { (gr.Id, 1500m) })));
            exception.Code.ShouldBe("ShopManagement:SupplierPaymentAllocationExceedsPending");
        }
    }

    [Fact]
    public async Task Allocation_GoodsReceipt_Must_Belong_To_Same_Supplier()
    {
        var tenantId = await CreateTenantAsync("tenant-supplier-mismatch-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, gr) = await CreateCompletedGoodsReceiptAsync();
            var otherSupplier = await CreateSupplierAsync();
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(otherSupplier.Id, 500, allocations: new[] { (gr.Id, 500m) })));
            exception.Code.ShouldBe("ShopManagement:SupplierPaymentGoodsReceiptSupplierMismatch");
        }
    }

    [Fact]
    public async Task Allocation_GoodsReceipt_Must_Be_Completed()
    {
        var tenantId = await CreateTenantAsync("tenant-not-completed-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, supplier) = await CreateApprovedPurchaseOrderAsync(quantity: 10, price: 100);
            var draftGr = await _grAppService.CreateAsync(BuildGrCreateInput(po, (po.Items[0].Id, 10, 0, 100, 0, 0)));

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 500, allocations: new[] { (draftGr.Id, 500m) })));
            exception.Code.ShouldBe("ShopManagement:SupplierPaymentGoodsReceiptNotCompleted");
        }
    }

    [Fact]
    public async Task Duplicate_GoodsReceipt_Allocation_In_Same_Payment_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-dup-alloc-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(quantity: 10, price: 100);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 1000, allocations: new[] { (gr.Id, 400m), (gr.Id, 400m) })));
            exception.Code.ShouldBe("ShopManagement:SupplierPaymentDuplicateGoodsReceiptAllocation");
        }
    }

    [Fact]
    public async Task Draft_Payment_Can_Be_Edited_And_Deleted()
    {
        var tenantId = await CreateTenantAsync("tenant-draft-edit-delete-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(quantity: 10, price: 100);
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 500, allocations: new[] { (gr.Id, 500m) }));

            var updated = await _paymentAppService.UpdateAsync(payment.Id, BuildPaymentInput(supplier.Id, 300, allocations: new[] { (gr.Id, 300m) }));
            updated.Amount.ShouldBe(300);
            updated.Allocations.Single().AllocatedAmount.ShouldBe(300);

            await _paymentAppService.DeleteAsync(payment.Id);
            await Should.ThrowAsync<BusinessException>(() => _paymentAppService.GetAsync(payment.Id));
        }
    }

    [Fact]
    public async Task Posted_Payment_Cannot_Be_Edited_Or_Deleted()
    {
        var tenantId = await CreateTenantAsync("tenant-posted-immutable-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(quantity: 10, price: 100);
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 500, allocations: new[] { (gr.Id, 500m) }));
            await _paymentAppService.PostAsync(payment.Id);

            var editException = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.UpdateAsync(payment.Id, BuildPaymentInput(supplier.Id, 300, allocations: new[] { (gr.Id, 300m) })));
            editException.Code.ShouldBe("ShopManagement:SupplierPaymentCannotBeEdited");

            var deleteException = await Should.ThrowAsync<BusinessException>(() => _paymentAppService.DeleteAsync(payment.Id));
            deleteException.Code.ShouldBe("ShopManagement:SupplierPaymentCannotBeDeleted");
        }
    }

    [Fact]
    public async Task Only_Draft_Payment_Can_Be_Posted()
    {
        var tenantId = await CreateTenantAsync("tenant-post-twice-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(quantity: 10, price: 100);
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 500, allocations: new[] { (gr.Id, 500m) }));
            var posted = await _paymentAppService.PostAsync(payment.Id);
            posted.Status.ShouldBe(ShopSupplierPaymentStatus.Posted);
            posted.PostedByUserId.ShouldNotBeNull();

            var exception = await Should.ThrowAsync<BusinessException>(() => _paymentAppService.PostAsync(payment.Id));
            exception.Code.ShouldBe("ShopManagement:SupplierPaymentCannotBePosted");
        }
    }

    [Fact]
    public async Task Only_Posted_Payment_Can_Be_Cancelled()
    {
        var tenantId = await CreateTenantAsync("tenant-cancel-draft-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync();
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 500, ShopSupplierPaymentType.Advance));

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CancelAsync(payment.Id, new CancelShopSupplierPaymentDto { CancellationReason = "test" }));
            exception.Code.ShouldBe("ShopManagement:SupplierPaymentCannotBeCancelled");
        }
    }

    [Fact]
    public async Task Draft_Payment_Does_Not_Reduce_GoodsReceipt_Pending_Amount()
    {
        var tenantId = await CreateTenantAsync("tenant-draft-no-effect-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(quantity: 10, price: 100);
            await _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 700, allocations: new[] { (gr.Id, 700m) }));

            var outstanding = await _paymentAppService.GetOutstandingReceiptsAsync(supplier.Id);
            var receipt = outstanding.Single(x => x.GoodsReceiptId == gr.Id);
            receipt.PendingAmount.ShouldBe(1000m);
        }
    }

    [Fact]
    public async Task Posting_Payment_Reduces_GoodsReceipt_Pending_Amount()
    {
        var tenantId = await CreateTenantAsync("tenant-posted-effect-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(quantity: 10, price: 100);
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 700, allocations: new[] { (gr.Id, 700m) }));
            await _paymentAppService.PostAsync(payment.Id);

            var outstanding = await _paymentAppService.GetOutstandingReceiptsAsync(supplier.Id);
            var receipt = outstanding.Single(x => x.GoodsReceiptId == gr.Id);
            receipt.PendingAmount.ShouldBe(300m);
            receipt.PaidAmount.ShouldBe(700m);
        }
    }

    [Fact]
    public async Task Cancelling_A_Posted_Payment_Reverses_Its_Allocation_Effect()
    {
        var tenantId = await CreateTenantAsync("tenant-cancel-effect-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(quantity: 10, price: 100);
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 700, allocations: new[] { (gr.Id, 700m) }));
            await _paymentAppService.PostAsync(payment.Id);

            var cancelled = await _paymentAppService.CancelAsync(payment.Id, new CancelShopSupplierPaymentDto { CancellationReason = "wrong entry" });
            cancelled.Status.ShouldBe(ShopSupplierPaymentStatus.Cancelled);
            cancelled.CancelledByUserId.ShouldNotBeNull();
            cancelled.CancellationReason.ShouldBe("wrong entry");

            var outstanding = await _paymentAppService.GetOutstandingReceiptsAsync(supplier.Id);
            var receipt = outstanding.Single(x => x.GoodsReceiptId == gr.Id);
            receipt.PendingAmount.ShouldBe(1000m);
        }
    }

    [Fact]
    public async Task GetOutstandingReceipts_Excludes_Fully_Paid_GoodsReceipts()
    {
        var tenantId = await CreateTenantAsync("tenant-fully-paid-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(quantity: 10, price: 100);
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 1000, allocations: new[] { (gr.Id, 1000m) }));
            await _paymentAppService.PostAsync(payment.Id);

            var outstanding = await _paymentAppService.GetOutstandingReceiptsAsync(supplier.Id);
            outstanding.ShouldNotContain(x => x.GoodsReceiptId == gr.Id);
        }
    }

    [Fact]
    public async Task GetOutstandingReceipts_Excludes_Draft_And_Cancelled_GoodsReceipts()
    {
        var tenantId = await CreateTenantAsync("tenant-draft-gr-excluded-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, supplier) = await CreateApprovedPurchaseOrderAsync(quantity: 10, price: 100);
            var draftGr = await _grAppService.CreateAsync(BuildGrCreateInput(po, (po.Items[0].Id, 10, 0, 100, 0, 0)));

            var outstanding = await _paymentAppService.GetOutstandingReceiptsAsync(supplier.Id);
            outstanding.ShouldNotContain(x => x.GoodsReceiptId == draftGr.Id);
        }
    }

    [Fact]
    public void TenantId_Is_Not_Accepted_Through_Dto()
    {
        typeof(CreateUpdateShopSupplierPaymentDto).GetProperty("TenantId").ShouldBeNull();
        typeof(CreateUpdateShopSupplierPaymentAllocationDto).GetProperty("TenantId").ShouldBeNull();
    }

    [Fact]
    public async Task Search_And_Filters_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-filters-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(quantity: 10, price: 100);
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(supplier.Id, 500, allocations: new[] { (gr.Id, 500m) }));

            (await _paymentAppService.GetListAsync(new GetShopSupplierPaymentsInput { Filter = payment.PaymentNumber })).TotalCount.ShouldBe(1);
            (await _paymentAppService.GetListAsync(new GetShopSupplierPaymentsInput { SupplierId = supplier.Id })).TotalCount.ShouldBe(1);
            (await _paymentAppService.GetListAsync(new GetShopSupplierPaymentsInput { Status = ShopSupplierPaymentStatus.Draft })).TotalCount.ShouldBe(1);
            (await _paymentAppService.GetListAsync(new GetShopSupplierPaymentsInput { PaymentType = ShopSupplierPaymentType.InvoicePayment })).TotalCount.ShouldBe(1);
            (await _paymentAppService.GetListAsync(new GetShopSupplierPaymentsInput { Status = ShopSupplierPaymentStatus.Posted })).TotalCount.ShouldBe(0);
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

    private async Task<ShopProductDto> CreateProductAsync()
    {
        var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        return await _productAppService.CreateAsync(new CreateShopProductDto
        {
            CategoryId = categoryId,
            UnitId = unitId,
            Name = "Product " + suffix,
            Code = "PRD-" + suffix,
            PurchasePrice = 10,
            SalePrice = 20,
            IsActive = true,
        });
    }

    private async Task<ShopSupplierDto> CreateSupplierAsync()
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
        return await _supplierAppService.CreateAsync(new CreateUpdateShopSupplierDto
        { Code = "SUP-" + suffix, Name = "Supplier " + suffix, IsActive = true });
    }

    private async Task<(ShopPurchaseOrderDto Po, ShopSupplierDto Supplier)> CreateApprovedPurchaseOrderAsync(decimal quantity, decimal price)
    {
        var supplier = await CreateSupplierAsync();
        var product = await CreateProductAsync();
        var po = await _poAppService.CreateAsync(BuildPoInput(supplier.Id, (product.Id, quantity, price, 0, 0)));
        await _poAppService.SubmitAsync(po.Id);
        var approved = await _poAppService.ApproveAsync(po.Id);
        return (approved, supplier);
    }

    private async Task<(ShopSupplierDto Supplier, ShopGoodsReceiptDto GoodsReceipt)> CreateCompletedGoodsReceiptAsync(decimal quantity = 10, decimal price = 100)
    {
        var (po, supplier) = await CreateApprovedPurchaseOrderAsync(quantity, price);
        var gr = await _grAppService.CreateAsync(BuildGrCreateInput(po, (po.Items[0].Id, quantity, 0, price, 0, 0)));
        var completed = await _grAppService.CompleteAsync(gr.Id);
        return (supplier, completed);
    }

    private static CreateShopPurchaseOrderDto BuildPoInput(Guid supplierId, (Guid ProductId, decimal Quantity, decimal Price, decimal Discount, decimal Tax) item) => new()
    {
        SupplierId = supplierId,
        OrderDate = DateTime.Today,
        Items = new List<CreateShopPurchaseOrderItemDto>
        {
            new() { ProductId = item.ProductId, OrderedQuantity = item.Quantity, UnitPurchasePrice = item.Price, DiscountPercentage = item.Discount, TaxPercentage = item.Tax }
        }
    };

    private static CreateShopGoodsReceiptDto BuildGrCreateInput(ShopPurchaseOrderDto po, (Guid PurchaseOrderItemId, decimal ReceivedQuantity, decimal BonusQuantity, decimal PurchasePrice, decimal Discount, decimal Tax) item) => new()
    {
        PurchaseOrderId = po.Id,
        ReceiptDate = DateTime.Today,
        Items = new List<CreateShopGoodsReceiptItemDto>
        {
            new()
            {
                PurchaseOrderItemId = item.PurchaseOrderItemId,
                ReceivedQuantity = item.ReceivedQuantity,
                BonusQuantity = item.BonusQuantity,
                PurchasePrice = item.PurchasePrice,
                DiscountPercentage = item.Discount,
                TaxPercentage = item.Tax,
            }
        }
    };

    private static CreateUpdateShopSupplierPaymentDto BuildPaymentInput(
        Guid supplierId,
        decimal amount,
        ShopSupplierPaymentType type = ShopSupplierPaymentType.InvoicePayment,
        ShopSupplierPaymentMethod method = ShopSupplierPaymentMethod.Cash,
        params (Guid GoodsReceiptId, decimal AllocatedAmount)[] allocations) => new()
    {
        SupplierId = supplierId,
        PaymentDate = DateTime.Today,
        PaymentType = type,
        PaymentMethod = method,
        Amount = amount,
        Allocations = allocations.Select(a => new CreateUpdateShopSupplierPaymentAllocationDto { GoodsReceiptId = a.GoodsReceiptId, AllocatedAmount = a.AllocatedAmount }).ToList()
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
