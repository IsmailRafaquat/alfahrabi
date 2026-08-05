using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EHub.ShopManagement.Customers;
using EHub.ShopManagement.ExpenseCategories;
using EHub.ShopManagement.Expenses;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.Reports;
using EHub.ShopManagement.SaleReturns;
using EHub.ShopManagement.Sales;
using EHub.ShopManagement.Suppliers;
using EHub.ShopManagement.Units;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Modularity;
using Xunit;

namespace EHub.ShopManagement.ProfitLoss;

/// <summary>
/// Regression coverage for <see cref="ShopProfitLossCalculator.ComputeAggregateAsync"/>'s join-based
/// rewrite (it used to fetch full Sale/SaleReturn rows into memory to extract IDs, then re-query
/// SaleItems/SaleReturnItems with a Contains(idList); it now joins straight to the filtered
/// Sales/SaleReturns queryable instead). These tests exercise it through the real AppService against
/// the real EF Core provider, so they'd catch a bad SQL translation, not just a LINQ-to-objects bug.
/// </summary>
public abstract class ShopProfitLossAppServiceTests<TStartupModule> : EHubApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShopProfitLossAppService _profitLossAppService;
    private readonly IShopSaleAppService _saleAppService;
    private readonly IShopSaleReturnAppService _saleReturnAppService;
    private readonly IShopCustomerAppService _customerAppService;
    private readonly IShopExpenseAppService _expenseAppService;
    private readonly IShopExpenseCategoryAppService _expenseCategoryAppService;
    private readonly IShopProductCategoryAppService _categoryAppService;
    private readonly IShopUnitAppService _unitAppService;
    private readonly IShopProductAppService _productAppService;
    private readonly IShopSupplierAppService _supplierAppService;
    private readonly IShopPurchaseOrderAppService _poAppService;
    private readonly IShopGoodsReceiptAppService _grAppService;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly ICurrentTenant _currentTenant;

    protected ShopProfitLossAppServiceTests()
    {
        _profitLossAppService = GetRequiredService<IShopProfitLossAppService>();
        _saleAppService = GetRequiredService<IShopSaleAppService>();
        _saleReturnAppService = GetRequiredService<IShopSaleReturnAppService>();
        _customerAppService = GetRequiredService<IShopCustomerAppService>();
        _expenseAppService = GetRequiredService<IShopExpenseAppService>();
        _expenseCategoryAppService = GetRequiredService<IShopExpenseCategoryAppService>();
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
    public async Task Summary_Aggregates_Sales_Cogs_And_Expenses_For_The_Period()
    {
        var tenantId = await CreateTenantAsync("tenant-pl-summary-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var product = await CreateProductWithStockAsync(20, 100, 250);
            var customer = await CreateCustomerAsync();

            // 5 units @ 250, no discount/tax -> 1250 gross/net sales; cost basis 5 * 100 purchase price = 500 COGS.
            var sale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, product.Id, 5, 250, 1250));
            await _saleAppService.CompleteAsync(sale.Id, new CompleteShopSaleDto());

            var category = await CreateExpenseCategoryAsync();
            var expense = await _expenseAppService.CreateAsync(new CreateUpdateShopExpenseDto
            {
                ExpenseCategoryId = category.Id,
                ExpenseDate = DateTime.Today,
                Amount = 300,
                PaymentMethod = ShopExpensePaymentMethod.Cash,
            });
            await _expenseAppService.PostAsync(expense.Id);

            var summary = await _profitLossAppService.GetSummaryAsync(new GetShopProfitLossInput
            {
                Period = ShopReportPeriod.Today,
                CompareWithPreviousPeriod = false,
            });

            summary.NetSales.ShouldBe(1250);

            summary.CostOfGoodsSold.ShouldNotBeNull();
            summary.CostOfGoodsSold!.Value.ShouldBe(500);

            summary.GrossProfit.ShouldNotBeNull();
            summary.GrossProfit!.Value.ShouldBe(750);

            summary.OperatingExpenses.ShouldNotBeNull();
            summary.OperatingExpenses!.Value.ShouldBe(300);

            summary.NetProfit.ShouldNotBeNull();
            summary.NetProfit!.Value.ShouldBe(450);
        }
    }

    [Fact]
    public async Task Summary_Nets_Out_A_Completed_Sale_Return_From_Sales_And_Cogs()
    {
        var tenantId = await CreateTenantAsync("tenant-pl-return-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var product = await CreateProductWithStockAsync(20, 100, 250);
            var customer = await CreateCustomerAsync();

            var sale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, product.Id, 5, 250, 1250));
            var completedSale = await _saleAppService.CompleteAsync(sale.Id, new CompleteShopSaleDto());

            // Return 2 of the 5 units sold.
            var saleReturn = await _saleReturnAppService.CreateAsync(new CreateShopSaleReturnDto
            {
                SaleId = completedSale.Id,
                ReturnDate = DateTime.Today,
                Reason = ShopSaleReturnReason.CustomerChangedMind,
                SettlementType = ShopSaleReturnSettlementType.CustomerCredit,
                Items = new List<CreateShopSaleReturnItemDto>
                {
                    new() { SaleItemId = completedSale.Items[0].Id, ReturnQuantity = 2, Reason = ShopSaleReturnReason.CustomerChangedMind },
                },
            });
            await _saleReturnAppService.CompleteAsync(saleReturn.Id);

            var summary = await _profitLossAppService.GetSummaryAsync(new GetShopProfitLossInput
            {
                Period = ShopReportPeriod.Today,
                CompareWithPreviousPeriod = false,
            });

            // Gross 1250 - return (2 * 250 = 500) = 750 net sales; COGS 5*100 - 2*100 returned = 300.
            summary.NetSales.ShouldBe(750);
            summary.CostOfGoodsSold.ShouldNotBeNull();
            summary.CostOfGoodsSold!.Value.ShouldBe(300);
        }
    }

    [Fact]
    public async Task Summary_Returns_Zero_Values_When_No_Data_Exists_For_The_Period()
    {
        var tenantId = await CreateTenantAsync("tenant-pl-empty-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var summary = await _profitLossAppService.GetSummaryAsync(new GetShopProfitLossInput
            {
                Period = ShopReportPeriod.Today,
                CompareWithPreviousPeriod = false,
            });

            summary.NetSales.ShouldBe(0);
            summary.CostOfGoodsSold.ShouldNotBeNull();
            summary.CostOfGoodsSold!.Value.ShouldBe(0);
            summary.OperatingExpenses.ShouldNotBeNull();
            summary.OperatingExpenses!.Value.ShouldBe(0);
            summary.NetProfit.ShouldNotBeNull();
            summary.NetProfit!.Value.ShouldBe(0);
            summary.ResultStatus.ShouldBe(ShopProfitLossResultStatus.BreakEven);
        }
    }

    [Fact]
    public async Task Summary_Reports_Loss_When_Expenses_Exceed_Gross_Profit()
    {
        var tenantId = await CreateTenantAsync("tenant-pl-loss-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var product = await CreateProductWithStockAsync(20, 100, 250);
            var customer = await CreateCustomerAsync();

            // 2 units @ 250 = 500 net sales; cost basis 2 * 100 = 200 COGS -> 300 gross profit.
            var sale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, product.Id, 2, 250, 500));
            await _saleAppService.CompleteAsync(sale.Id, new CompleteShopSaleDto());

            var category = await CreateExpenseCategoryAsync();
            var expense = await _expenseAppService.CreateAsync(new CreateUpdateShopExpenseDto
            {
                ExpenseCategoryId = category.Id,
                ExpenseDate = DateTime.Today,
                Amount = 1000, // Bigger than the 300 gross profit -> net loss.
                PaymentMethod = ShopExpensePaymentMethod.Cash,
            });
            await _expenseAppService.PostAsync(expense.Id);

            var summary = await _profitLossAppService.GetSummaryAsync(new GetShopProfitLossInput
            {
                Period = ShopReportPeriod.Today,
                CompareWithPreviousPeriod = false,
            });

            summary.NetProfit.ShouldNotBeNull();
            summary.NetProfit!.Value.ShouldBe(-700);
            summary.ResultStatus.ShouldBe(ShopProfitLossResultStatus.Loss);
        }
    }

    [Fact]
    public async Task Summary_Excludes_Sales_Outside_The_Requested_Date_Range()
    {
        var tenantId = await CreateTenantAsync("tenant-pl-daterange-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var product = await CreateProductWithStockAsync(20, 100, 250);
            var customer = await CreateCustomerAsync();

            // Today's sale should count...
            var todaySale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, product.Id, 2, 250, 500));
            await _saleAppService.CompleteAsync(todaySale.Id, new CompleteShopSaleDto());

            // ...but a sale from 10 days ago should not, when the query is scoped to "Today".
            var oldSale = await _saleAppService.CreateAsync(
                BuildSaleCreateInput(customer.Id, product.Id, 3, 250, 750, DateTime.Today.AddDays(-10)));
            await _saleAppService.CompleteAsync(oldSale.Id, new CompleteShopSaleDto());

            var summary = await _profitLossAppService.GetSummaryAsync(new GetShopProfitLossInput
            {
                Period = ShopReportPeriod.Today,
                CompareWithPreviousPeriod = false,
            });

            summary.NetSales.ShouldBe(500);
        }
    }

    [Fact]
    public async Task Summary_Isolates_Totals_Between_Tenants()
    {
        var tenantAId = await CreateTenantAsync("tenant-pl-iso-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-pl-iso-b-" + Guid.NewGuid().ToString("N"));

        using (_currentTenant.Change(tenantAId))
        {
            var product = await CreateProductWithStockAsync(20, 100, 250);
            var customer = await CreateCustomerAsync();
            var sale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, product.Id, 4, 250, 1000));
            await _saleAppService.CompleteAsync(sale.Id, new CompleteShopSaleDto());
        }

        using (_currentTenant.Change(tenantBId))
        {
            var summary = await _profitLossAppService.GetSummaryAsync(new GetShopProfitLossInput
            {
                Period = ShopReportPeriod.Today,
                CompareWithPreviousPeriod = false,
            });

            // Tenant B has posted nothing - tenant A's sale must never leak across the tenant filter.
            summary.NetSales.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Summary_Excludes_Cancelled_Sales_And_Unposted_Expenses()
    {
        var tenantId = await CreateTenantAsync("tenant-pl-cancelled-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var product = await CreateProductWithStockAsync(20, 100, 250);
            var customer = await CreateCustomerAsync();

            // A sale can only be cancelled while still Draft (completing then reversing goes through
            // a SaleReturn instead, covered by Summary_Nets_Out_A_Completed_Sale_Return_From_Sales_And_Cogs).
            var sale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, product.Id, 2, 250, 500));
            await _saleAppService.CancelAsync(sale.Id, new CancelShopSaleDto { CancellationReason = "Test cancellation" });

            var category = await CreateExpenseCategoryAsync();
            // Created but never posted (still Draft) - must not count as an operating expense.
            await _expenseAppService.CreateAsync(new CreateUpdateShopExpenseDto
            {
                ExpenseCategoryId = category.Id,
                ExpenseDate = DateTime.Today,
                Amount = 400,
                PaymentMethod = ShopExpensePaymentMethod.Cash,
            });

            var summary = await _profitLossAppService.GetSummaryAsync(new GetShopProfitLossInput
            {
                Period = ShopReportPeriod.Today,
                CompareWithPreviousPeriod = false,
            });

            summary.NetSales.ShouldBe(0);
            summary.OperatingExpenses.ShouldNotBeNull();
            summary.OperatingExpenses!.Value.ShouldBe(0);
        }
    }

    [Fact]
    public async Task GetAsync_Summary_Matches_GetSummaryAsync_For_The_Same_Period()
    {
        // GetAsync (ComputeDetailedAsync, row-level + in-memory aggregation for the trend chart) and
        // GetSummaryAsync (ComputeAggregateAsync, server-side aggregation only) must always agree -
        // they're two different code paths computing the same numbers for the same period.
        var tenantId = await CreateTenantAsync("tenant-pl-cross-check-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var product = await CreateProductWithStockAsync(20, 100, 250);
            var customer = await CreateCustomerAsync();

            var sale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, product.Id, 5, 250, 1250));
            await _saleAppService.CompleteAsync(sale.Id, new CompleteShopSaleDto());

            var category = await CreateExpenseCategoryAsync();
            var expense = await _expenseAppService.CreateAsync(new CreateUpdateShopExpenseDto
            {
                ExpenseCategoryId = category.Id,
                ExpenseDate = DateTime.Today,
                Amount = 300,
                PaymentMethod = ShopExpensePaymentMethod.Cash,
            });
            await _expenseAppService.PostAsync(expense.Id);

            var input = new GetShopProfitLossInput
            {
                Period = ShopReportPeriod.Today,
                CompareWithPreviousPeriod = false,
                IncludeExpenseBreakdown = false,
                IncludeProductContribution = false,
            };

            var full = await _profitLossAppService.GetAsync(input);
            var summary = await _profitLossAppService.GetSummaryAsync(input);

            full.Summary.NetSales.ShouldBe(summary.NetSales);
            full.Summary.CostOfGoodsSold.ShouldBe(summary.CostOfGoodsSold);
            full.Summary.GrossProfit.ShouldBe(summary.GrossProfit);
            full.Summary.OperatingExpenses.ShouldBe(summary.OperatingExpenses);
            full.Summary.NetProfit.ShouldBe(summary.NetProfit);
        }
    }

    private async Task<ShopExpenseCategoryDto> CreateExpenseCategoryAsync()
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
        return await _expenseCategoryAppService.CreateAsync(new CreateUpdateShopExpenseCategoryDto
        { Code = "CAT-" + suffix, Name = "Category " + suffix, IsActive = true });
    }

    private async Task<ShopCustomerDto> CreateCustomerAsync()
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        return await _customerAppService.CreateAsync(new CreateUpdateShopCustomerDto
        {
            Code = "CUS-" + suffix,
            Name = "Customer " + suffix,
            CustomerType = ShopCustomerType.Business,
            OpeningBalance = 0,
            PaymentTermsDays = 30,
            IsActive = true,
        });
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

    private async Task<ShopProductDto> CreateProductWithStockAsync(decimal quantity, decimal purchasePrice, decimal salePrice)
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
            { new() { ProductId = product.Id, OrderedQuantity = quantity, UnitPurchasePrice = purchasePrice } }
        });
        await _poAppService.SubmitAsync(po.Id);
        var approved = await _poAppService.ApproveAsync(po.Id);

        var gr = await _grAppService.CreateAsync(new CreateShopGoodsReceiptDto
        {
            PurchaseOrderId = approved.Id,
            ReceiptDate = DateTime.Today,
            Items = new List<CreateShopGoodsReceiptItemDto>
            { new() { PurchaseOrderItemId = approved.Items[0].Id, ReceivedQuantity = quantity, PurchasePrice = purchasePrice } }
        });
        await _grAppService.CompleteAsync(gr.Id);

        return await _productAppService.GetAsync(product.Id);
    }

    private static CreateShopSaleDto BuildSaleCreateInput(
        Guid customerId, Guid productId, decimal quantity, decimal price, decimal paidAmount, DateTime? saleDate = null) => new()
    {
        CustomerId = customerId,
        SaleDate = saleDate ?? DateTime.Today,
        SaleType = ShopSaleType.Cash,
        PaymentMethod = ShopSalePaymentMethod.Cash,
        PaidAmount = paidAmount,
        Items = new List<CreateShopSaleItemDto>
        {
            new() { ProductId = productId, Quantity = quantity, UnitSalePrice = price, DiscountPercentage = 0, TaxPercentage = 0 }
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
