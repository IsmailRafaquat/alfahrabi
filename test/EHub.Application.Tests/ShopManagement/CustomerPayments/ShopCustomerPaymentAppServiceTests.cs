using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Customers;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.Sales;
using EHub.ShopManagement.Suppliers;
using EHub.ShopManagement.Units;
using Microsoft.AspNetCore.Authorization;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Modularity;
using Xunit;

namespace EHub.ShopManagement.CustomerPayments;

public abstract class ShopCustomerPaymentAppServiceTests<TStartupModule> : EHubApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShopCustomerPaymentAppService _paymentAppService;
    private readonly IShopCustomerAppService _customerAppService;
    private readonly IShopSaleAppService _saleAppService;
    private readonly IShopProductCategoryAppService _categoryAppService;
    private readonly IShopUnitAppService _unitAppService;
    private readonly IShopProductAppService _productAppService;
    private readonly IShopSupplierAppService _supplierAppService;
    private readonly IShopPurchaseOrderAppService _poAppService;
    private readonly IShopGoodsReceiptAppService _grAppService;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly ICurrentTenant _currentTenant;

    protected ShopCustomerPaymentAppServiceTests()
    {
        _paymentAppService = GetRequiredService<IShopCustomerPaymentAppService>();
        _customerAppService = GetRequiredService<IShopCustomerAppService>();
        _saleAppService = GetRequiredService<IShopSaleAppService>();
        _categoryAppService = GetRequiredService<IShopProductCategoryAppService>();
        _unitAppService = GetRequiredService<IShopUnitAppService>();
        _productAppService = GetRequiredService<IShopProductAppService>();
        _supplierAppService = GetRequiredService<IShopSupplierAppService>();
        _poAppService = GetRequiredService<IShopPurchaseOrderAppService>();
        _grAppService = GetRequiredService<IShopGoodsReceiptAppService>();
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
            var (customer, sale) = await CreateCompletedSaleAsync();
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 700, allocations: new[] { (sale.Id, 700m) }));

            payment.Id.ShouldNotBe(Guid.Empty);
            payment.PaymentNumber.ShouldStartWith("CP-");
            payment.Status.ShouldBe(ShopCustomerPaymentStatus.Draft);
            payment.Allocations.Single().AllocatedAmount.ShouldBe(700);
            payment.AllocatedAmount.ShouldBe(700);
            payment.UnallocatedAmount.ShouldBe(0);
        }
    }

    [Fact]
    public async Task PaymentNumber_Is_Server_Generated_And_Unique_Per_Tenant()
    {
        var tenantId = await CreateTenantAsync("tenant-numbering-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, sale) = await CreateCompletedSaleAsync(quantity: 10, price: 100);
            var first = await _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 500, allocations: new[] { (sale.Id, 500m) }));
            var second = await _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 300, ShopCustomerPaymentType.Advance));

            first.PaymentNumber.ShouldBe("CP-000001");
            second.PaymentNumber.ShouldBe("CP-000002");
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
            var (customer, sale) = await CreateCompletedSaleAsync();
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 500, allocations: new[] { (sale.Id, 500m) }));
            paymentId = payment.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() => _paymentAppService.GetAsync(paymentId));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentNotFound");
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Use_TenantB_Customer()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-cust-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-cust-" + Guid.NewGuid().ToString("N"));

        Guid otherCustomerId;
        using (_currentTenant.Change(tenantBId))
        {
            otherCustomerId = (await CreateCustomerAsync()).Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(otherCustomerId, 100, ShopCustomerPaymentType.Advance)));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentCustomerNotFound");
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Use_TenantB_Sale()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-sale-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-sale-" + Guid.NewGuid().ToString("N"));

        Guid otherSaleId;
        using (_currentTenant.Change(tenantBId))
        {
            var (_, sale) = await CreateCompletedSaleAsync();
            otherSaleId = sale.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var customer = await CreateCustomerAsync();
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 500, allocations: new[] { (otherSaleId, 500m) })));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentSaleNotFound");
        }
    }

    [Fact]
    public async Task Host_Context_Cannot_Create_A_Payment()
    {
        using (_currentTenant.Change(null))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(Guid.NewGuid(), 100, ShopCustomerPaymentType.Advance)));
            exception.Code.ShouldBe("ShopManagement:TenantRequired");
        }
    }

    [Fact]
    public async Task Amount_Must_Be_Greater_Than_Zero()
    {
        var tenantId = await CreateTenantAsync("tenant-invalid-amount-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 0, ShopCustomerPaymentType.Advance)));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentInvalidAmount");
        }
    }

    [Fact]
    public async Task Allocation_Amount_Must_Be_Greater_Than_Zero()
    {
        var tenantId = await CreateTenantAsync("tenant-invalid-alloc-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, sale) = await CreateCompletedSaleAsync();
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 500, allocations: new[] { (sale.Id, 0m) })));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentInvalidAllocationAmount");
        }
    }

    [Fact]
    public async Task Cheque_Method_Requires_ChequeNumber()
    {
        var tenantId = await CreateTenantAsync("tenant-cheque-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var input = BuildPaymentInput(customer.Id, 100, ShopCustomerPaymentType.Advance, ShopCustomerPaymentMethod.Cheque);
            input.BankName = "Test Bank";
            var exception = await Should.ThrowAsync<BusinessException>(() => _paymentAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentChequeNumberRequired");
        }
    }

    [Fact]
    public async Task BankTransfer_Method_Requires_BankName()
    {
        var tenantId = await CreateTenantAsync("tenant-bank-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var input = BuildPaymentInput(customer.Id, 100, ShopCustomerPaymentType.Advance, ShopCustomerPaymentMethod.BankTransfer);
            var exception = await Should.ThrowAsync<BusinessException>(() => _paymentAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentBankNameRequired");
        }
    }

    [Fact]
    public async Task InvoicePayment_Requires_At_Least_One_Allocation()
    {
        var tenantId = await CreateTenantAsync("tenant-requires-alloc-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 100)));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentRequiresAllocation");
        }
    }

    [Fact]
    public async Task Advance_Payment_Allows_Zero_Allocations()
    {
        var tenantId = await CreateTenantAsync("tenant-advance-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 500, ShopCustomerPaymentType.Advance));
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
            var (customer, sale) = await CreateCompletedSaleAsync(quantity: 10, price: 100);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 500, allocations: new[] { (sale.Id, 600m) })));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentAllocationExceedsAmount");
        }
    }

    [Fact]
    public async Task Allocation_Cannot_Exceed_Sale_Pending_Amount()
    {
        var tenantId = await CreateTenantAsync("tenant-exceeds-pending-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, sale) = await CreateCompletedSaleAsync(quantity: 10, price: 100, paidAmount: 0);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 2000, allocations: new[] { (sale.Id, 1500m) })));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentAllocationExceedsPending");
        }
    }

    [Fact]
    public async Task Allocation_Sale_Must_Belong_To_Same_Customer()
    {
        var tenantId = await CreateTenantAsync("tenant-customer-mismatch-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, sale) = await CreateCompletedSaleAsync();
            var otherCustomer = await CreateCustomerAsync();
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(otherCustomer.Id, 500, allocations: new[] { (sale.Id, 500m) })));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentSaleCustomerMismatch");
        }
    }

    [Fact]
    public async Task Allocation_Sale_Must_Be_Completed()
    {
        var tenantId = await CreateTenantAsync("tenant-not-completed-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(10);
            var draftSale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, (product.Id, 5, 100, 0, 0)));

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 500, allocations: new[] { (draftSale.Id, 500m) })));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentSaleNotCompleted");
        }
    }

    [Fact]
    public async Task Duplicate_Sale_Allocation_In_Same_Payment_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-dup-alloc-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, sale) = await CreateCompletedSaleAsync(quantity: 10, price: 100);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 1000, allocations: new[] { (sale.Id, 400m), (sale.Id, 400m) })));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentDuplicateSaleAllocation");
        }
    }

    [Fact]
    public async Task Draft_Payment_Can_Be_Edited_And_Deleted()
    {
        var tenantId = await CreateTenantAsync("tenant-draft-edit-delete-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, sale) = await CreateCompletedSaleAsync(quantity: 10, price: 100);
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 500, allocations: new[] { (sale.Id, 500m) }));

            var updated = await _paymentAppService.UpdateAsync(payment.Id, BuildPaymentInput(customer.Id, 300, allocations: new[] { (sale.Id, 300m) }));
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
            var (customer, sale) = await CreateCompletedSaleAsync(quantity: 10, price: 100);
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 500, allocations: new[] { (sale.Id, 500m) }));
            await _paymentAppService.PostAsync(payment.Id);

            var editException = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.UpdateAsync(payment.Id, BuildPaymentInput(customer.Id, 300, allocations: new[] { (sale.Id, 300m) })));
            editException.Code.ShouldBe("ShopManagement:CustomerPaymentCannotBeEdited");

            var deleteException = await Should.ThrowAsync<BusinessException>(() => _paymentAppService.DeleteAsync(payment.Id));
            deleteException.Code.ShouldBe("ShopManagement:CustomerPaymentCannotBeDeleted");
        }
    }

    [Fact]
    public async Task Only_Draft_Payment_Can_Be_Posted()
    {
        var tenantId = await CreateTenantAsync("tenant-post-twice-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, sale) = await CreateCompletedSaleAsync(quantity: 10, price: 100);
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 500, allocations: new[] { (sale.Id, 500m) }));
            var posted = await _paymentAppService.PostAsync(payment.Id);
            posted.Status.ShouldBe(ShopCustomerPaymentStatus.Posted);
            posted.PostedByUserId.ShouldNotBeNull();

            var exception = await Should.ThrowAsync<BusinessException>(() => _paymentAppService.PostAsync(payment.Id));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentCannotBePosted");
        }
    }

    [Fact]
    public async Task Only_Posted_Payment_Can_Be_Cancelled()
    {
        var tenantId = await CreateTenantAsync("tenant-cancel-draft-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 500, ShopCustomerPaymentType.Advance));

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _paymentAppService.CancelAsync(payment.Id, new CancelShopCustomerPaymentDto { CancellationReason = "test" }));
            exception.Code.ShouldBe("ShopManagement:CustomerPaymentCannotBeCancelled");
        }
    }

    [Fact]
    public async Task Draft_Payment_Does_Not_Reduce_Sale_Pending_Amount()
    {
        var tenantId = await CreateTenantAsync("tenant-draft-no-effect-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, sale) = await CreateCompletedSaleAsync(quantity: 10, price: 100, paidAmount: 300);
            await _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 700, allocations: new[] { (sale.Id, 700m) }));

            var outstanding = await _paymentAppService.GetOutstandingSalesAsync(customer.Id);
            var row = outstanding.Items.Single(x => x.SaleId == sale.Id);
            row.PendingAmount.ShouldBe(700m);
        }
    }

    [Fact]
    public async Task Posting_Payment_Reduces_Sale_Pending_Amount()
    {
        var tenantId = await CreateTenantAsync("tenant-posted-effect-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, sale) = await CreateCompletedSaleAsync(quantity: 10, price: 100, paidAmount: 300);
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 700, allocations: new[] { (sale.Id, 700m) }));
            await _paymentAppService.PostAsync(payment.Id);

            var outstanding = await _paymentAppService.GetOutstandingSalesAsync(customer.Id);
            outstanding.Items.ShouldNotContain(x => x.SaleId == sale.Id);
        }
    }

    [Fact]
    public async Task Cancelling_A_Posted_Payment_Reverses_Its_Allocation_Effect()
    {
        var tenantId = await CreateTenantAsync("tenant-cancel-effect-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, sale) = await CreateCompletedSaleAsync(quantity: 10, price: 100, paidAmount: 300);
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 700, allocations: new[] { (sale.Id, 700m) }));
            await _paymentAppService.PostAsync(payment.Id);

            var cancelled = await _paymentAppService.CancelAsync(payment.Id, new CancelShopCustomerPaymentDto { CancellationReason = "wrong entry" });
            cancelled.Status.ShouldBe(ShopCustomerPaymentStatus.Cancelled);
            cancelled.CancelledByUserId.ShouldNotBeNull();
            cancelled.CancellationReason.ShouldBe("wrong entry");

            var outstanding = await _paymentAppService.GetOutstandingSalesAsync(customer.Id);
            var row = outstanding.Items.Single(x => x.SaleId == sale.Id);
            row.PendingAmount.ShouldBe(700m);
        }
    }

    [Fact]
    public async Task GetOutstandingSales_Excludes_Fully_Paid_Sales()
    {
        var tenantId = await CreateTenantAsync("tenant-fully-paid-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, sale) = await CreateCompletedSaleAsync(quantity: 10, price: 100, paidAmount: 0);
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 1000, allocations: new[] { (sale.Id, 1000m) }));
            await _paymentAppService.PostAsync(payment.Id);

            var outstanding = await _paymentAppService.GetOutstandingSalesAsync(customer.Id);
            outstanding.Items.ShouldNotContain(x => x.SaleId == sale.Id);
        }
    }

    [Fact]
    public async Task GetOutstandingSales_Excludes_Draft_Sales()
    {
        var tenantId = await CreateTenantAsync("tenant-draft-sale-excluded-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(10);
            var draftSale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, (product.Id, 5, 100, 0, 0)));

            var outstanding = await _paymentAppService.GetOutstandingSalesAsync(customer.Id);
            outstanding.Items.ShouldNotContain(x => x.SaleId == draftSale.Id);
        }
    }

    [Fact]
    public async Task Customer_Payment_Does_Not_Affect_Stock()
    {
        var tenantId = await CreateTenantAsync("tenant-no-stock-effect-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, sale) = await CreateCompletedSaleAsync(quantity: 10, price: 100, paidAmount: 300);
            var product = await _productAppService.GetAsync(sale.Items.Single().ProductId);
            var stockBefore = product.CurrentStock;

            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 700, allocations: new[] { (sale.Id, 700m) }));
            await _paymentAppService.PostAsync(payment.Id);

            (await _productAppService.GetAsync(sale.Items.Single().ProductId)).CurrentStock.ShouldBe(stockBefore);
        }
    }

    [Fact]
    public void TenantId_Is_Not_Accepted_Through_Dto()
    {
        typeof(CreateUpdateShopCustomerPaymentDto).GetProperty("TenantId").ShouldBeNull();
        typeof(CreateUpdateShopCustomerPaymentAllocationDto).GetProperty("TenantId").ShouldBeNull();
    }

    /// <summary>
    /// The test host registers AddAlwaysAllowAuthorization(), so permission checks (including the
    /// ViewAmount field-hiding behavior) cannot be exercised end-to-end here. This verifies the
    /// [Authorize] attributes themselves are present with the correct policy names, by static reflection.
    /// </summary>
    [Fact]
    public void Permissions_Are_Enforced()
    {
        var type = typeof(ShopCustomerPaymentAppService);
        var classAuthorize = type.GetCustomAttribute<AuthorizeAttribute>();
        classAuthorize.ShouldNotBeNull();
        classAuthorize!.Policy.ShouldBe(EHubPermissions.ShopCustomerPayments.Default);

        AssertMethodPolicy(type, nameof(ShopCustomerPaymentAppService.CreateAsync), EHubPermissions.ShopCustomerPayments.Create);
        AssertMethodPolicy(type, nameof(ShopCustomerPaymentAppService.UpdateAsync), EHubPermissions.ShopCustomerPayments.Edit);
        AssertMethodPolicy(type, nameof(ShopCustomerPaymentAppService.DeleteAsync), EHubPermissions.ShopCustomerPayments.Delete);
        AssertMethodPolicy(type, nameof(ShopCustomerPaymentAppService.PostAsync), EHubPermissions.ShopCustomerPayments.Post);
        AssertMethodPolicy(type, nameof(ShopCustomerPaymentAppService.CancelAsync), EHubPermissions.ShopCustomerPayments.Cancel);
    }

    private static void AssertMethodPolicy(Type type, string methodName, string expectedPolicy)
    {
        var method = type.GetMethod(methodName) ?? throw new InvalidOperationException($"Method {methodName} not found.");
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();
        authorize.ShouldNotBeNull();
        authorize!.Policy.ShouldBe(expectedPolicy);
    }

    [Fact]
    public async Task Search_Filters_Paging_And_Sorting_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-filters-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, sale) = await CreateCompletedSaleAsync(quantity: 10, price: 100);
            var payment = await _paymentAppService.CreateAsync(BuildPaymentInput(customer.Id, 500, allocations: new[] { (sale.Id, 500m) }));

            (await _paymentAppService.GetListAsync(new GetShopCustomerPaymentsInput { Filter = payment.PaymentNumber })).TotalCount.ShouldBe(1);
            (await _paymentAppService.GetListAsync(new GetShopCustomerPaymentsInput { CustomerId = customer.Id })).TotalCount.ShouldBe(1);
            (await _paymentAppService.GetListAsync(new GetShopCustomerPaymentsInput { Status = ShopCustomerPaymentStatus.Draft })).TotalCount.ShouldBe(1);
            (await _paymentAppService.GetListAsync(new GetShopCustomerPaymentsInput { PaymentType = ShopCustomerPaymentType.InvoicePayment })).TotalCount.ShouldBe(1);
            (await _paymentAppService.GetListAsync(new GetShopCustomerPaymentsInput { Status = ShopCustomerPaymentStatus.Posted })).TotalCount.ShouldBe(0);
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

    private async Task<ShopProductDto> CreateProductWithStockAsync(decimal stockQuantity, decimal purchasePrice = 100, decimal salePrice = 250)
    {
        var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        var product = await _productAppService.CreateAsync(new CreateShopProductDto
        {
            CategoryId = categoryId,
            UnitId = unitId,
            Name = "Product " + suffix,
            Code = "PRD-" + suffix,
            PurchasePrice = purchasePrice,
            SalePrice = salePrice,
            IsActive = true,
        });

        var supplier = await _supplierAppService.CreateAsync(new CreateUpdateShopSupplierDto
        { Code = "SUP-" + suffix, Name = "Supplier " + suffix, IsActive = true });

        var po = await _poAppService.CreateAsync(new CreateShopPurchaseOrderDto
        {
            SupplierId = supplier.Id,
            OrderDate = DateTime.Today,
            Items = new List<CreateShopPurchaseOrderItemDto>
            { new() { ProductId = product.Id, OrderedQuantity = stockQuantity, UnitPurchasePrice = purchasePrice } }
        });
        await _poAppService.SubmitAsync(po.Id);
        var approved = await _poAppService.ApproveAsync(po.Id);

        var gr = await _grAppService.CreateAsync(new CreateShopGoodsReceiptDto
        {
            PurchaseOrderId = approved.Id,
            ReceiptDate = DateTime.Today,
            Items = new List<CreateShopGoodsReceiptItemDto>
            { new() { PurchaseOrderItemId = approved.Items[0].Id, ReceivedQuantity = stockQuantity, PurchasePrice = purchasePrice } }
        });
        await _grAppService.CompleteAsync(gr.Id);

        return await _productAppService.GetAsync(product.Id);
    }

    private async Task<ShopCustomerDto> CreateCustomerAsync()
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        return await _customerAppService.CreateAsync(new CreateUpdateShopCustomerDto
        {
            Code = "CUS-" + suffix,
            Name = "Customer " + suffix,
            CustomerType = ShopCustomerType.Business,
            PaymentTermsDays = 30,
            IsActive = true,
        });
    }

    private async Task<(ShopCustomerDto Customer, ShopSaleDto Sale)> CreateCompletedSaleAsync(decimal quantity = 10, decimal price = 100, decimal paidAmount = 0)
    {
        var customer = await CreateCustomerAsync();
        var product = await CreateProductWithStockAsync(quantity, purchasePrice: price / 2, salePrice: price);
        var sale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, (product.Id, quantity, price, 0, 0), paidAmount));
        var completed = await _saleAppService.CompleteAsync(sale.Id);
        return (customer, completed);
    }

    private static CreateShopSaleDto BuildSaleCreateInput(Guid customerId, (Guid ProductId, decimal Quantity, decimal Price, decimal Discount, decimal Tax) item, decimal paidAmount = 0) => new()
    {
        CustomerId = customerId,
        SaleDate = DateTime.Today,
        SaleType = ShopSaleType.Credit,
        DueDate = DateTime.Today.AddDays(30),
        PaymentMethod = ShopSalePaymentMethod.Cash,
        PaidAmount = paidAmount,
        Items = new List<CreateShopSaleItemDto>
        {
            new() { ProductId = item.ProductId, Quantity = item.Quantity, UnitSalePrice = item.Price, DiscountPercentage = item.Discount, TaxPercentage = item.Tax }
        }
    };

    private static CreateUpdateShopCustomerPaymentDto BuildPaymentInput(
        Guid customerId,
        decimal amount,
        ShopCustomerPaymentType type = ShopCustomerPaymentType.InvoicePayment,
        ShopCustomerPaymentMethod method = ShopCustomerPaymentMethod.Cash,
        params (Guid SaleId, decimal AllocatedAmount)[] allocations) => new()
    {
        CustomerId = customerId,
        PaymentDate = DateTime.Today,
        PaymentType = type,
        PaymentMethod = method,
        Amount = amount,
        Allocations = allocations.Select(a => new CreateUpdateShopCustomerPaymentAllocationDto { SaleId = a.SaleId, AllocatedAmount = a.AllocatedAmount }).ToList()
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
