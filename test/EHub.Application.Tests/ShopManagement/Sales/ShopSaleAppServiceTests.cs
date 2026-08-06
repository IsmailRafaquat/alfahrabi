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
using EHub.ShopManagement.StockTransactions;
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

namespace EHub.ShopManagement.Sales;

public abstract class ShopSaleAppServiceTests<TStartupModule> : EHubApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShopSaleAppService _saleAppService;
    private readonly IShopCustomerAppService _customerAppService;
    private readonly IShopProductCategoryAppService _categoryAppService;
    private readonly IShopUnitAppService _unitAppService;
    private readonly IShopProductAppService _productAppService;
    private readonly IShopSupplierAppService _supplierAppService;
    private readonly IShopPurchaseOrderAppService _poAppService;
    private readonly IShopGoodsReceiptAppService _grAppService;
    private readonly IShopStockTransactionAppService _stockTransactionAppService;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly ICurrentTenant _currentTenant;

    protected ShopSaleAppServiceTests()
    {
        _saleAppService = GetRequiredService<IShopSaleAppService>();
        _customerAppService = GetRequiredService<IShopCustomerAppService>();
        _categoryAppService = GetRequiredService<IShopProductCategoryAppService>();
        _unitAppService = GetRequiredService<IShopUnitAppService>();
        _productAppService = GetRequiredService<IShopProductAppService>();
        _supplierAppService = GetRequiredService<IShopSupplierAppService>();
        _poAppService = GetRequiredService<IShopPurchaseOrderAppService>();
        _grAppService = GetRequiredService<IShopGoodsReceiptAppService>();
        _stockTransactionAppService = GetRequiredService<IShopStockTransactionAppService>();
        _tenantManager = GetRequiredService<ITenantManager>();
        _tenantRepository = GetRequiredService<IRepository<Tenant, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task TenantA_Can_Create_A_Draft_Sale()
    {
        var tenantId = await CreateTenantAsync("tenant-create-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(allowDecimal: false, stockQuantity: 100);

            var sale = await _saleAppService.CreateAsync(BuildCreateInput(customer.Id, DateTime.Today, null,
                ShopSaleType.Cash, 5000, (product.Id, 20, 250, 0, 0)));

            sale.Id.ShouldNotBe(Guid.Empty);
            sale.SaleNumber.ShouldStartWith("SAL-");
            sale.Status.ShouldBe(ShopSaleStatus.Draft);
            sale.GrandTotal.ShouldBe(5000);
            sale.PaidAmount.ShouldBe(5000);
            sale.PendingAmount.ShouldBe(0);
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Access_TenantB_Sale()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid saleId;
        using (_currentTenant.Change(tenantBId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(false, 100);
            var sale = await _saleAppService.CreateAsync(BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 100, (product.Id, 1, 100, 0, 0)));
            saleId = sale.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() => _saleAppService.GetAsync(saleId));
            exception.Code.ShouldBe("ShopManagement:SaleNotFound");
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
            var product = await CreateProductWithStockAsync(false, 10);
            var input = BuildCreateInput(otherCustomerId, DateTime.Today, null, ShopSaleType.Cash, 100, (product.Id, 1, 100, 0, 0));
            var exception = await Should.ThrowAsync<BusinessException>(() => _saleAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:SaleCustomerNotFound");
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Use_TenantB_Product()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-prod-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-prod-" + Guid.NewGuid().ToString("N"));

        Guid otherProductId;
        using (_currentTenant.Change(tenantBId))
        {
            otherProductId = (await CreateProductWithStockAsync(false, 10)).Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var customer = await CreateCustomerAsync();
            var input = BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 100, (otherProductId, 1, 100, 0, 0));
            var exception = await Should.ThrowAsync<BusinessException>(() => _saleAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:SaleProductNotFound");
        }
    }

    [Fact]
    public async Task Inactive_Customer_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-inactive-cust-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync(isActive: false);
            var product = await CreateProductWithStockAsync(false, 10);
            var input = BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 100, (product.Id, 1, 100, 0, 0));
            var exception = await Should.ThrowAsync<BusinessException>(() => _saleAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:SaleCustomerInactive");
        }
    }

    [Fact]
    public async Task Inactive_Product_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-inactive-prod-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(false, 0, isActive: false);
            var input = BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 100, (product.Id, 1, 100, 0, 0));
            var exception = await Should.ThrowAsync<BusinessException>(() => _saleAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:SaleProductInactive");
        }
    }

    [Fact]
    public async Task Sale_Requires_At_Least_One_Item()
    {
        var tenantId = await CreateTenantAsync("tenant-no-items-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var input = new CreateShopSaleDto
            {
                CustomerId = customer.Id,
                SaleDate = DateTime.Today,
                SaleType = ShopSaleType.Cash,
                PaidAmount = 0,
                Items = new List<CreateShopSaleItemDto>()
            };
            await Should.ThrowAsync<Exception>(() => _saleAppService.CreateAsync(input));
        }
    }

    [Fact]
    public async Task Quantity_Must_Be_Greater_Than_Zero()
    {
        var tenantId = await CreateTenantAsync("tenant-zero-qty-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(false, 10);
            var input = BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 0, (product.Id, 0, 100, 0, 0));
            var exception = await Should.ThrowAsync<BusinessException>(() => _saleAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:SaleInvalidQuantity");
        }
    }

    [Fact]
    public async Task WholeNumber_Unit_Rejects_Decimal_Quantity()
    {
        var tenantId = await CreateTenantAsync("tenant-whole-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(allowDecimal: false, stockQuantity: 10);
            var input = BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 100, (product.Id, 2.5m, 100, 0, 0));
            var exception = await Should.ThrowAsync<BusinessException>(() => _saleAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:SaleWholeQuantityRequired");
        }
    }

    [Fact]
    public async Task Decimal_Unit_Accepts_Decimal_Quantity()
    {
        var tenantId = await CreateTenantAsync("tenant-decimal-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(allowDecimal: true, stockQuantity: 10);
            var sale = await _saleAppService.CreateAsync(BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 250, (product.Id, 2.5m, 100, 0, 0)));
            sale.Items.Single().Quantity.ShouldBe(2.5m);
        }
    }

    [Fact]
    public async Task Insufficient_Stock_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-insufficient-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(false, 5);

            var sale = await _saleAppService.CreateAsync(BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 1000, (product.Id, 10, 100, 0, 0)));

            var exception = await Should.ThrowAsync<BusinessException>(() => _saleAppService.CompleteAsync(sale.Id, new CompleteShopSaleDto()));
            exception.Code.ShouldBe("ShopManagement:InsufficientProductStock");
        }
    }

    [Fact]
    public async Task Duplicate_Products_Are_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-dup-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(false, 10);
            var input = BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 100,
                (product.Id, 1, 100, 0, 0), (product.Id, 2, 100, 0, 0));
            var exception = await Should.ThrowAsync<BusinessException>(() => _saleAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:SaleDuplicateProduct");
        }
    }

    [Fact]
    public async Task Totals_Are_Recalculated_On_Server()
    {
        var tenantId = await CreateTenantAsync("tenant-totals-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(false, 100);

            var input = BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 0, (product.Id, 20, 250, 10, 5));
            input.OtherCharges = 100;
            input.PaidAmount = 0;

            var sale = await _saleAppService.CreateAsync(input);

            // LineSubTotal = 20*250 = 5000; DiscountAmount = 500; Taxable = 4500; TaxAmount = 225; LineTotal = 4725
            sale.SubTotal.ShouldBe(5000m);
            sale.DiscountAmount.ShouldBe(500m);
            sale.TaxAmount.ShouldBe(225m);
            sale.GrandTotal.ShouldBe(5000m - 500m + 225m + 100m);
        }
    }

    [Fact]
    public void Frontend_Provided_Totals_Are_Ignored()
    {
        typeof(CreateShopSaleDto).GetProperty("SubTotal").ShouldBeNull();
        typeof(CreateShopSaleDto).GetProperty("GrandTotal").ShouldBeNull();
        typeof(CreateShopSaleDto).GetProperty("SaleNumber").ShouldBeNull();
        typeof(CreateShopSaleDto).GetProperty("Status").ShouldBeNull();
        typeof(CreateShopSaleItemDto).GetProperty("LineTotal").ShouldBeNull();
        typeof(CreateShopSaleItemDto).GetProperty("UnitCostSnapshot").ShouldBeNull();
    }

    [Fact]
    public void TenantId_Is_Not_Accepted_Through_Dto()
    {
        typeof(CreateShopSaleDto).GetProperty("TenantId").ShouldBeNull();
        typeof(UpdateShopSaleDto).GetProperty("TenantId").ShouldBeNull();
    }

    [Fact]
    public async Task PaidAmount_Cannot_Exceed_GrandTotal()
    {
        var tenantId = await CreateTenantAsync("tenant-overpaid-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(false, 10);
            var input = BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 999999, (product.Id, 1, 100, 0, 0));
            var exception = await Should.ThrowAsync<BusinessException>(() => _saleAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:SalePaidAmountExceedsGrandTotal");
        }
    }

    [Fact]
    public async Task Credit_Sale_Requires_DueDate()
    {
        var tenantId = await CreateTenantAsync("tenant-credit-duedate-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(false, 10);
            var input = BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Credit, 0, (product.Id, 1, 100, 0, 0));
            var exception = await Should.ThrowAsync<BusinessException>(() => _saleAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:SaleDueDateRequiredForCreditSale");
        }
    }

    [Fact]
    public async Task Draft_Sale_Does_Not_Reduce_Stock()
    {
        var tenantId = await CreateTenantAsync("tenant-draft-no-stock-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(false, 100);
            var stockBefore = (await _productAppService.GetAsync(product.Id)).CurrentStock;

            await _saleAppService.CreateAsync(BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 5000, (product.Id, 20, 250, 0, 0)));

            (await _productAppService.GetAsync(product.Id)).CurrentStock.ShouldBe(stockBefore);
        }
    }

    [Fact]
    public async Task Completing_Sale_Reduces_Stock()
    {
        var tenantId = await CreateTenantAsync("tenant-complete-stock-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(false, 100);
            var stockBefore = (await _productAppService.GetAsync(product.Id)).CurrentStock;

            var sale = await _saleAppService.CreateAsync(BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 5000, (product.Id, 20, 250, 0, 0)));
            var completed = await _saleAppService.CompleteAsync(sale.Id, new CompleteShopSaleDto());

            completed.Status.ShouldBe(ShopSaleStatus.Completed);
            completed.CompletedByUserId.ShouldNotBeNull();
            (await _productAppService.GetAsync(product.Id)).CurrentStock.ShouldBe(stockBefore - 20);
        }
    }

    [Fact]
    public async Task Completing_Sale_Creates_Stock_Transactions()
    {
        var tenantId = await CreateTenantAsync("tenant-stock-txn-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(false, 100, purchasePrice: 100);
            var stockBefore = (await _productAppService.GetAsync(product.Id)).CurrentStock;

            var sale = await _saleAppService.CreateAsync(BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 5000, (product.Id, 20, 250, 0, 0)));
            await _saleAppService.CompleteAsync(sale.Id, new CompleteShopSaleDto());

            var transactions = await _stockTransactionAppService.GetListAsync(new GetShopStockTransactionsInput { ProductId = product.Id, TransactionType = ShopStockTransactionType.Sale });
            transactions.TotalCount.ShouldBe(1);
            var transaction = transactions.Items.Single();
            transaction.QuantityIn.ShouldBe(0);
            transaction.QuantityOut.ShouldBe(20);
            transaction.BalanceQuantity.ShouldBe(stockBefore - 20);
            transaction.UnitCost.ShouldBe(100);
            transaction.TransactionType.ShouldBe(ShopStockTransactionType.Sale);
            transaction.ReferenceType.ShouldBe(ShopStockReferenceType.Sale);
            transaction.ReferenceNumber.ShouldBe(sale.SaleNumber);
        }
    }

    [Fact]
    public async Task Completing_Twice_Is_Prevented()
    {
        var tenantId = await CreateTenantAsync("tenant-complete-twice-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(false, 100);
            var sale = await _saleAppService.CreateAsync(BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 5000, (product.Id, 20, 250, 0, 0)));
            await _saleAppService.CompleteAsync(sale.Id, new CompleteShopSaleDto());

            var exception = await Should.ThrowAsync<BusinessException>(() => _saleAppService.CompleteAsync(sale.Id, new CompleteShopSaleDto()));
            exception.Code.ShouldBe("ShopManagement:SaleAlreadyCompleted");
        }
    }

    [Fact]
    public async Task Completed_Sale_Cannot_Be_Edited_Or_Deleted()
    {
        var tenantId = await CreateTenantAsync("tenant-completed-immutable-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(false, 100);
            var sale = await _saleAppService.CreateAsync(BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 5000, (product.Id, 20, 250, 0, 0)));
            await _saleAppService.CompleteAsync(sale.Id, new CompleteShopSaleDto());

            var updateInput = new UpdateShopSaleDto
            {
                CustomerId = customer.Id,
                SaleDate = DateTime.Today,
                SaleType = ShopSaleType.Cash,
                PaidAmount = 0,
                Items = new List<UpdateShopSaleItemDto> { new() { ProductId = product.Id, Quantity = 1, UnitSalePrice = 100 } }
            };
            var editException = await Should.ThrowAsync<BusinessException>(() => _saleAppService.UpdateAsync(sale.Id, updateInput));
            editException.Code.ShouldBe("ShopManagement:SaleCannotBeEdited");

            var deleteException = await Should.ThrowAsync<BusinessException>(() => _saleAppService.DeleteAsync(sale.Id));
            deleteException.Code.ShouldBe("ShopManagement:SaleCannotBeDeleted");
        }
    }

    [Fact]
    public async Task Draft_Sale_Can_Be_Cancelled()
    {
        var tenantId = await CreateTenantAsync("tenant-cancel-draft-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(false, 100);
            var sale = await _saleAppService.CreateAsync(BuildCreateInput(customer.Id, DateTime.Today, null, ShopSaleType.Cash, 5000, (product.Id, 20, 250, 0, 0)));

            var cancelled = await _saleAppService.CancelAsync(sale.Id, new CancelShopSaleDto { CancellationReason = "Customer changed mind" });
            cancelled.Status.ShouldBe(ShopSaleStatus.Cancelled);
            cancelled.CancellationReason.ShouldBe("Customer changed mind");

            (await _productAppService.GetAsync(product.Id)).CurrentStock.ShouldBe(100);
        }
    }

    [Fact]
    public async Task Credit_Limit_Validation_Works()
    {
        var tenantId = await CreateTenantAsync("tenant-credit-limit-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync(creditLimit: 1000);
            var product = await CreateProductWithStockAsync(false, 100);

            // GrandTotal = 20 * 250 = 5000, PaidAmount 0 => PendingAmount 5000 > CreditLimit 1000
            var input = BuildCreateInput(customer.Id, DateTime.Today, DateTime.Today.AddDays(30), ShopSaleType.Credit, 0, (product.Id, 20, 250, 0, 0));
            var exception = await Should.ThrowAsync<BusinessException>(() => _saleAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:SaleExceedsCustomerCreditLimit");

            // Within the limit succeeds
            var withinLimitInput = BuildCreateInput(customer.Id, DateTime.Today, DateTime.Today.AddDays(30), ShopSaleType.Credit, 0, (product.Id, 2, 250, 0, 0));
            var sale = await _saleAppService.CreateAsync(withinLimitInput);
            sale.PendingAmount.ShouldBe(500);
        }
    }

    [Fact]
    public async Task Search_Filters_Paging_And_Sorting_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-filters-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(false, 100);

            var input1 = BuildCreateInput(customer.Id, new DateTime(2026, 1, 1), null, ShopSaleType.Cash, 250, (product.Id, 1, 250, 0, 0));
            var sale1 = await _saleAppService.CreateAsync(input1);

            var input2 = BuildCreateInput(customer.Id, new DateTime(2026, 3, 1), new DateTime(2026, 4, 1), ShopSaleType.Credit, 200, (product.Id, 2, 250, 0, 0));
            var sale2 = await _saleAppService.CreateAsync(input2);

            (await _saleAppService.GetListAsync(new GetShopSalesInput { Filter = sale1.SaleNumber })).TotalCount.ShouldBe(1);
            (await _saleAppService.GetListAsync(new GetShopSalesInput { CustomerId = customer.Id })).TotalCount.ShouldBe(2);
            (await _saleAppService.GetListAsync(new GetShopSalesInput { Status = ShopSaleStatus.Draft })).TotalCount.ShouldBe(2);
            (await _saleAppService.GetListAsync(new GetShopSalesInput { SaleType = ShopSaleType.Cash })).TotalCount.ShouldBe(1);
            (await _saleAppService.GetListAsync(new GetShopSalesInput { SaleDateFrom = new DateTime(2026, 2, 1) })).TotalCount.ShouldBe(1);
            (await _saleAppService.GetListAsync(new GetShopSalesInput { SaleDateTo = new DateTime(2025, 12, 31) })).TotalCount.ShouldBe(0);

            var page1 = await _saleAppService.GetListAsync(new GetShopSalesInput { MaxResultCount = 1, SkipCount = 0 });
            page1.TotalCount.ShouldBe(2);
            page1.Items.Single().SaleDate.ShouldBe(sale2.SaleDate);

            var noPendingList = await _saleAppService.GetListAsync(new GetShopSalesInput { HasPendingAmount = false });
            noPendingList.TotalCount.ShouldBe(1);
            var pendingList = await _saleAppService.GetListAsync(new GetShopSalesInput { HasPendingAmount = true });
            pendingList.TotalCount.ShouldBe(1);
            pendingList.Items.Single().Id.ShouldBe(sale2.Id);
        }
    }

    [Fact]
    public async Task Host_Context_Cannot_Create_A_Sale()
    {
        using (_currentTenant.Change(null))
        {
            var input = new CreateShopSaleDto
            {
                CustomerId = Guid.NewGuid(),
                SaleDate = DateTime.Today,
                SaleType = ShopSaleType.Cash,
                PaidAmount = 0,
                Items = new List<CreateShopSaleItemDto> { new() { ProductId = Guid.NewGuid(), Quantity = 1, UnitSalePrice = 10 } }
            };
            var exception = await Should.ThrowAsync<BusinessException>(() => _saleAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:TenantRequired");
        }
    }

    /// <summary>
    /// The test host registers AddAlwaysAllowAuthorization(), so permission checks cannot be
    /// exercised end-to-end here. This verifies the [Authorize] attributes themselves are present
    /// with the correct policy names, by static reflection.
    /// </summary>
    [Fact]
    public void Permissions_Are_Enforced()
    {
        var type = typeof(ShopSaleAppService);
        var classAuthorize = type.GetCustomAttribute<AuthorizeAttribute>();
        classAuthorize.ShouldNotBeNull();
        classAuthorize!.Policy.ShouldBe(EHubPermissions.ShopSales.Default);

        AssertMethodPolicy(type, nameof(ShopSaleAppService.CreateAsync), EHubPermissions.ShopSales.Create);
        AssertMethodPolicy(type, nameof(ShopSaleAppService.UpdateAsync), EHubPermissions.ShopSales.Edit);
        AssertMethodPolicy(type, nameof(ShopSaleAppService.DeleteAsync), EHubPermissions.ShopSales.Delete);
        AssertMethodPolicy(type, nameof(ShopSaleAppService.CompleteAsync), EHubPermissions.ShopSales.Complete);
        AssertMethodPolicy(type, nameof(ShopSaleAppService.CancelAsync), EHubPermissions.ShopSales.Cancel);
    }

    private static void AssertMethodPolicy(Type type, string methodName, string expectedPolicy)
    {
        var method = type.GetMethod(methodName) ?? throw new InvalidOperationException($"Method {methodName} not found.");
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();
        authorize.ShouldNotBeNull();
        authorize!.Policy.ShouldBe(expectedPolicy);
    }

    private static CreateShopSaleDto BuildCreateInput(
        Guid customerId,
        DateTime saleDate,
        DateTime? dueDate,
        ShopSaleType saleType,
        decimal paidAmount,
        params (Guid ProductId, decimal Quantity, decimal Price, decimal Discount, decimal Tax)[] items) => new()
    {
        CustomerId = customerId,
        SaleDate = saleDate,
        DueDate = dueDate,
        SaleType = saleType,
        PaymentMethod = ShopSalePaymentMethod.Cash,
        PaidAmount = paidAmount,
        Items = items.Select(i => new CreateShopSaleItemDto
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitSalePrice = i.Price,
            DiscountPercentage = i.Discount,
            TaxPercentage = i.Tax
        }).ToList()
    };

    private async Task<ShopCustomerDto> CreateCustomerAsync(ShopCustomerType type = ShopCustomerType.Business, decimal creditLimit = 0, decimal openingBalance = 0, bool isActive = true)
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        return await _customerAppService.CreateAsync(new CreateUpdateShopCustomerDto
        {
            Code = "CUS-" + suffix,
            Name = "Customer " + suffix,
            CustomerType = type,
            OpeningBalance = openingBalance,
            CreditLimit = creditLimit,
            PaymentTermsDays = 30,
            IsActive = isActive,
        });
    }

    private async Task<ShopProductDto> CreateProductWithStockAsync(
        bool allowDecimal, decimal stockQuantity, decimal purchasePrice = 100, decimal salePrice = 250, bool isActive = true, bool trackBatch = false, bool trackExpiry = false)
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        var category = await _categoryAppService.CreateAsync(new CreateUpdateShopProductCategoryDto
        { Name = "Category " + suffix, Code = "CAT-" + suffix, DisplayOrder = 0, IsActive = true });
        var unit = await _unitAppService.CreateAsync(new CreateUpdateShopUnitDto
        { Name = "Unit " + suffix, ShortName = suffix.Substring(0, 4), AllowDecimal = allowDecimal, IsActive = true });
        var product = await _productAppService.CreateAsync(new CreateShopProductDto
        {
            CategoryId = category.Id,
            UnitId = unit.Id,
            Name = "Product " + suffix,
            Code = "PRD-" + suffix,
            PurchasePrice = purchasePrice,
            SalePrice = salePrice,
            TrackBatch = trackBatch,
            TrackExpiry = trackExpiry,
            IsActive = isActive,
        });

        if (stockQuantity > 0)
        {
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
        }

        return await _productAppService.GetAsync(product.Id);
    }

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
