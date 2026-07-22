using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.StockTransactions;
using EHub.ShopManagement.Suppliers;
using EHub.ShopManagement.Units;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Modularity;
using Xunit;

namespace EHub.ShopManagement.GoodsReceipts;

public abstract class ShopGoodsReceiptAppServiceTests<TStartupModule> : EHubApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShopGoodsReceiptAppService _grAppService;
    private readonly IShopPurchaseOrderAppService _poAppService;
    private readonly IShopSupplierAppService _supplierAppService;
    private readonly IShopProductCategoryAppService _categoryAppService;
    private readonly IShopUnitAppService _unitAppService;
    private readonly IShopProductAppService _productAppService;
    private readonly IShopStockTransactionAppService _stockTransactionAppService;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly IRepository<ShopGoodsReceipt, Guid> _grRepository;
    private readonly ICurrentTenant _currentTenant;

    protected ShopGoodsReceiptAppServiceTests()
    {
        _grAppService = GetRequiredService<IShopGoodsReceiptAppService>();
        _poAppService = GetRequiredService<IShopPurchaseOrderAppService>();
        _supplierAppService = GetRequiredService<IShopSupplierAppService>();
        _categoryAppService = GetRequiredService<IShopProductCategoryAppService>();
        _unitAppService = GetRequiredService<IShopUnitAppService>();
        _productAppService = GetRequiredService<IShopProductAppService>();
        _stockTransactionAppService = GetRequiredService<IShopStockTransactionAppService>();
        _tenantManager = GetRequiredService<ITenantManager>();
        _tenantRepository = GetRequiredService<IRepository<Tenant, Guid>>();
        _grRepository = GetRequiredService<IRepository<ShopGoodsReceipt, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task TenantA_Can_Create_A_Draft_Goods_Receipt()
    {
        var tenantId = await CreateTenantAsync("tenant-create-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0)));

            gr.Id.ShouldNotBe(Guid.Empty);
            gr.GoodsReceiptNumber.ShouldStartWith("GRN-");
            gr.Status.ShouldBe(ShopGoodsReceiptStatus.Draft);
            gr.Items.Single().ReceivedQuantity.ShouldBe(5);
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Access_TenantB_Goods_Receipt()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid grId;
        using (_currentTenant.Change(tenantBId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0)));
            grId = gr.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            await Should.ThrowAsync<BusinessException>(() => _grAppService.GetAsync(grId));
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Use_TenantB_Purchase_Order()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid otherPoId;
        using (_currentTenant.Change(tenantBId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            otherPoId = po.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var input = new CreateShopGoodsReceiptDto
            {
                PurchaseOrderId = otherPoId,
                ReceiptDate = DateTime.Today,
                Items = new List<CreateShopGoodsReceiptItemDto> { new() { PurchaseOrderItemId = Guid.NewGuid(), ReceivedQuantity = 1, PurchasePrice = 10 } }
            };
            var exception = await Should.ThrowAsync<BusinessException>(() => _grAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptPurchaseOrderNotFound");
        }
    }

    [Fact]
    public async Task Draft_PurchaseOrder_Cannot_Be_Received()
    {
        var tenantId = await CreateTenantAsync("tenant-draft-po-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync();
            var product = await CreateProductAsync(allowDecimal: false);
            var po = await _poAppService.CreateAsync(BuildPoInput(supplier.Id, (product.Id, 10, 100, 0, 0)));

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptInvalidPurchaseOrderStatus");
        }
    }

    [Fact]
    public async Task PendingApproval_PurchaseOrder_Cannot_Be_Received()
    {
        var tenantId = await CreateTenantAsync("tenant-pending-po-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync();
            var product = await CreateProductAsync(allowDecimal: false);
            var po = await _poAppService.CreateAsync(BuildPoInput(supplier.Id, (product.Id, 10, 100, 0, 0)));
            await _poAppService.SubmitAsync(po.Id);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptInvalidPurchaseOrderStatus");
        }
    }

    [Fact]
    public async Task Approved_PurchaseOrder_Can_Be_Received()
    {
        var tenantId = await CreateTenantAsync("tenant-approved-po-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0)));
            gr.Status.ShouldBe(ShopGoodsReceiptStatus.Draft);
        }
    }

    [Fact]
    public async Task PartiallyReceived_PurchaseOrder_Can_Be_Received()
    {
        var tenantId = await CreateTenantAsync("tenant-partial-po-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var firstGr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 4, 0, 100, 0, 0)));
            await _grAppService.CompleteAsync(firstGr.Id);

            var updatedPo = await _poAppService.GetAsync(po.Id);
            updatedPo.Status.ShouldBe(ShopPurchaseOrderStatus.PartiallyReceived);

            var secondGr = await _grAppService.CreateAsync(BuildCreateInput(updatedPo, (updatedPo.Items[0].Id, 3, 0, 100, 0, 0)));
            secondGr.Status.ShouldBe(ShopGoodsReceiptStatus.Draft);
        }
    }

    [Fact]
    public async Task FullyReceived_PurchaseOrder_Cannot_Be_Received()
    {
        var tenantId = await CreateTenantAsync("tenant-full-po-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 10, 0, 100, 0, 0)));
            await _grAppService.CompleteAsync(gr.Id);

            var updatedPo = await _poAppService.GetAsync(po.Id);
            updatedPo.Status.ShouldBe(ShopPurchaseOrderStatus.FullyReceived);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _grAppService.CreateAsync(BuildCreateInput(updatedPo, (updatedPo.Items[0].Id, 1, 0, 100, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptInvalidPurchaseOrderStatus");
        }
    }

    [Fact]
    public async Task Cancelled_PurchaseOrder_Cannot_Be_Received()
    {
        var tenantId = await CreateTenantAsync("tenant-cancelled-po-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            await _poAppService.CancelAsync(po.Id, new CancelShopPurchaseOrderDto { CancellationReason = "test" });

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 1, 0, 100, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptInvalidPurchaseOrderStatus");
        }
    }

    [Fact]
    public async Task Supplier_Is_Derived_From_Purchase_Order()
    {
        var tenantId = await CreateTenantAsync("tenant-derive-sup-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, supplier) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0)));
            gr.SupplierId.ShouldBe(supplier.Id);
        }
    }

    [Fact]
    public async Task Goods_Receipt_Requires_At_Least_One_Item()
    {
        var tenantId = await CreateTenantAsync("tenant-no-items-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var input = new CreateShopGoodsReceiptDto { PurchaseOrderId = po.Id, ReceiptDate = DateTime.Today, Items = new List<CreateShopGoodsReceiptItemDto>() };
            await Should.ThrowAsync<Exception>(() => _grAppService.CreateAsync(input));
        }
    }

    [Fact]
    public async Task Purchase_Order_Item_Must_Belong_To_Purchase_Order()
    {
        var tenantId = await CreateTenantAsync("tenant-mismatched-item-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po1, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var (po2, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _grAppService.CreateAsync(BuildCreateInput(po1, (po2.Items[0].Id, 5, 0, 100, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptPurchaseOrderItemNotFound");
        }
    }

    [Fact]
    public async Task ReceivedQuantity_Must_Be_Greater_Than_Zero()
    {
        var tenantId = await CreateTenantAsync("tenant-zero-qty-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 0, 0, 100, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptInvalidQuantity");
        }
    }

    [Fact]
    public async Task BonusQuantity_Cannot_Be_Negative()
    {
        var tenantId = await CreateTenantAsync("tenant-neg-bonus-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, -1, 100, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptInvalidBonusQuantity");
        }
    }

    [Fact]
    public async Task WholeNumber_Unit_Rejects_Decimal_ReceivedQuantity()
    {
        var tenantId = await CreateTenantAsync("tenant-whole-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 2.5m, 0, 100, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptWholeQuantityRequired");
        }
    }

    [Fact]
    public async Task Decimal_Unit_Accepts_Decimal_ReceivedQuantity()
    {
        var tenantId = await CreateTenantAsync("tenant-decimal-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: true, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 2.5m, 0, 100, 0, 0)));
            gr.Items.Single().ReceivedQuantity.ShouldBe(2.5m);
        }
    }

    [Fact]
    public async Task ReceivedQuantity_Cannot_Exceed_RemainingQuantity()
    {
        var tenantId = await CreateTenantAsync("tenant-exceeds-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 11, 0, 100, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptQuantityExceedsRemaining");
        }
    }

    [Fact]
    public async Task BonusQuantity_Does_Not_Reduce_Remaining_PO_Quantity()
    {
        var tenantId = await CreateTenantAsync("tenant-bonus-remaining-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 10, 5, 100, 0, 0)));
            await _grAppService.CompleteAsync(gr.Id);

            var updatedPo = await _poAppService.GetAsync(po.Id);
            updatedPo.Items.Single().ReceivedQuantity.ShouldBe(10);
            updatedPo.Status.ShouldBe(ShopPurchaseOrderStatus.FullyReceived);
        }
    }

    [Fact]
    public async Task TrackBatch_Product_Requires_BatchNumber()
    {
        var tenantId = await CreateTenantAsync("tenant-batch-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10, trackBatch: true);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptBatchNumberRequired");
        }
    }

    [Fact]
    public async Task TrackExpiry_Product_Requires_ExpiryDate()
    {
        var tenantId = await CreateTenantAsync("tenant-expiry-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10, trackExpiry: true);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptExpiryDateRequired");
        }
    }

    [Fact]
    public async Task Invalid_ExpiryDate_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-invalid-expiry-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10, trackExpiry: true);
            var input = BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0));
            input.Items[0].ExpiryDate = DateTime.Today.AddDays(-1);
            var exception = await Should.ThrowAsync<BusinessException>(() => _grAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptInvalidExpiryDate");
        }
    }

    [Fact]
    public async Task ManufacturingDate_After_ExpiryDate_Is_Rejected()
    {
        var tenantId = await CreateTenantAsync("tenant-mfg-after-exp-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10, trackExpiry: true);
            var input = BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0));
            input.Items[0].ExpiryDate = DateTime.Today.AddMonths(6);
            input.Items[0].ManufacturingDate = DateTime.Today.AddMonths(7);
            var exception = await Should.ThrowAsync<BusinessException>(() => _grAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptInvalidManufacturingDate");
        }
    }

    [Fact]
    public async Task Serial_Tracked_Product_Is_Blocked()
    {
        var tenantId = await CreateTenantAsync("tenant-serial-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10, trackSerialNumber: true);
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0))));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptSerialTrackingNotSupported");
        }
    }

    [Fact]
    public async Task Totals_Are_Recalculated_On_Server()
    {
        var tenantId = await CreateTenantAsync("tenant-totals-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: true, quantity: 100);
            var input = BuildCreateInput(po, (po.Items[0].Id, 100, 0, 180, 0, 0));
            input.ShippingCharges = 1500;
            input.OtherCharges = 500;

            var gr = await _grAppService.CreateAsync(input);
            gr.SubTotal.ShouldBe(18000m);
            gr.GrandTotal.ShouldBe(20000m);
        }
    }

    [Fact]
    public void Frontend_Provided_Totals_Are_Ignored()
    {
        typeof(CreateShopGoodsReceiptDto).GetProperty("SubTotal").ShouldBeNull();
        typeof(CreateShopGoodsReceiptDto).GetProperty("GrandTotal").ShouldBeNull();
        typeof(CreateShopGoodsReceiptItemDto).GetProperty("LineTotal").ShouldBeNull();
        typeof(CreateShopGoodsReceiptItemDto).GetProperty("ProductId").ShouldBeNull();
    }

    [Fact]
    public void TenantId_Is_Not_Accepted_Through_Dto()
    {
        typeof(CreateShopGoodsReceiptDto).GetProperty("TenantId").ShouldBeNull();
        typeof(CreateShopGoodsReceiptDto).GetProperty("SupplierId").ShouldBeNull();
        typeof(CreateShopGoodsReceiptItemDto).GetProperty("PreviouslyReceivedQuantity").ShouldBeNull();
    }

    [Fact]
    public async Task Draft_Goods_Receipt_Does_Not_Update_Stock_Or_PurchaseOrder()
    {
        var tenantId = await CreateTenantAsync("tenant-draft-no-stock-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var productId = po.Items[0].ProductId;
            var stockBefore = (await _productAppService.GetAsync(productId)).CurrentStock;

            await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0)));

            (await _productAppService.GetAsync(productId)).CurrentStock.ShouldBe(stockBefore);
            (await _poAppService.GetAsync(po.Id)).Items.Single().ReceivedQuantity.ShouldBe(0);
            (await _poAppService.GetAsync(po.Id)).Status.ShouldBe(ShopPurchaseOrderStatus.Approved);
        }
    }

    [Fact]
    public async Task Completing_Receipt_Increases_Product_Stock_Including_Bonus()
    {
        var tenantId = await CreateTenantAsync("tenant-complete-stock-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var productId = po.Items[0].ProductId;
            var stockBefore = (await _productAppService.GetAsync(productId)).CurrentStock;

            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 6, 2, 100, 0, 0)));
            await _grAppService.CompleteAsync(gr.Id);

            (await _productAppService.GetAsync(productId)).CurrentStock.ShouldBe(stockBefore + 8);
        }
    }

    [Fact]
    public async Task Completing_Receipt_Updates_PurchaseOrderItem_ReceivedQuantity()
    {
        var tenantId = await CreateTenantAsync("tenant-po-received-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 6, 2, 100, 0, 0)));
            await _grAppService.CompleteAsync(gr.Id);

            var updatedPo = await _poAppService.GetAsync(po.Id);
            updatedPo.Items.Single().ReceivedQuantity.ShouldBe(6);
        }
    }

    [Fact]
    public async Task Partial_Receiving_Sets_PO_Status_To_PartiallyReceived()
    {
        var tenantId = await CreateTenantAsync("tenant-partial-status-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0)));
            await _grAppService.CompleteAsync(gr.Id);

            (await _poAppService.GetAsync(po.Id)).Status.ShouldBe(ShopPurchaseOrderStatus.PartiallyReceived);
        }
    }

    [Fact]
    public async Task Full_Receiving_Sets_PO_Status_To_FullyReceived()
    {
        var tenantId = await CreateTenantAsync("tenant-full-status-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 10, 0, 100, 0, 0)));
            await _grAppService.CompleteAsync(gr.Id);

            (await _poAppService.GetAsync(po.Id)).Status.ShouldBe(ShopPurchaseOrderStatus.FullyReceived);
        }
    }

    [Fact]
    public async Task Completing_Receipt_Creates_One_Stock_Transaction_Per_Item_With_Correct_Values()
    {
        var tenantId = await CreateTenantAsync("tenant-stock-txn-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var productId = po.Items[0].ProductId;
            var stockBefore = (await _productAppService.GetAsync(productId)).CurrentStock;

            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 6, 2, 100, 0, 0)));
            await _grAppService.CompleteAsync(gr.Id);

            var transactions = await _stockTransactionAppService.GetListAsync(new GetShopStockTransactionsInput { ProductId = productId });
            transactions.TotalCount.ShouldBe(1);
            var transaction = transactions.Items.Single();
            transaction.QuantityIn.ShouldBe(8);
            transaction.QuantityOut.ShouldBe(0);
            transaction.BalanceQuantity.ShouldBe(stockBefore + 8);
            transaction.UnitCost.ShouldBe(100);
            transaction.TransactionType.ShouldBe(ShopStockTransactionType.Purchase);
            transaction.ReferenceType.ShouldBe(ShopStockReferenceType.GoodsReceipt);
            transaction.ReferenceNumber.ShouldBe(gr.GoodsReceiptNumber);
        }
    }

    [Fact]
    public async Task Completing_Twice_Is_Prevented()
    {
        var tenantId = await CreateTenantAsync("tenant-complete-twice-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0)));
            await _grAppService.CompleteAsync(gr.Id);

            var exception = await Should.ThrowAsync<BusinessException>(() => _grAppService.CompleteAsync(gr.Id));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptAlreadyCompleted");
        }
    }

    [Fact]
    public async Task Completed_Goods_Receipt_Cannot_Be_Edited_Or_Deleted()
    {
        var tenantId = await CreateTenantAsync("tenant-completed-immutable-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0)));
            await _grAppService.CompleteAsync(gr.Id);

            var updateInput = BuildUpdateInput(po, (po.Items[0].Id, 3, 0, 100, 0, 0));
            var editException = await Should.ThrowAsync<BusinessException>(() => _grAppService.UpdateAsync(gr.Id, updateInput));
            editException.Code.ShouldBe("ShopManagement:GoodsReceiptCannotBeEdited");

            var deleteException = await Should.ThrowAsync<BusinessException>(() => _grAppService.DeleteAsync(gr.Id));
            deleteException.Code.ShouldBe("ShopManagement:GoodsReceiptCannotBeDeleted");
        }
    }

    [Fact]
    public async Task Draft_Goods_Receipt_Can_Be_Edited_And_Deleted()
    {
        var tenantId = await CreateTenantAsync("tenant-draft-edit-delete-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0)));

            var updateInput = BuildUpdateInput(po, (po.Items[0].Id, 3, 0, 100, 0, 0));
            var updated = await _grAppService.UpdateAsync(gr.Id, updateInput);
            updated.Items.Single().ReceivedQuantity.ShouldBe(3);

            await _grAppService.DeleteAsync(gr.Id);
            await Should.ThrowAsync<BusinessException>(() => _grAppService.GetAsync(gr.Id));
        }
    }

    [Fact]
    public async Task Draft_Goods_Receipt_Can_Be_Cancelled()
    {
        var tenantId = await CreateTenantAsync("tenant-cancel-draft-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0)));

            var cancelled = await _grAppService.CancelAsync(gr.Id, new CancelShopGoodsReceiptDto { CancellationReason = "Wrong PO" });
            cancelled.Status.ShouldBe(ShopGoodsReceiptStatus.Cancelled);
            cancelled.CancelledByUserId.ShouldNotBeNull();
            cancelled.CancellationReason.ShouldBe("Wrong PO");
        }
    }

    [Fact]
    public async Task Completed_Goods_Receipt_Cannot_Be_Cancelled()
    {
        var tenantId = await CreateTenantAsync("tenant-cancel-completed-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0)));
            await _grAppService.CompleteAsync(gr.Id);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _grAppService.CancelAsync(gr.Id, new CancelShopGoodsReceiptDto { CancellationReason = "test" }));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptCannotBeCancelled");
        }
    }

    [Fact]
    public async Task GoodsReceiptNumber_Is_Server_Generated_And_Unique_Per_Tenant()
    {
        var tenantId = await CreateTenantAsync("tenant-numbering-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var first = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 3, 0, 100, 0, 0)));
            await _grAppService.CompleteAsync(first.Id);

            var updatedPo = await _poAppService.GetAsync(po.Id);
            var second = await _grAppService.CreateAsync(BuildCreateInput(updatedPo, (updatedPo.Items[0].Id, 3, 0, 100, 0, 0)));

            first.GoodsReceiptNumber.ShouldBe("GRN-000001");
            second.GoodsReceiptNumber.ShouldBe("GRN-000002");
        }
    }

    [Fact]
    public async Task Host_Context_Cannot_Create_A_Goods_Receipt()
    {
        using (_currentTenant.Change(null))
        {
            var input = new CreateShopGoodsReceiptDto
            {
                PurchaseOrderId = Guid.NewGuid(),
                ReceiptDate = DateTime.Today,
                Items = new List<CreateShopGoodsReceiptItemDto> { new() { PurchaseOrderItemId = Guid.NewGuid(), ReceivedQuantity = 1, PurchasePrice = 10 } }
            };
            var exception = await Should.ThrowAsync<BusinessException>(() => _grAppService.CreateAsync(input));
            exception.Code.ShouldBe("ShopManagement:TenantRequired");
        }
    }

    [Fact]
    public async Task Search_And_Filters_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-filters-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, supplier) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var input = BuildCreateInput(po, (po.Items[0].Id, 5, 0, 100, 0, 0));
            input.ReceiptDate = new DateTime(2026, 1, 10);
            var gr = await _grAppService.CreateAsync(input);

            (await _grAppService.GetListAsync(new GetShopGoodsReceiptsInput { Filter = gr.GoodsReceiptNumber })).TotalCount.ShouldBe(1);
            (await _grAppService.GetListAsync(new GetShopGoodsReceiptsInput { SupplierId = supplier.Id })).TotalCount.ShouldBe(1);
            (await _grAppService.GetListAsync(new GetShopGoodsReceiptsInput { PurchaseOrderId = po.Id })).TotalCount.ShouldBe(1);
            (await _grAppService.GetListAsync(new GetShopGoodsReceiptsInput { Status = ShopGoodsReceiptStatus.Draft })).TotalCount.ShouldBe(1);
            (await _grAppService.GetListAsync(new GetShopGoodsReceiptsInput { ReceiptDateFrom = new DateTime(2026, 1, 1) })).TotalCount.ShouldBe(1);
            (await _grAppService.GetListAsync(new GetShopGoodsReceiptsInput { ReceiptDateTo = new DateTime(2025, 12, 31) })).TotalCount.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Paging_And_Default_Sorting_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-paging-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var input1 = BuildCreateInput(po, (po.Items[0].Id, 3, 0, 100, 0, 0));
            input1.ReceiptDate = new DateTime(2026, 1, 1);
            await _grAppService.CreateAsync(input1);

            var updatedPo = await _poAppService.GetAsync(po.Id);
            var input2 = BuildCreateInput(updatedPo, (updatedPo.Items[0].Id, 3, 0, 100, 0, 0));
            input2.ReceiptDate = new DateTime(2026, 3, 1);
            await _grAppService.CreateAsync(input2);

            var page1 = await _grAppService.GetListAsync(new GetShopGoodsReceiptsInput { MaxResultCount = 1, SkipCount = 0 });
            page1.Items.Single().ReceiptDate.ShouldBe(new DateTime(2026, 3, 1));
            page1.TotalCount.ShouldBe(2);
        }
    }

    [Fact]
    public async Task GetPurchaseOrderForReceiving_Returns_Only_Items_With_Remaining_Quantity()
    {
        var tenantId = await CreateTenantAsync("tenant-receiving-dto-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, _) = await CreateApprovedPurchaseOrderAsync(allowDecimal: false, quantity: 10);
            var gr = await _grAppService.CreateAsync(BuildCreateInput(po, (po.Items[0].Id, 10, 0, 100, 0, 0)));
            await _grAppService.CompleteAsync(gr.Id);

            var updatedPo = await _poAppService.GetAsync(po.Id);
            var exception = await Should.ThrowAsync<BusinessException>(() => _grAppService.GetPurchaseOrderForReceivingAsync(updatedPo.Id));
            exception.Code.ShouldBe("ShopManagement:GoodsReceiptInvalidPurchaseOrderStatus");
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

    private async Task<ShopProductDto> CreateProductAsync(bool allowDecimal, bool trackBatch = false, bool trackExpiry = false, bool trackSerialNumber = false)
    {
        var (categoryId, unitId) = await CreateCategoryAndUnitAsync(allowDecimal);
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        return await _productAppService.CreateAsync(new CreateShopProductDto
        {
            CategoryId = categoryId,
            UnitId = unitId,
            Name = "Product " + suffix,
            Code = "PRD-" + suffix,
            PurchasePrice = 10,
            SalePrice = 20,
            TrackBatch = trackBatch,
            TrackExpiry = trackExpiry,
            TrackSerialNumber = trackSerialNumber,
            IsActive = true,
        });
    }

    private async Task<ShopSupplierDto> CreateSupplierAsync()
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
        return await _supplierAppService.CreateAsync(new CreateUpdateShopSupplierDto
        { Code = "SUP-" + suffix, Name = "Supplier " + suffix, IsActive = true });
    }

    private async Task<(ShopPurchaseOrderDto Po, ShopSupplierDto Supplier)> CreateApprovedPurchaseOrderAsync(
        bool allowDecimal, decimal quantity, bool trackBatch = false, bool trackExpiry = false, bool trackSerialNumber = false)
    {
        var supplier = await CreateSupplierAsync();
        var product = await CreateProductAsync(allowDecimal, trackBatch, trackExpiry, trackSerialNumber);
        var po = await _poAppService.CreateAsync(BuildPoInput(supplier.Id, (product.Id, quantity, 100, 0, 0)));
        await _poAppService.SubmitAsync(po.Id);
        var approved = await _poAppService.ApproveAsync(po.Id);
        return (approved, supplier);
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

    private static CreateShopGoodsReceiptDto BuildCreateInput(ShopPurchaseOrderDto po, (Guid PurchaseOrderItemId, decimal ReceivedQuantity, decimal BonusQuantity, decimal PurchasePrice, decimal Discount, decimal Tax) item) => new()
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

    private static UpdateShopGoodsReceiptDto BuildUpdateInput(ShopPurchaseOrderDto po, (Guid PurchaseOrderItemId, decimal ReceivedQuantity, decimal BonusQuantity, decimal PurchasePrice, decimal Discount, decimal Tax) item) => new()
    {
        PurchaseOrderId = po.Id,
        ReceiptDate = DateTime.Today,
        Items = new List<UpdateShopGoodsReceiptItemDto>
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
