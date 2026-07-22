using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.PurchaseReturns;
using EHub.ShopManagement.SupplierPayments;
using EHub.ShopManagement.Suppliers;
using EHub.ShopManagement.Units;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Modularity;
using Xunit;

namespace EHub.ShopManagement.SupplierLedger;

public abstract class ShopSupplierLedgerAppServiceTests<TStartupModule> : EHubApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShopSupplierLedgerAppService _ledgerAppService;
    private readonly IShopSupplierPaymentAppService _paymentAppService;
    private readonly IShopPurchaseReturnAppService _returnAppService;
    private readonly IShopGoodsReceiptAppService _grAppService;
    private readonly IShopPurchaseOrderAppService _poAppService;
    private readonly IShopSupplierAppService _supplierAppService;
    private readonly IShopProductCategoryAppService _categoryAppService;
    private readonly IShopUnitAppService _unitAppService;
    private readonly IShopProductAppService _productAppService;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly ICurrentTenant _currentTenant;

    protected ShopSupplierLedgerAppServiceTests()
    {
        _ledgerAppService = GetRequiredService<IShopSupplierLedgerAppService>();
        _paymentAppService = GetRequiredService<IShopSupplierPaymentAppService>();
        _returnAppService = GetRequiredService<IShopPurchaseReturnAppService>();
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
    public async Task Opening_Balance_Appears_As_Debit()
    {
        var tenantId = await CreateTenantAsync("tenant-opening-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync(openingBalance: 25000);
            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput { SupplierId = supplier.Id });

            var opening = ledger.Entries.Single(x => x.ReferenceType == ShopSupplierLedgerReferenceType.OpeningBalance);
            opening.DebitAmount.ShouldBe(25000);
            opening.CreditAmount.ShouldBe(0);
            ledger.OpeningBalance.ShouldBe(25000);
            ledger.ClosingBalance.ShouldBe(25000);
        }
    }

    [Fact]
    public async Task Completed_GoodsReceipt_Increases_Balance()
    {
        var tenantId = await CreateTenantAsync("tenant-gr-increase-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, _) = await CreateCompletedGoodsReceiptAsync(openingBalance: 0, quantity: 10, price: 100);

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput { SupplierId = supplier.Id });
            var grEntry = ledger.Entries.Single(x => x.ReferenceType == ShopSupplierLedgerReferenceType.GoodsReceipt);
            grEntry.DebitAmount.ShouldBe(1000);
            grEntry.CreditAmount.ShouldBe(0);
            ledger.ClosingBalance.ShouldBe(1000);
        }
    }

    [Fact]
    public async Task Draft_GoodsReceipt_Is_Ignored()
    {
        var tenantId = await CreateTenantAsync("tenant-gr-draft-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (po, supplier) = await CreateApprovedPurchaseOrderAsync(quantity: 10, price: 100);
            await _grAppService.CreateAsync(BuildGrCreateInput(po, (po.Items[0].Id, 10, 0, 100, 0, 0)));

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput { SupplierId = supplier.Id });
            ledger.Entries.ShouldNotContain(x => x.ReferenceType == ShopSupplierLedgerReferenceType.GoodsReceipt);
            ledger.ClosingBalance.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Posted_Payment_Decreases_Balance()
    {
        var tenantId = await CreateTenantAsync("tenant-payment-posted-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, _) = await CreateCompletedGoodsReceiptAsync(openingBalance: 0, quantity: 10, price: 100);
            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(supplier.Id, 400));
            await _paymentAppService.PostAsync(payment.Id);

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput { SupplierId = supplier.Id });
            var paymentEntry = ledger.Entries.Single(x => x.ReferenceType == ShopSupplierLedgerReferenceType.SupplierPayment);
            paymentEntry.CreditAmount.ShouldBe(400);
            paymentEntry.DebitAmount.ShouldBe(0);
            ledger.ClosingBalance.ShouldBe(600);
        }
    }

    [Fact]
    public async Task Draft_Payment_Is_Ignored()
    {
        var tenantId = await CreateTenantAsync("tenant-payment-draft-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync(openingBalance: 0);
            await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(supplier.Id, 400));

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput { SupplierId = supplier.Id });
            ledger.Entries.ShouldNotContain(x => x.ReferenceType == ShopSupplierLedgerReferenceType.SupplierPayment);
            ledger.ClosingBalance.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Cancelled_Payment_Is_Ignored()
    {
        var tenantId = await CreateTenantAsync("tenant-payment-cancelled-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync(openingBalance: 0);
            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(supplier.Id, 400));
            await _paymentAppService.PostAsync(payment.Id);
            await _paymentAppService.CancelAsync(payment.Id, new CancelShopSupplierPaymentDto { CancellationReason = "test" });

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput { SupplierId = supplier.Id });
            ledger.Entries.ShouldNotContain(x => x.ReferenceType == ShopSupplierLedgerReferenceType.SupplierPayment);
            ledger.ClosingBalance.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Completed_PurchaseReturn_Decreases_Balance()
    {
        var tenantId = await CreateTenantAsync("tenant-return-completed-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(openingBalance: 0, quantity: 10, price: 100);
            var ret = await _returnAppService.CreateAsync(BuildReturnInput(gr.Id, gr.Items[0].Id, 2));
            await _returnAppService.CompleteAsync(ret.Id);

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput { SupplierId = supplier.Id });
            var returnEntry = ledger.Entries.Single(x => x.ReferenceType == ShopSupplierLedgerReferenceType.PurchaseReturn);
            returnEntry.CreditAmount.ShouldBe(200);
            returnEntry.DebitAmount.ShouldBe(0);
            ledger.ClosingBalance.ShouldBe(800);
        }
    }

    [Fact]
    public async Task Draft_PurchaseReturn_Is_Ignored()
    {
        var tenantId = await CreateTenantAsync("tenant-return-draft-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(openingBalance: 0, quantity: 10, price: 100);
            await _returnAppService.CreateAsync(BuildReturnInput(gr.Id, gr.Items[0].Id, 2));

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput { SupplierId = supplier.Id });
            ledger.Entries.ShouldNotContain(x => x.ReferenceType == ShopSupplierLedgerReferenceType.PurchaseReturn);
            ledger.ClosingBalance.ShouldBe(1000);
        }
    }

    [Fact]
    public async Task Advance_Payment_Can_Create_A_Negative_Balance()
    {
        var tenantId = await CreateTenantAsync("tenant-advance-negative-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync(openingBalance: 0);
            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(supplier.Id, 5000));
            await _paymentAppService.PostAsync(payment.Id);

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput { SupplierId = supplier.Id });
            ledger.ClosingBalance.ShouldBe(-5000);
        }
    }

    [Fact]
    public async Task PayableAmount_And_AdvanceAmount_Are_Calculated_Correctly()
    {
        var tenantId = await CreateTenantAsync("tenant-payable-advance-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, _) = await CreateCompletedGoodsReceiptAsync(openingBalance: 0, quantity: 10, price: 100);

            var positiveLedger = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput { SupplierId = supplier.Id });
            positiveLedger.ClosingBalance.ShouldBe(1000);
            positiveLedger.PayableAmount.ShouldBe(1000);
            positiveLedger.AdvanceAmount.ShouldBe(0);

            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(supplier.Id, 3000));
            await _paymentAppService.PostAsync(payment.Id);

            var negativeLedger = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput { SupplierId = supplier.Id });
            negativeLedger.ClosingBalance.ShouldBe(-2000);
            negativeLedger.PayableAmount.ShouldBe(0);
            negativeLedger.AdvanceAmount.ShouldBe(2000);
        }
    }

    [Fact]
    public async Task DateRange_Calculates_BroughtForward_Balance_Correctly()
    {
        var tenantId = await CreateTenantAsync("tenant-brought-forward-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var supplier = await CreateSupplierAsync(openingBalance: 1000);
            var product = await CreateProductAsync();
            var po = await _poAppService.CreateAsync(BuildPoInput(supplier.Id, (product.Id, 10, 100, 0, 0)));
            await _poAppService.SubmitAsync(po.Id);
            var approved = await _poAppService.ApproveAsync(po.Id);

            var earlyInput = BuildGrCreateInput(approved, (approved.Items[0].Id, 10, 0, 100, 0, 0));
            earlyInput.ReceiptDate = DateTime.Today.AddDays(-10);
            var earlyGr = await _grAppService.CreateAsync(earlyInput);
            await _grAppService.CompleteAsync(earlyGr.Id);

            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(supplier.Id, 300));
            await _paymentAppService.PostAsync(payment.Id);

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput
            {
                SupplierId = supplier.Id,
                DateFrom = DateTime.Today.AddDays(-1),
            });

            // Brought-forward should fold in opening balance (1000) + the early GR (1000) - nothing else before DateFrom.
            var broughtForward = ledger.Entries.Single(x => x.ReferenceType == ShopSupplierLedgerReferenceType.OpeningBalance);
            broughtForward.RunningBalance.ShouldBe(2000);
            ledger.OpeningBalance.ShouldBe(2000);

            // Only the payment (dated today) should appear as an individual entry within the period.
            ledger.Entries.ShouldNotContain(x => x.ReferenceType == ShopSupplierLedgerReferenceType.GoodsReceipt);
            ledger.Entries.Single(x => x.ReferenceType == ShopSupplierLedgerReferenceType.SupplierPayment).CreditAmount.ShouldBe(300);
            ledger.ClosingBalance.ShouldBe(1700);
        }
    }

    [Fact]
    public async Task Running_Balance_Is_Correct()
    {
        var tenantId = await CreateTenantAsync("tenant-running-balance-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(openingBalance: 500, quantity: 10, price: 100);

            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(supplier.Id, 200));
            await _paymentAppService.PostAsync(payment.Id);

            var ret = await _returnAppService.CreateAsync(BuildReturnInput(gr.Id, gr.Items[0].Id, 1));
            await _returnAppService.CompleteAsync(ret.Id);

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput { SupplierId = supplier.Id });

            // 500 (opening) -> +1000 (GR) = 1500 -> -200 (payment) = 1300 -> -100 (return) = 1200
            var running = ledger.Entries.Select(x => x.RunningBalance).ToList();
            running[0].ShouldBe(500);
            var last = running[running.Count - 1];
            last.ShouldBe(1200);
            ledger.ClosingBalance.ShouldBe(1200);

            // Running balance must be monotonically consistent with debit/credit deltas.
            decimal expected = 500;
            foreach (var entry in ledger.Entries.Skip(1))
            {
                expected += (entry.DebitAmount ?? 0) - (entry.CreditAmount ?? 0);
                entry.RunningBalance.ShouldBe(expected);
            }
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Access_TenantB_Ledger()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-ledger-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-ledger-" + Guid.NewGuid().ToString("N"));

        Guid supplierId;
        using (_currentTenant.Change(tenantBId))
        {
            var supplier = await CreateSupplierAsync(openingBalance: 1000);
            supplierId = supplier.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput { SupplierId = supplierId }));
            exception.Code.ShouldBe("ShopManagement:SupplierLedgerSupplierNotFound");
        }
    }

    [Fact]
    public async Task Search_And_Reference_Filters_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-filters-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, _) = await CreateCompletedGoodsReceiptAsync(openingBalance: 0, quantity: 10, price: 100);
            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(supplier.Id, 200, referenceNumber: "TXN-UNIQUE-42"));
            await _paymentAppService.PostAsync(payment.Id);

            var byType = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput
            {
                SupplierId = supplier.Id,
                ReferenceType = ShopSupplierLedgerReferenceType.GoodsReceipt,
            });
            byType.Entries.ShouldAllBe(x => x.ReferenceType == ShopSupplierLedgerReferenceType.OpeningBalance || x.ReferenceType == ShopSupplierLedgerReferenceType.GoodsReceipt);

            var bySearch = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput
            {
                SupplierId = supplier.Id,
                Filter = "TXN-UNIQUE-42",
            });
            bySearch.Entries.ShouldContain(x => x.ReferenceType == ShopSupplierLedgerReferenceType.SupplierPayment);
            bySearch.Entries.ShouldNotContain(x => x.ReferenceType == ShopSupplierLedgerReferenceType.GoodsReceipt);

            // Filtering only changes the displayed rows, never the underlying totals.
            byType.ClosingBalance.ShouldBe(bySearch.ClosingBalance);
        }
    }

    [Fact]
    public async Task Supplier_Detail_Summary_Matches_Ledger_Totals()
    {
        var tenantId = await CreateTenantAsync("tenant-summary-matches-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (supplier, gr) = await CreateCompletedGoodsReceiptAsync(openingBalance: 500, quantity: 10, price: 100);

            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(supplier.Id, 300));
            await _paymentAppService.PostAsync(payment.Id);

            var ret = await _returnAppService.CreateAsync(BuildReturnInput(gr.Id, gr.Items[0].Id, 1));
            await _returnAppService.CompleteAsync(ret.Id);

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopSupplierLedgerInput { SupplierId = supplier.Id });
            var summary = await _ledgerAppService.GetBalanceSummaryAsync(supplier.Id);

            summary.CurrentBalance.ShouldBe(ledger.ClosingBalance);
            summary.PayableAmount.ShouldBe(ledger.PayableAmount);
            summary.AdvanceAmount.ShouldBe(ledger.AdvanceAmount);
            summary.OpeningBalance.ShouldBe(500);
            summary.TotalCompletedPurchases.ShouldBe(1000);
            summary.TotalCompletedReturns.ShouldBe(100);
            summary.TotalPostedPayments.ShouldBe(300);
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

    private async Task<ShopSupplierDto> CreateSupplierAsync(decimal openingBalance)
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
        return await _supplierAppService.CreateAsync(new CreateUpdateShopSupplierDto
        { Code = "SUP-" + suffix, Name = "Supplier " + suffix, OpeningBalance = openingBalance, IsActive = true });
    }

    private async Task<(ShopPurchaseOrderDto Po, ShopSupplierDto Supplier)> CreateApprovedPurchaseOrderAsync(decimal quantity, decimal price)
    {
        var supplier = await CreateSupplierAsync(openingBalance: 0);
        var product = await CreateProductAsync();
        var po = await _poAppService.CreateAsync(BuildPoInput(supplier.Id, (product.Id, quantity, price, 0, 0)));
        await _poAppService.SubmitAsync(po.Id);
        var approved = await _poAppService.ApproveAsync(po.Id);
        return (approved, supplier);
    }

    private async Task<(ShopSupplierDto Supplier, ShopGoodsReceiptDto GoodsReceipt)> CreateCompletedGoodsReceiptAsync(decimal openingBalance, decimal quantity, decimal price)
    {
        var supplier = await CreateSupplierAsync(openingBalance);
        var product = await CreateProductAsync();
        var po = await _poAppService.CreateAsync(BuildPoInput(supplier.Id, (product.Id, quantity, price, 0, 0)));
        await _poAppService.SubmitAsync(po.Id);
        var approved = await _poAppService.ApproveAsync(po.Id);
        var gr = await _grAppService.CreateAsync(BuildGrCreateInput(approved, (approved.Items[0].Id, quantity, 0, price, 0, 0)));
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

    private static CreateShopPurchaseReturnDto BuildReturnInput(Guid goodsReceiptId, Guid goodsReceiptItemId, decimal returnQuantity) => new()
    {
        GoodsReceiptId = goodsReceiptId,
        ReturnDate = DateTime.Today,
        Reason = ShopPurchaseReturnReason.Damaged,
        OtherCharges = 0,
        Items = new List<CreateShopPurchaseReturnItemDto>
        {
            new() { GoodsReceiptItemId = goodsReceiptItemId, ReturnQuantity = returnQuantity, Reason = ShopPurchaseReturnReason.Damaged }
        }
    };

    private static CreateUpdateShopSupplierPaymentDto BuildAdvancePaymentInput(Guid supplierId, decimal amount, string? referenceNumber = null) => new()
    {
        SupplierId = supplierId,
        PaymentDate = DateTime.Today,
        PaymentType = ShopSupplierPaymentType.Advance,
        PaymentMethod = ShopSupplierPaymentMethod.Cash,
        Amount = amount,
        ReferenceNumber = referenceNumber,
        Allocations = new List<CreateUpdateShopSupplierPaymentAllocationDto>(),
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
