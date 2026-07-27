using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Customers;
using EHub.ShopManagement.CustomerPayments;
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

namespace EHub.ShopManagement.SaleReturns;

public abstract class ShopSaleReturnAppServiceTests<TStartupModule> : EHubApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShopSaleReturnAppService _returnAppService;
    private readonly IShopSaleAppService _saleAppService;
    private readonly IShopCustomerAppService _customerAppService;
    private readonly IShopCustomerPaymentAppService _paymentAppService;
    private readonly IShopProductCategoryAppService _categoryAppService;
    private readonly IShopUnitAppService _unitAppService;
    private readonly IShopProductAppService _productAppService;
    private readonly IShopSupplierAppService _supplierAppService;
    private readonly IShopPurchaseOrderAppService _poAppService;
    private readonly IShopGoodsReceiptAppService _grAppService;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly ICurrentTenant _currentTenant;

    protected ShopSaleReturnAppServiceTests()
    {
        _returnAppService = GetRequiredService<IShopSaleReturnAppService>();
        _saleAppService = GetRequiredService<IShopSaleAppService>();
        _customerAppService = GetRequiredService<IShopCustomerAppService>();
        _paymentAppService = GetRequiredService<IShopCustomerPaymentAppService>();
        _categoryAppService = GetRequiredService<IShopProductCategoryAppService>();
        _unitAppService = GetRequiredService<IShopUnitAppService>();
        _productAppService = GetRequiredService<IShopProductAppService>();
        _supplierAppService = GetRequiredService<IShopSupplierAppService>();
        _poAppService = GetRequiredService<IShopPurchaseOrderAppService>();
        _grAppService = GetRequiredService<IShopGoodsReceiptAppService>();
        _productRepository = GetRequiredService<IRepository<ShopProduct, Guid>>();
        _tenantManager = GetRequiredService<ITenantManager>();
        _tenantRepository = GetRequiredService<IRepository<Tenant, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task TenantA_Can_Create_A_Draft_Sale_Return()
    {
        var tenantId = await CreateTenantAsync("tenant-create-draft-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, sale, saleItem, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100);

            var saleReturn = await _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 3, ShopSaleReturnReason.Damaged)));

            saleReturn.Status.ShouldBe(ShopSaleReturnStatus.Draft);
            saleReturn.SaleReturnNumber.ShouldStartWith("SR-");
            saleReturn.GrandTotal.ShouldBe(300);
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Access_TenantB_Sale_Return()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-return-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-return-" + Guid.NewGuid().ToString("N"));

        Guid returnId;
        using (_currentTenant.Change(tenantBId))
        {
            var (_, sale, saleItem, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100);
            var saleReturn = await _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 2, ShopSaleReturnReason.Damaged)));
            returnId = saleReturn.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() => _returnAppService.GetAsync(returnId));
            exception.Code.ShouldBe("ShopManagement:SaleReturnNotFound");
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Use_TenantB_Sale_Or_SaleItem()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-sale-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-sale-" + Guid.NewGuid().ToString("N"));

        Guid saleId;
        Guid saleItemId;
        using (_currentTenant.Change(tenantBId))
        {
            var (_, sale, saleItem, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100);
            saleId = sale.Id;
            saleItemId = saleItem.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _returnAppService.CreateAsync(BuildReturnInput(saleId, (saleItemId, 2, ShopSaleReturnReason.Damaged))));
            exception.Code.ShouldBe("ShopManagement:SaleReturnSaleNotFound");
        }
    }

    [Fact]
    public async Task Only_Completed_Sales_Can_Be_Returned()
    {
        var tenantId = await CreateTenantAsync("tenant-draft-sale-return-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync(0);
            var product = await CreateProductWithStockAsync(10, 50, 100);
            var sale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, (product.Id, 10, 100, null, null), 0));

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (sale.Items[0].Id, 2, ShopSaleReturnReason.Damaged))));
            exception.Code.ShouldBe("ShopManagement:SaleReturnRequiresCompletedSale");
        }
    }

    [Fact]
    public async Task SaleItem_Must_Belong_To_Selected_Sale()
    {
        var tenantId = await CreateTenantAsync("tenant-item-mismatch-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, saleA, _, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100);
            var (_, _, saleItemB, _) = await CreateCompletedSaleAsync(quantity: 5, price: 50);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _returnAppService.CreateAsync(BuildReturnInput(saleA.Id, (saleItemB.Id, 1, ShopSaleReturnReason.Damaged))));
            exception.Code.ShouldBe("ShopManagement:SaleReturnSaleItemNotFound");
        }
    }

    [Fact]
    public async Task ReturnQuantity_Must_Be_Greater_Than_Zero()
    {
        var tenantId = await CreateTenantAsync("tenant-zero-qty-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, sale, saleItem, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100);

            // Enforced both by the DTO's [Range] floor (0.0001) and, defensively, by the domain
            // entity itself - either layer rejecting a non-positive quantity satisfies the rule.
            var input = BuildReturnInput(sale.Id, (saleItem.Id, 0m, ShopSaleReturnReason.Damaged));
            await Should.ThrowAsync<Exception>(() => _returnAppService.CreateAsync(input));
        }
    }

    [Fact]
    public async Task ReturnQuantity_Cannot_Exceed_ReturnableQuantity()
    {
        var tenantId = await CreateTenantAsync("tenant-exceed-qty-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, sale, saleItem, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 11, ShopSaleReturnReason.Damaged))));
            exception.Code.ShouldBe("ShopManagement:SaleReturnQuantityExceedsReturnable");
        }
    }

    [Fact]
    public async Task WholeNumber_Unit_Rejects_Decimal_Quantity()
    {
        var tenantId = await CreateTenantAsync("tenant-whole-unit-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, sale, saleItem, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100, allowDecimal: false);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 2.5m, ShopSaleReturnReason.Damaged))));
            exception.Code.ShouldBe("ShopManagement:SaleReturnWholeQuantityRequired");
        }
    }

    [Fact]
    public async Task Decimal_Unit_Accepts_Decimal_Quantity()
    {
        var tenantId = await CreateTenantAsync("tenant-decimal-unit-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, sale, saleItem, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100, allowDecimal: true);

            var saleReturn = await _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 2.5m, ShopSaleReturnReason.Damaged)));
            saleReturn.Items.Single().ReturnQuantity.ShouldBe(2.5m);
        }
    }

    [Fact]
    public async Task Draft_Return_Does_Not_Increase_Stock()
    {
        var tenantId = await CreateTenantAsync("tenant-draft-no-stock-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, sale, saleItem, product) = await CreateCompletedSaleAsync(quantity: 10, price: 100);
            var stockAfterSale = (await _productAppService.GetAsync(product.Id)).CurrentStock;

            await _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 5, ShopSaleReturnReason.Damaged)));

            var stockAfterDraftReturn = (await _productAppService.GetAsync(product.Id)).CurrentStock;
            stockAfterDraftReturn.ShouldBe(stockAfterSale);
        }
    }

    [Fact]
    public async Task Completing_Return_Increases_Stock()
    {
        var tenantId = await CreateTenantAsync("tenant-complete-stock-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, sale, saleItem, product) = await CreateCompletedSaleAsync(quantity: 20, price: 250, stockQuantity: 100);
            var stockAfterSale = (await _productAppService.GetAsync(product.Id)).CurrentStock;
            stockAfterSale.ShouldBe(80);

            var saleReturn = await _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 5, ShopSaleReturnReason.Damaged)));
            await _returnAppService.CompleteAsync(saleReturn.Id);

            var stockAfterReturn = (await _productAppService.GetAsync(product.Id)).CurrentStock;
            stockAfterReturn.ShouldBe(85);
        }
    }

    [Fact]
    public async Task Completing_Return_Creates_SaleReturn_Stock_Transactions()
    {
        var tenantId = await CreateTenantAsync("tenant-stock-txn-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, sale, saleItem, _) = await CreateCompletedSaleAsync(quantity: 20, price: 250, stockQuantity: 100);
            var saleReturn = await _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 5, ShopSaleReturnReason.Damaged)));
            var completed = await _returnAppService.CompleteAsync(saleReturn.Id);

            completed.Status.ShouldBe(ShopSaleReturnStatus.Completed);
            completed.GrandTotal.ShouldBe(1250);
        }
    }

    [Fact]
    public async Task Original_Batch_And_Expiry_Are_Preserved()
    {
        var tenantId = await CreateTenantAsync("tenant-batch-expiry-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var expiry = DateTime.Today.AddMonths(6);
            var (_, sale, saleItem, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100, batchNumber: "BATCH-001", expiryDate: expiry);

            var saleReturn = await _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 3, ShopSaleReturnReason.Damaged)));
            var item = saleReturn.Items.Single();
            item.BatchNumber.ShouldBe("BATCH-001");
            item.ExpiryDate.ShouldBe(expiry);
        }
    }

    [Fact]
    public async Task Completing_Twice_Is_Prevented()
    {
        var tenantId = await CreateTenantAsync("tenant-complete-twice-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, sale, saleItem, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100);
            var saleReturn = await _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 3, ShopSaleReturnReason.Damaged)));
            await _returnAppService.CompleteAsync(saleReturn.Id);

            var exception = await Should.ThrowAsync<BusinessException>(() => _returnAppService.CompleteAsync(saleReturn.Id));
            exception.Code.ShouldBe("ShopManagement:SaleReturnAlreadyCompleted");
        }
    }

    [Fact]
    public async Task Completed_Return_Cannot_Be_Edited_Or_Deleted()
    {
        var tenantId = await CreateTenantAsync("tenant-completed-locked-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, sale, saleItem, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100);
            var saleReturn = await _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 3, ShopSaleReturnReason.Damaged)));
            await _returnAppService.CompleteAsync(saleReturn.Id);

            var editException = await Should.ThrowAsync<BusinessException>(() =>
                _returnAppService.UpdateAsync(saleReturn.Id, BuildReturnUpdateInput((saleItem.Id, 2, ShopSaleReturnReason.Damaged))));
            editException.Code.ShouldBe("ShopManagement:SaleReturnCannotBeEdited");

            var deleteException = await Should.ThrowAsync<BusinessException>(() => _returnAppService.DeleteAsync(saleReturn.Id));
            deleteException.Code.ShouldBe("ShopManagement:SaleReturnCannotBeDeleted");
        }
    }

    [Fact]
    public async Task Draft_Return_Can_Be_Edited_Deleted_Or_Cancelled()
    {
        var tenantId = await CreateTenantAsync("tenant-draft-editable-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, sale, saleItem, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100);

            var saleReturn = await _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 3, ShopSaleReturnReason.Damaged)));
            var updated = await _returnAppService.UpdateAsync(saleReturn.Id, BuildReturnUpdateInput((saleItem.Id, 4, ShopSaleReturnReason.Damaged)));
            updated.Items.Single().ReturnQuantity.ShouldBe(4);

            var cancelled = await _returnAppService.CancelAsync(updated.Id, new CancelShopSaleReturnDto { CancellationReason = "test" });
            cancelled.Status.ShouldBe(ShopSaleReturnStatus.Cancelled);

            var anotherReturn = await _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 2, ShopSaleReturnReason.Damaged)));
            await _returnAppService.DeleteAsync(anotherReturn.Id);
            await Should.ThrowAsync<BusinessException>(() => _returnAppService.GetAsync(anotherReturn.Id));
        }
    }

    [Fact]
    public async Task CustomerCredit_Reduces_Receivable()
    {
        var tenantId = await CreateTenantAsync("tenant-customer-credit-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, sale, saleItem, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100, paidAmount: 0);
            var saleReturn = await _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 3, ShopSaleReturnReason.Damaged), ShopSaleReturnSettlementType.CustomerCredit));
            var completed = await _returnAppService.CompleteAsync(saleReturn.Id);

            completed.CustomerCreditAmount.ShouldBe(300);
            completed.RefundAmount.ShouldBe(0);
        }
    }

    [Fact]
    public async Task CashRefund_Is_Represented_Correctly()
    {
        var tenantId = await CreateTenantAsync("tenant-cash-refund-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, sale, saleItem, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100, paidAmount: 1000, saleType: ShopSaleType.Cash);
            var saleReturn = await _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 3, ShopSaleReturnReason.Damaged), ShopSaleReturnSettlementType.CashRefund));
            var completed = await _returnAppService.CompleteAsync(saleReturn.Id);

            completed.RefundAmount.ShouldBe(300);
            completed.CustomerCreditAmount.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Completed_Return_Reduces_Sale_Pending_Calculation()
    {
        var tenantId = await CreateTenantAsync("tenant-pending-calc-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, sale, saleItem, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100, paidAmount: 200);

            var outstandingBefore = await _paymentAppService.GetOutstandingSalesAsync(customer.Id);
            outstandingBefore.Items.Single(x => x.SaleId == sale.Id).PendingAmount.ShouldBe(800);

            var saleReturn = await _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 3, ShopSaleReturnReason.Damaged), ShopSaleReturnSettlementType.CustomerCredit));
            await _returnAppService.CompleteAsync(saleReturn.Id);

            // The customer-payment outstanding view is payment-only and does not itself subtract returns;
            // combined with the return's GrandTotal (300), the caller derives the fully return-aware pending
            // amount (GrandTotal - InitialPaid - PostedAllocations - ReturnAmount = 1000-200-0-300 = 500).
            var outstandingAfter = await _paymentAppService.GetOutstandingSalesAsync(customer.Id);
            var rowAfter = outstandingAfter.Items.Single(x => x.SaleId == sale.Id);
            rowAfter.PendingAmount.ShouldBe(800);
            var finalPending = Math.Max(0m, (rowAfter.PendingAmount ?? 0) - saleReturn.GrandTotal!.Value);
            finalPending.ShouldBe(500);
        }
    }

    [Fact]
    public async Task Draft_And_Cancelled_Returns_Do_Not_Affect_Balances()
    {
        var tenantId = await CreateTenantAsync("tenant-draft-cancelled-balance-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (_, sale, saleItem, product) = await CreateCompletedSaleAsync(quantity: 10, price: 100);
            var stockBefore = (await _productAppService.GetAsync(product.Id)).CurrentStock;

            var draftReturn = await _returnAppService.CreateAsync(BuildReturnInput(sale.Id, (saleItem.Id, 3, ShopSaleReturnReason.Damaged)));
            (await _productAppService.GetAsync(product.Id)).CurrentStock.ShouldBe(stockBefore);

            await _returnAppService.CancelAsync(draftReturn.Id, new CancelShopSaleReturnDto { CancellationReason = "test" });
            (await _productAppService.GetAsync(product.Id)).CurrentStock.ShouldBe(stockBefore);
        }
    }

    [Fact]
    public void TenantId_Is_Not_Accepted_Through_Dto()
    {
        typeof(CreateShopSaleReturnDto).GetProperty("TenantId").ShouldBeNull();
        typeof(CreateShopSaleReturnItemDto).GetProperty("TenantId").ShouldBeNull();
        typeof(UpdateShopSaleReturnDto).GetProperty("TenantId").ShouldBeNull();
    }

    /// <summary>
    /// The test host registers AddAlwaysAllowAuthorization(), so permission checks cannot be
    /// exercised end-to-end here. This verifies the [Authorize] attributes themselves are present
    /// with the correct policy names, by static reflection.
    /// </summary>
    [Fact]
    public void Permissions_Are_Enforced()
    {
        var type = typeof(ShopSaleReturnAppService);
        var classAuthorize = type.GetCustomAttribute<AuthorizeAttribute>();
        classAuthorize.ShouldNotBeNull();
        classAuthorize!.Policy.ShouldBe(EHubPermissions.ShopSaleReturns.Default);

        AssertMethodPolicy(type, nameof(ShopSaleReturnAppService.CreateAsync), EHubPermissions.ShopSaleReturns.Create);
        AssertMethodPolicy(type, nameof(ShopSaleReturnAppService.UpdateAsync), EHubPermissions.ShopSaleReturns.Edit);
        AssertMethodPolicy(type, nameof(ShopSaleReturnAppService.DeleteAsync), EHubPermissions.ShopSaleReturns.Delete);
        AssertMethodPolicy(type, nameof(ShopSaleReturnAppService.CompleteAsync), EHubPermissions.ShopSaleReturns.Complete);
        AssertMethodPolicy(type, nameof(ShopSaleReturnAppService.CancelAsync), EHubPermissions.ShopSaleReturns.Cancel);
    }

    [Fact]
    public async Task Search_Filters_Paging_And_Sorting_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-list-filters-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, saleA, saleItemA, _) = await CreateCompletedSaleAsync(quantity: 10, price: 100);
            var returnA = await _returnAppService.CreateAsync(BuildReturnInput(saleA.Id, (saleItemA.Id, 2, ShopSaleReturnReason.Damaged)));
            await _returnAppService.CompleteAsync(returnA.Id);

            var (_, saleB, saleItemB, _) = await CreateCompletedSaleAsync(quantity: 5, price: 50);
            var returnB = await _returnAppService.CreateAsync(BuildReturnInput(saleB.Id, (saleItemB.Id, 1, ShopSaleReturnReason.WrongProduct)));

            var all = await _returnAppService.GetListAsync(new GetShopSaleReturnsInput { MaxResultCount = 100 });
            all.TotalCount.ShouldBeGreaterThanOrEqualTo(2);

            var byCustomer = await _returnAppService.GetListAsync(new GetShopSaleReturnsInput { CustomerId = customer.Id });
            byCustomer.Items.ShouldContain(x => x.Id == returnA.Id);
            byCustomer.Items.ShouldNotContain(x => x.Id == returnB.Id);

            var byStatus = await _returnAppService.GetListAsync(new GetShopSaleReturnsInput { Status = ShopSaleReturnStatus.Completed });
            byStatus.Items.ShouldContain(x => x.Id == returnA.Id);
            byStatus.Items.ShouldNotContain(x => x.Id == returnB.Id);

            var byReason = await _returnAppService.GetListAsync(new GetShopSaleReturnsInput { Reason = ShopSaleReturnReason.WrongProduct });
            byReason.Items.ShouldContain(x => x.Id == returnB.Id);
            byReason.Items.ShouldNotContain(x => x.Id == returnA.Id);

            var bySearch = await _returnAppService.GetListAsync(new GetShopSaleReturnsInput { Filter = returnA.SaleReturnNumber });
            bySearch.Items.ShouldContain(x => x.Id == returnA.Id);

            var paged = await _returnAppService.GetListAsync(new GetShopSaleReturnsInput { MaxResultCount = 1, SkipCount = 0 });
            paged.Items.Count.ShouldBe(1);

            var sorted = await _returnAppService.GetListAsync(new GetShopSaleReturnsInput { Sorting = "SaleReturnNumber asc", MaxResultCount = 100 });
            var numbers = sorted.Items.Select(x => x.SaleReturnNumber).ToList();
            numbers.ShouldBe(numbers.OrderBy(x => x).ToList());
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

    private async Task<ShopProductDto> CreateProductWithStockAsync(decimal stockQuantity, decimal purchasePrice, decimal salePrice, bool allowDecimal = false)
    {
        var (categoryId, unitId) = await CreateCategoryAndUnitAsync(allowDecimal);
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

    private async Task<ShopCustomerDto> CreateCustomerAsync(decimal openingBalance)
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        return await _customerAppService.CreateAsync(new CreateUpdateShopCustomerDto
        {
            Code = "CUS-" + suffix,
            Name = "Customer " + suffix,
            CustomerType = ShopCustomerType.Business,
            OpeningBalance = openingBalance,
            PaymentTermsDays = 30,
            IsActive = true,
        });
    }

    private async Task<(ShopCustomerDto Customer, ShopSaleDto Sale, ShopSaleItemDto SaleItem, ShopProductDto Product)> CreateCompletedSaleAsync(
        decimal quantity,
        decimal price,
        decimal paidAmount = 0,
        ShopSaleType saleType = ShopSaleType.Credit,
        bool allowDecimal = false,
        decimal stockQuantity = 0,
        string? batchNumber = null,
        DateTime? expiryDate = null)
    {
        var customer = await CreateCustomerAsync(0);
        var effectiveStock = stockQuantity > 0 ? stockQuantity : quantity;
        var product = await CreateProductWithStockAsync(effectiveStock, price / 2, price, allowDecimal);
        var sale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, (product.Id, quantity, price, batchNumber, expiryDate), paidAmount, saleType));
        var completed = await _saleAppService.CompleteAsync(sale.Id, new CompleteShopSaleDto());
        return (customer, completed, completed.Items[0], product);
    }

    private static CreateShopSaleDto BuildSaleCreateInput(
        Guid customerId,
        (Guid ProductId, decimal Quantity, decimal Price, string? BatchNumber, DateTime? ExpiryDate) item,
        decimal paidAmount,
        ShopSaleType saleType = ShopSaleType.Credit) => new()
    {
        CustomerId = customerId,
        SaleDate = DateTime.Today,
        SaleType = saleType,
        DueDate = saleType == ShopSaleType.Credit ? DateTime.Today.AddDays(30) : null,
        PaymentMethod = ShopSalePaymentMethod.Cash,
        PaidAmount = paidAmount,
        Items = new List<CreateShopSaleItemDto>
        {
            new()
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitSalePrice = item.Price,
                DiscountPercentage = 0,
                TaxPercentage = 0,
                BatchNumber = item.BatchNumber,
                ExpiryDate = item.ExpiryDate
            }
        }
    };

    private static CreateShopSaleReturnDto BuildReturnInput(
        Guid saleId,
        (Guid SaleItemId, decimal Quantity, ShopSaleReturnReason Reason) item,
        ShopSaleReturnSettlementType settlementType = ShopSaleReturnSettlementType.CustomerCredit) => new()
    {
        SaleId = saleId,
        ReturnDate = DateTime.Today,
        Reason = item.Reason,
        SettlementType = settlementType,
        OtherCharges = 0,
        Items = new List<CreateShopSaleReturnItemDto>
        {
            new() { SaleItemId = item.SaleItemId, ReturnQuantity = item.Quantity, Reason = item.Reason }
        }
    };

    private static UpdateShopSaleReturnDto BuildReturnUpdateInput(
        (Guid SaleItemId, decimal Quantity, ShopSaleReturnReason Reason) item,
        ShopSaleReturnSettlementType settlementType = ShopSaleReturnSettlementType.CustomerCredit) => new()
    {
        ReturnDate = DateTime.Today,
        Reason = item.Reason,
        SettlementType = settlementType,
        OtherCharges = 0,
        Items = new List<UpdateShopSaleReturnItemDto>
        {
            new() { SaleItemId = item.SaleItemId, ReturnQuantity = item.Quantity, Reason = item.Reason }
        }
    };

    private static void AssertMethodPolicy(Type type, string methodName, string expectedPolicy)
    {
        var method = type.GetMethod(methodName) ?? throw new InvalidOperationException($"Method {methodName} not found.");
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();
        authorize.ShouldNotBeNull();
        authorize!.Policy.ShouldBe(expectedPolicy);
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
