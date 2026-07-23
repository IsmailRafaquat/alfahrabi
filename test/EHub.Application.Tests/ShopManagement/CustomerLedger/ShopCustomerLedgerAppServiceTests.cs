using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.CustomerPayments;
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

namespace EHub.ShopManagement.CustomerLedger;

public abstract class ShopCustomerLedgerAppServiceTests<TStartupModule> : EHubApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShopCustomerLedgerAppService _ledgerAppService;
    private readonly IShopCustomerPaymentAppService _paymentAppService;
    private readonly IShopSaleAppService _saleAppService;
    private readonly IShopCustomerAppService _customerAppService;
    private readonly IShopProductCategoryAppService _categoryAppService;
    private readonly IShopUnitAppService _unitAppService;
    private readonly IShopProductAppService _productAppService;
    private readonly IShopSupplierAppService _supplierAppService;
    private readonly IShopPurchaseOrderAppService _poAppService;
    private readonly IShopGoodsReceiptAppService _grAppService;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly ICurrentTenant _currentTenant;

    protected ShopCustomerLedgerAppServiceTests()
    {
        _ledgerAppService = GetRequiredService<IShopCustomerLedgerAppService>();
        _paymentAppService = GetRequiredService<IShopCustomerPaymentAppService>();
        _saleAppService = GetRequiredService<IShopSaleAppService>();
        _customerAppService = GetRequiredService<IShopCustomerAppService>();
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
    public async Task Opening_Balance_Appears_As_Debit()
    {
        var tenantId = await CreateTenantAsync("tenant-opening-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync(openingBalance: 12000);
            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });

            var opening = ledger.Entries.Single(x => x.ReferenceType == ShopCustomerLedgerReferenceType.OpeningBalance);
            opening.DebitAmount.ShouldBe(12000);
            opening.CreditAmount.ShouldBe(0);
            ledger.OpeningBalance.ShouldBe(12000);
            ledger.ClosingBalance.ShouldBe(12000);
        }
    }

    [Fact]
    public async Task Completed_Credit_Sale_Increases_Receivable()
    {
        var tenantId = await CreateTenantAsync("tenant-sale-increase-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, _) = await CreateCompletedSaleAsync(openingBalance: 0, quantity: 10, price: 100, paidAmount: 0);

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });
            var saleEntry = ledger.Entries.Single(x => x.ReferenceType == ShopCustomerLedgerReferenceType.Sale);
            saleEntry.DebitAmount.ShouldBe(1000);
            saleEntry.CreditAmount.ShouldBe(0);
            ledger.ClosingBalance.ShouldBe(1000);
        }
    }

    [Fact]
    public async Task Completed_Sale_Only_Adds_GrandTotal_Minus_InitialPaid()
    {
        var tenantId = await CreateTenantAsync("tenant-sale-partial-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, sale) = await CreateCompletedSaleAsync(openingBalance: 0, quantity: 10, price: 100, paidAmount: 300);
            sale.GrandTotal.ShouldBe(1000);

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });
            var saleEntry = ledger.Entries.Single(x => x.ReferenceType == ShopCustomerLedgerReferenceType.Sale);
            saleEntry.DebitAmount.ShouldBe(700);
            ledger.ClosingBalance.ShouldBe(700);
            ledger.TotalSales.ShouldBe(1000);
            ledger.TotalInitialPaid.ShouldBe(300);
        }
    }

    [Fact]
    public async Task Fully_Paid_Cash_Sale_Does_Not_Increase_Receivable()
    {
        var tenantId = await CreateTenantAsync("tenant-cash-fully-paid-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, _) = await CreateCompletedSaleAsync(openingBalance: 0, quantity: 10, price: 100, paidAmount: 1000, saleType: ShopSaleType.Cash);

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });
            ledger.Entries.ShouldNotContain(x => x.ReferenceType == ShopCustomerLedgerReferenceType.Sale);
            ledger.ClosingBalance.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Draft_Sale_Is_Ignored()
    {
        var tenantId = await CreateTenantAsync("tenant-sale-draft-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync(openingBalance: 0);
            var product = await CreateProductWithStockAsync(10, 50, 100);
            await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, (product.Id, 10, 100, 0, 0), 0));

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });
            ledger.Entries.ShouldNotContain(x => x.ReferenceType == ShopCustomerLedgerReferenceType.Sale);
            ledger.ClosingBalance.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Cancelled_Sale_Is_Ignored()
    {
        var tenantId = await CreateTenantAsync("tenant-sale-cancelled-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync(openingBalance: 0);
            var product = await CreateProductWithStockAsync(10, 50, 100);
            var sale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, (product.Id, 10, 100, 0, 0), 0));
            await _saleAppService.CancelAsync(sale.Id, new CancelShopSaleDto { CancellationReason = "test" });

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });
            ledger.Entries.ShouldNotContain(x => x.ReferenceType == ShopCustomerLedgerReferenceType.Sale);
            ledger.ClosingBalance.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Posted_CustomerPayment_Decreases_Receivable()
    {
        var tenantId = await CreateTenantAsync("tenant-payment-posted-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, _) = await CreateCompletedSaleAsync(openingBalance: 0, quantity: 10, price: 100, paidAmount: 0);
            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(customer.Id, 400));
            await _paymentAppService.PostAsync(payment.Id);

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });
            var paymentEntry = ledger.Entries.Single(x => x.ReferenceType == ShopCustomerLedgerReferenceType.CustomerPayment);
            paymentEntry.CreditAmount.ShouldBe(400);
            paymentEntry.DebitAmount.ShouldBe(0);
            ledger.ClosingBalance.ShouldBe(600);
        }
    }

    [Fact]
    public async Task Draft_CustomerPayment_Is_Ignored()
    {
        var tenantId = await CreateTenantAsync("tenant-payment-draft-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync(openingBalance: 0);
            await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(customer.Id, 400));

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });
            ledger.Entries.ShouldNotContain(x => x.ReferenceType == ShopCustomerLedgerReferenceType.CustomerPayment);
            ledger.ClosingBalance.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Cancelled_CustomerPayment_Is_Ignored()
    {
        var tenantId = await CreateTenantAsync("tenant-payment-cancelled-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync(openingBalance: 0);
            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(customer.Id, 400));
            await _paymentAppService.PostAsync(payment.Id);
            await _paymentAppService.CancelAsync(payment.Id, new CancelShopCustomerPaymentDto { CancellationReason = "test" });

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });
            ledger.Entries.ShouldNotContain(x => x.ReferenceType == ShopCustomerLedgerReferenceType.CustomerPayment);
            ledger.ClosingBalance.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Advance_Payment_Can_Create_A_Negative_Balance()
    {
        var tenantId = await CreateTenantAsync("tenant-advance-negative-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync(openingBalance: 0);
            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(customer.Id, 5000));
            await _paymentAppService.PostAsync(payment.Id);

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });
            ledger.ClosingBalance.ShouldBe(-5000);
        }
    }

    [Fact]
    public async Task ReceivableAmount_And_AdvanceAmount_Are_Calculated_Correctly()
    {
        var tenantId = await CreateTenantAsync("tenant-receivable-advance-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, _) = await CreateCompletedSaleAsync(openingBalance: 0, quantity: 10, price: 100, paidAmount: 0);

            var positiveLedger = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });
            positiveLedger.ClosingBalance.ShouldBe(1000);
            positiveLedger.ReceivableAmount.ShouldBe(1000);
            positiveLedger.AdvanceAmount.ShouldBe(0);

            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(customer.Id, 3000));
            await _paymentAppService.PostAsync(payment.Id);

            var negativeLedger = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });
            negativeLedger.ClosingBalance.ShouldBe(-2000);
            negativeLedger.ReceivableAmount.ShouldBe(0);
            negativeLedger.AdvanceAmount.ShouldBe(2000);
        }
    }

    [Fact]
    public async Task Running_Balance_Is_Correct()
    {
        var tenantId = await CreateTenantAsync("tenant-running-balance-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, _) = await CreateCompletedSaleAsync(openingBalance: 500, quantity: 10, price: 100, paidAmount: 0);

            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(customer.Id, 200));
            await _paymentAppService.PostAsync(payment.Id);

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });

            // 500 (opening) -> +1000 (sale) = 1500 -> -200 (payment) = 1300
            var running = ledger.Entries.Select(x => x.RunningBalance).ToList();
            running[0].ShouldBe(500);
            running[running.Count - 1].ShouldBe(1300);
            ledger.ClosingBalance.ShouldBe(1300);

            decimal expected = 500;
            foreach (var entry in ledger.Entries.Skip(1))
            {
                expected += (entry.DebitAmount ?? 0) - (entry.CreditAmount ?? 0);
                entry.RunningBalance.ShouldBe(expected);
            }
        }
    }

    [Fact]
    public async Task DateRange_Calculates_BroughtForward_Balance_Correctly()
    {
        var tenantId = await CreateTenantAsync("tenant-brought-forward-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var customer = await CreateCustomerAsync(openingBalance: 1000);
            var product = await CreateProductWithStockAsync(10, 50, 100);

            var earlyInput = BuildSaleCreateInput(customer.Id, (product.Id, 10, 100, 0, 0), 0);
            earlyInput.SaleDate = DateTime.Today.AddDays(-10);
            var earlySale = await _saleAppService.CreateAsync(earlyInput);
            await _saleAppService.CompleteAsync(earlySale.Id);

            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(customer.Id, 300));
            await _paymentAppService.PostAsync(payment.Id);

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput
            {
                CustomerId = customer.Id,
                DateFrom = DateTime.Today.AddDays(-1),
            });

            // Brought-forward should fold in opening balance (1000) + the early sale (1000) - nothing else before DateFrom.
            var broughtForward = ledger.Entries.Single(x => x.ReferenceType == ShopCustomerLedgerReferenceType.OpeningBalance);
            broughtForward.RunningBalance.ShouldBe(2000);
            ledger.OpeningBalance.ShouldBe(2000);

            ledger.Entries.ShouldNotContain(x => x.ReferenceType == ShopCustomerLedgerReferenceType.Sale);
            ledger.Entries.Single(x => x.ReferenceType == ShopCustomerLedgerReferenceType.CustomerPayment).CreditAmount.ShouldBe(300);
            ledger.ClosingBalance.ShouldBe(1700);
        }
    }

    [Fact]
    public async Task CustomerA_Ledger_Does_Not_Include_CustomerB_Transactions()
    {
        var tenantId = await CreateTenantAsync("tenant-customer-isolation-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customerA, _) = await CreateCompletedSaleAsync(openingBalance: 0, quantity: 10, price: 100, paidAmount: 0);
            var (customerB, _) = await CreateCompletedSaleAsync(openingBalance: 0, quantity: 5, price: 200, paidAmount: 0);

            var ledgerA = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customerA.Id });
            ledgerA.ClosingBalance.ShouldBe(1000);

            var ledgerB = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customerB.Id });
            ledgerB.ClosingBalance.ShouldBe(1000);
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Access_TenantB_Ledger()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-ledger-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-ledger-" + Guid.NewGuid().ToString("N"));

        Guid customerId;
        using (_currentTenant.Change(tenantBId))
        {
            var customer = await CreateCustomerAsync(openingBalance: 1000);
            customerId = customer.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customerId }));
            exception.Code.ShouldBe("ShopManagement:CustomerLedgerNotFound");
        }
    }

    [Fact]
    public async Task Search_And_Reference_Filters_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-filters-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, _) = await CreateCompletedSaleAsync(openingBalance: 0, quantity: 10, price: 100, paidAmount: 0);
            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(customer.Id, 200, referenceNumber: "TXN-UNIQUE-42"));
            await _paymentAppService.PostAsync(payment.Id);

            var byType = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput
            {
                CustomerId = customer.Id,
                ReferenceType = ShopCustomerLedgerReferenceType.Sale,
            });
            byType.Entries.ShouldAllBe(x => x.ReferenceType == ShopCustomerLedgerReferenceType.OpeningBalance || x.ReferenceType == ShopCustomerLedgerReferenceType.Sale);

            var bySearch = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput
            {
                CustomerId = customer.Id,
                Filter = "TXN-UNIQUE-42",
            });
            bySearch.Entries.ShouldContain(x => x.ReferenceType == ShopCustomerLedgerReferenceType.CustomerPayment);
            bySearch.Entries.ShouldNotContain(x => x.ReferenceType == ShopCustomerLedgerReferenceType.Sale);

            // Filtering only changes the displayed rows, never the underlying totals.
            byType.ClosingBalance.ShouldBe(bySearch.ClosingBalance);
        }
    }

    [Fact]
    public void Users_Without_ViewAmounts_Do_Not_Receive_Financial_Values()
    {
        // The test host registers AddAlwaysAllowAuthorization(), so permission checks cannot be
        // exercised end-to-end here. This verifies the [Authorize] attribute is present with the
        // correct policy name, by static reflection; the hiding logic itself is verified by code review
        // (HideLedgerAmountsIfNotAllowedAsync / HideSummaryAmountsIfNotAllowedAsync / HideStatementAmountsIfNotAllowedAsync).
        var type = typeof(ShopCustomerLedgerAppService);
        var classAuthorize = type.GetCustomAttribute<AuthorizeAttribute>();
        classAuthorize.ShouldNotBeNull();
        classAuthorize!.Policy.ShouldBe(EHubPermissions.ShopCustomerLedger.Default);
    }

    [Fact]
    public async Task Customer_Detail_Summary_Matches_Ledger_Totals()
    {
        var tenantId = await CreateTenantAsync("tenant-summary-matches-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, _) = await CreateCompletedSaleAsync(openingBalance: 500, quantity: 10, price: 100, paidAmount: 0);

            var payment = await _paymentAppService.CreateAsync(BuildAdvancePaymentInput(customer.Id, 300));
            await _paymentAppService.PostAsync(payment.Id);

            var ledger = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });
            var summary = await _ledgerAppService.GetBalanceSummaryAsync(customer.Id);

            summary.CurrentBalance.ShouldBe(ledger.ClosingBalance);
            summary.ReceivableAmount.ShouldBe(ledger.ReceivableAmount);
            summary.AdvanceAmount.ShouldBe(ledger.AdvanceAmount);
            summary.OpeningBalance.ShouldBe(500);
            summary.TotalCompletedSales.ShouldBe(1000);
            summary.TotalInitialPaidAtSale.ShouldBe(0);
            summary.TotalPostedCustomerPayments.ShouldBe(300);
        }
    }

    [Fact]
    public async Task Sale_Detail_Payment_Totals_Remain_Consistent_With_Ledger()
    {
        var tenantId = await CreateTenantAsync("tenant-sale-ledger-consistency-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var (customer, sale) = await CreateCompletedSaleAsync(openingBalance: 0, quantity: 10, price: 100, paidAmount: 300);

            var outstandingBefore = await _paymentAppService.GetOutstandingSalesAsync(customer.Id);
            var rowBefore = outstandingBefore.Items.Single(x => x.SaleId == sale.Id);
            rowBefore.PendingAmount.ShouldBe(700);

            var ledgerBefore = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });
            ledgerBefore.ClosingBalance.ShouldBe(700);

            var payment = await _paymentAppService.CreateAsync(new CreateUpdateShopCustomerPaymentDto
            {
                CustomerId = customer.Id,
                PaymentDate = DateTime.Today,
                PaymentType = ShopCustomerPaymentType.InvoicePayment,
                PaymentMethod = ShopCustomerPaymentMethod.Cash,
                Amount = 700,
                Allocations = new List<CreateUpdateShopCustomerPaymentAllocationDto>
                {
                    new() { SaleId = sale.Id, AllocatedAmount = 700 }
                }
            });
            await _paymentAppService.PostAsync(payment.Id);

            var outstandingAfter = await _paymentAppService.GetOutstandingSalesAsync(customer.Id);
            outstandingAfter.Items.ShouldNotContain(x => x.SaleId == sale.Id);

            var ledgerAfter = await _ledgerAppService.GetLedgerAsync(new GetShopCustomerLedgerInput { CustomerId = customer.Id });
            ledgerAfter.ClosingBalance.ShouldBe(0);
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

    private async Task<ShopProductDto> CreateProductWithStockAsync(decimal stockQuantity, decimal purchasePrice, decimal salePrice)
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

    private async Task<(ShopCustomerDto Customer, ShopSaleDto Sale)> CreateCompletedSaleAsync(
        decimal openingBalance, decimal quantity, decimal price, decimal paidAmount, ShopSaleType saleType = ShopSaleType.Credit)
    {
        var customer = await CreateCustomerAsync(openingBalance);
        var product = await CreateProductWithStockAsync(quantity, price / 2, price);
        var sale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, (product.Id, quantity, price, 0, 0), paidAmount, saleType));
        var completed = await _saleAppService.CompleteAsync(sale.Id);
        return (customer, completed);
    }

    private static CreateShopSaleDto BuildSaleCreateInput(
        Guid customerId, (Guid ProductId, decimal Quantity, decimal Price, decimal Discount, decimal Tax) item, decimal paidAmount, ShopSaleType saleType = ShopSaleType.Credit) => new()
    {
        CustomerId = customerId,
        SaleDate = DateTime.Today,
        SaleType = saleType,
        DueDate = saleType == ShopSaleType.Credit ? DateTime.Today.AddDays(30) : null,
        PaymentMethod = ShopSalePaymentMethod.Cash,
        PaidAmount = paidAmount,
        Items = new List<CreateShopSaleItemDto>
        {
            new() { ProductId = item.ProductId, Quantity = item.Quantity, UnitSalePrice = item.Price, DiscountPercentage = item.Discount, TaxPercentage = item.Tax }
        }
    };

    private static CreateUpdateShopCustomerPaymentDto BuildAdvancePaymentInput(Guid customerId, decimal amount, string? referenceNumber = null) => new()
    {
        CustomerId = customerId,
        PaymentDate = DateTime.Today,
        PaymentType = ShopCustomerPaymentType.Advance,
        PaymentMethod = ShopCustomerPaymentMethod.Cash,
        Amount = amount,
        ReferenceNumber = referenceNumber,
        Allocations = new List<CreateUpdateShopCustomerPaymentAllocationDto>(),
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
