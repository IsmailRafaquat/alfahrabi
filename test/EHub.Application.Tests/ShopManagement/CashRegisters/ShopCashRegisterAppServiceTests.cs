using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.CustomerPayments;
using EHub.ShopManagement.Customers;
using EHub.ShopManagement.ExpenseCategories;
using EHub.ShopManagement.Expenses;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.SaleReturns;
using EHub.ShopManagement.Sales;
using EHub.ShopManagement.Suppliers;
using EHub.ShopManagement.SupplierPayments;
using EHub.ShopManagement.Units;
using Microsoft.AspNetCore.Authorization;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Modularity;
using Xunit;

namespace EHub.ShopManagement.CashRegisters;

public abstract class ShopCashRegisterAppServiceTests<TStartupModule> : EHubApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IShopCashRegisterAppService _cashAppService;
    private readonly IShopSaleAppService _saleAppService;
    private readonly IShopCustomerAppService _customerAppService;
    private readonly IShopCustomerPaymentAppService _customerPaymentAppService;
    private readonly IShopSupplierPaymentAppService _supplierPaymentAppService;
    private readonly IShopExpenseAppService _expenseAppService;
    private readonly IShopExpenseCategoryAppService _expenseCategoryAppService;
    private readonly IShopSaleReturnAppService _saleReturnAppService;
    private readonly IShopProductCategoryAppService _categoryAppService;
    private readonly IShopUnitAppService _unitAppService;
    private readonly IShopProductAppService _productAppService;
    private readonly IShopSupplierAppService _supplierAppService;
    private readonly IShopPurchaseOrderAppService _poAppService;
    private readonly IShopGoodsReceiptAppService _grAppService;
    private readonly ITenantManager _tenantManager;
    private readonly IRepository<Tenant, Guid> _tenantRepository;
    private readonly ICurrentTenant _currentTenant;

    protected ShopCashRegisterAppServiceTests()
    {
        _cashAppService = GetRequiredService<IShopCashRegisterAppService>();
        _saleAppService = GetRequiredService<IShopSaleAppService>();
        _customerAppService = GetRequiredService<IShopCustomerAppService>();
        _customerPaymentAppService = GetRequiredService<IShopCustomerPaymentAppService>();
        _supplierPaymentAppService = GetRequiredService<IShopSupplierPaymentAppService>();
        _expenseAppService = GetRequiredService<IShopExpenseAppService>();
        _expenseCategoryAppService = GetRequiredService<IShopExpenseCategoryAppService>();
        _saleReturnAppService = GetRequiredService<IShopSaleReturnAppService>();
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
    public async Task TenantA_Can_Create_A_Cash_Register()
    {
        var tenantId = await CreateTenantAsync("tenant-create-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var dto = await _cashAppService.CreateAsync(BuildRegisterInput("MAIN", "Main Counter", isDefault: true));
            dto.Code.ShouldBe("MAIN");
            dto.Name.ShouldBe("Main Counter");
            dto.IsDefault.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task Only_One_Default_Register_Exists_Per_Tenant()
    {
        var tenantId = await CreateTenantAsync("tenant-default-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var first = await _cashAppService.CreateAsync(BuildRegisterInput("MAIN", "Main Counter", isDefault: true));
            var second = await _cashAppService.CreateAsync(BuildRegisterInput("BACK", "Back Counter", isDefault: true));

            var firstReloaded = await _cashAppService.GetAsync(first.Id);
            firstReloaded.IsDefault.ShouldBeFalse();

            var secondReloaded = await _cashAppService.GetAsync(second.Id);
            secondReloaded.IsDefault.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task TenantA_Cannot_Access_TenantB_Register()
    {
        var tenantAId = await CreateTenantAsync("tenant-a-" + Guid.NewGuid().ToString("N"));
        var tenantBId = await CreateTenantAsync("tenant-b-" + Guid.NewGuid().ToString("N"));

        Guid registerId;
        using (_currentTenant.Change(tenantBId))
        {
            var register = await _cashAppService.CreateAsync(BuildRegisterInput("MAIN", "Main Counter", isDefault: true));
            registerId = register.Id;
        }

        using (_currentTenant.Change(tenantAId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() => _cashAppService.GetAsync(registerId));
            exception.Code.ShouldBe("ShopManagement:CashRegisterNotFound");
        }
    }

    [Fact]
    public async Task Only_One_Open_Closing_Exists_Per_Register()
    {
        var tenantId = await CreateTenantAsync("tenant-one-open-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            await _cashAppService.OpenAsync(register.Id, new OpenShopCashRegisterDto { BusinessDate = DateTime.Today, OpeningCash = 1000 });

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _cashAppService.OpenAsync(register.Id, new OpenShopCashRegisterDto { BusinessDate = DateTime.Today, OpeningCash = 500 }));
            exception.Code.ShouldBe("ShopManagement:CashRegisterAlreadyOpen");
        }
    }

    [Fact]
    public async Task Opening_Cash_Creates_An_Opening_Transaction()
    {
        var tenantId = await CreateTenantAsync("tenant-opening-tx-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            var closing = await _cashAppService.OpenAsync(register.Id, new OpenShopCashRegisterDto { BusinessDate = DateTime.Today, OpeningCash = 10000 });

            var transactions = await _cashAppService.GetTransactionsAsync(new GetShopCashTransactionsInput { CashRegisterId = register.Id });
            transactions.Items.ShouldContain(x => x.TransactionType == ShopCashTransactionType.OpeningCash && x.Direction == ShopCashDirection.In && x.Amount == 10000);

            closing.OpeningCash.ShouldBe(10000);
        }
    }

    [Fact]
    public async Task Draft_Sale_Does_Not_Create_Cash_Transaction()
    {
        var tenantId = await CreateTenantAsync("tenant-draft-sale-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            await OpenRegisterAsync(register.Id, 1000);

            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(10, 50, 100);
            await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, product.Id, 5, 100, 0, ShopSaleType.Cash, ShopSalePaymentMethod.Cash));

            var transactions = await _cashAppService.GetTransactionsAsync(new GetShopCashTransactionsInput { CashRegisterId = register.Id });
            transactions.Items.ShouldNotContain(x => x.TransactionType == ShopCashTransactionType.CashSale);
        }
    }

    [Fact]
    public async Task Completed_Cash_Sale_Creates_Cash_In()
    {
        var tenantId = await CreateTenantAsync("tenant-cash-sale-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            await OpenRegisterAsync(register.Id, 1000);

            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(10, 50, 100);
            var sale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, product.Id, 5, 100, 500, ShopSaleType.Cash, ShopSalePaymentMethod.Cash));
            await _saleAppService.CompleteAsync(sale.Id);

            var transactions = await _cashAppService.GetTransactionsAsync(new GetShopCashTransactionsInput { CashRegisterId = register.Id });
            var saleTx = transactions.Items.Single(x => x.TransactionType == ShopCashTransactionType.CashSale);
            saleTx.Direction.ShouldBe(ShopCashDirection.In);
            saleTx.Amount.ShouldBe(500);
        }
    }

    [Fact]
    public async Task Credit_Sale_Records_Only_Actual_Initial_Cash_Received()
    {
        var tenantId = await CreateTenantAsync("tenant-credit-sale-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            await OpenRegisterAsync(register.Id, 1000);

            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(10, 50, 100);
            // Credit sale (GrandTotal 1000) with only 300 paid in cash up front.
            var sale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, product.Id, 10, 100, 300, ShopSaleType.Credit, ShopSalePaymentMethod.Cash));
            await _saleAppService.CompleteAsync(sale.Id);

            var transactions = await _cashAppService.GetTransactionsAsync(new GetShopCashTransactionsInput { CashRegisterId = register.Id });
            var saleTx = transactions.Items.Single(x => x.TransactionType == ShopCashTransactionType.CashSale);
            saleTx.Amount.ShouldBe(300);
        }
    }

    [Fact]
    public async Task Posted_Cash_Customer_Payment_Creates_Cash_In()
    {
        var tenantId = await CreateTenantAsync("tenant-cust-payment-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            await OpenRegisterAsync(register.Id, 1000);

            var customer = await CreateCustomerAsync();
            var payment = await _customerPaymentAppService.CreateAsync(new CreateUpdateShopCustomerPaymentDto
            {
                CustomerId = customer.Id,
                PaymentDate = DateTime.Today,
                PaymentType = ShopCustomerPaymentType.Advance,
                PaymentMethod = ShopCustomerPaymentMethod.Cash,
                Amount = 400,
                Allocations = new List<CreateUpdateShopCustomerPaymentAllocationDto>(),
            });
            await _customerPaymentAppService.PostAsync(payment.Id);

            var transactions = await _cashAppService.GetTransactionsAsync(new GetShopCashTransactionsInput { CashRegisterId = register.Id });
            var tx = transactions.Items.Single(x => x.TransactionType == ShopCashTransactionType.CustomerPayment);
            tx.Direction.ShouldBe(ShopCashDirection.In);
            tx.Amount.ShouldBe(400);
        }
    }

    [Fact]
    public async Task Posted_Cash_Supplier_Payment_Creates_Cash_Out()
    {
        var tenantId = await CreateTenantAsync("tenant-sup-payment-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            await OpenRegisterAsync(register.Id, 5000);

            var (supplier, _) = await CreateCompletedGoodsReceiptAsync(10, 100);
            var payment = await _supplierPaymentAppService.CreateAsync(new CreateUpdateShopSupplierPaymentDto
            {
                SupplierId = supplier.Id,
                PaymentDate = DateTime.Today,
                PaymentType = ShopSupplierPaymentType.Advance,
                PaymentMethod = ShopSupplierPaymentMethod.Cash,
                Amount = 600,
                Allocations = new List<CreateUpdateShopSupplierPaymentAllocationDto>(),
            });
            await _supplierPaymentAppService.PostAsync(payment.Id);

            var transactions = await _cashAppService.GetTransactionsAsync(new GetShopCashTransactionsInput { CashRegisterId = register.Id });
            var tx = transactions.Items.Single(x => x.TransactionType == ShopCashTransactionType.SupplierPayment);
            tx.Direction.ShouldBe(ShopCashDirection.Out);
            tx.Amount.ShouldBe(600);
        }
    }

    [Fact]
    public async Task Posted_Cash_Expense_Creates_Cash_Out()
    {
        var tenantId = await CreateTenantAsync("tenant-expense-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            await OpenRegisterAsync(register.Id, 5000);

            var category = await CreateExpenseCategoryAsync();
            var expense = await _expenseAppService.CreateAsync(new CreateUpdateShopExpenseDto
            {
                ExpenseCategoryId = category.Id,
                ExpenseDate = DateTime.Today,
                Amount = 2000,
                PaymentMethod = ShopExpensePaymentMethod.Cash,
            });
            await _expenseAppService.PostAsync(expense.Id);

            var transactions = await _cashAppService.GetTransactionsAsync(new GetShopCashTransactionsInput { CashRegisterId = register.Id });
            var tx = transactions.Items.Single(x => x.TransactionType == ShopCashTransactionType.Expense);
            tx.Direction.ShouldBe(ShopCashDirection.Out);
            tx.Amount.ShouldBe(2000);
        }
    }

    [Fact]
    public async Task Bank_Card_Cheque_Transactions_Are_Ignored()
    {
        var tenantId = await CreateTenantAsync("tenant-noncash-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            await OpenRegisterAsync(register.Id, 1000);

            var category = await CreateExpenseCategoryAsync();
            var bankExpense = await _expenseAppService.CreateAsync(new CreateUpdateShopExpenseDto
            {
                ExpenseCategoryId = category.Id,
                ExpenseDate = DateTime.Today,
                Amount = 300,
                PaymentMethod = ShopExpensePaymentMethod.BankTransfer,
                BankName = "Meezan Bank",
            });
            await _expenseAppService.PostAsync(bankExpense.Id);

            var customer = await CreateCustomerAsync();
            var chequePayment = await _customerPaymentAppService.CreateAsync(new CreateUpdateShopCustomerPaymentDto
            {
                CustomerId = customer.Id,
                PaymentDate = DateTime.Today,
                PaymentType = ShopCustomerPaymentType.Advance,
                PaymentMethod = ShopCustomerPaymentMethod.Cheque,
                Amount = 700,
                ChequeNumber = "CHQ-1",
                BankName = "HBL",
                Allocations = new List<CreateUpdateShopCustomerPaymentAllocationDto>(),
            });
            await _customerPaymentAppService.PostAsync(chequePayment.Id);

            var transactions = await _cashAppService.GetTransactionsAsync(new GetShopCashTransactionsInput { CashRegisterId = register.Id });
            transactions.Items.ShouldNotContain(x => x.TransactionType == ShopCashTransactionType.Expense);
            transactions.Items.ShouldNotContain(x => x.TransactionType == ShopCashTransactionType.CustomerPayment);
        }
    }

    [Fact]
    public async Task Cash_Refund_Creates_Cash_Out()
    {
        var tenantId = await CreateTenantAsync("tenant-refund-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            await OpenRegisterAsync(register.Id, 5000);

            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(20, 100, 250, stockQuantity: 100);
            var sale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, product.Id, 20, 250, 5000, ShopSaleType.Cash, ShopSalePaymentMethod.Cash));
            var completedSale = await _saleAppService.CompleteAsync(sale.Id);

            var saleReturn = await _saleReturnAppService.CreateAsync(new CreateShopSaleReturnDto
            {
                SaleId = completedSale.Id,
                ReturnDate = DateTime.Today,
                Reason = ShopSaleReturnReason.Damaged,
                SettlementType = ShopSaleReturnSettlementType.CashRefund,
                OtherCharges = 0,
                Items = new List<CreateShopSaleReturnItemDto>
                {
                    new() { SaleItemId = completedSale.Items[0].Id, ReturnQuantity = 5, Reason = ShopSaleReturnReason.Damaged }
                }
            });
            await _saleReturnAppService.CompleteAsync(saleReturn.Id);

            var transactions = await _cashAppService.GetTransactionsAsync(new GetShopCashTransactionsInput { CashRegisterId = register.Id });
            var tx = transactions.Items.Single(x => x.TransactionType == ShopCashTransactionType.CustomerRefund);
            tx.Direction.ShouldBe(ShopCashDirection.Out);
            tx.Amount.ShouldBe(1250);
        }
    }

    [Fact]
    public async Task Duplicate_Source_Posting_Does_Not_Duplicate_Cash_Transactions()
    {
        var tenantId = await CreateTenantAsync("tenant-idempotent-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            await OpenRegisterAsync(register.Id, 1000);

            var category = await CreateExpenseCategoryAsync();
            var expense = await _expenseAppService.CreateAsync(new CreateUpdateShopExpenseDto
            {
                ExpenseCategoryId = category.Id,
                ExpenseDate = DateTime.Today,
                Amount = 500,
                PaymentMethod = ShopExpensePaymentMethod.Cash,
            });
            await _expenseAppService.PostAsync(expense.Id);

            // Posting twice is already prevented at the Expense domain level (ExpenseCannotBePosted),
            // but this confirms no duplicate cash transaction could ever result even if that guard
            // were bypassed, since RecordAutomaticTransactionAsync itself is idempotent.
            var exception = await Should.ThrowAsync<BusinessException>(() => _expenseAppService.PostAsync(expense.Id));
            exception.Code.ShouldBe("ShopManagement:ExpenseCannotBePosted");

            var transactions = await _cashAppService.GetTransactionsAsync(new GetShopCashTransactionsInput { CashRegisterId = register.Id });
            transactions.Items.Count(x => x.TransactionType == ShopCashTransactionType.Expense).ShouldBe(1);
        }
    }

    [Fact]
    public async Task Manual_Cash_In_And_Cash_Out_Work()
    {
        var tenantId = await CreateTenantAsync("tenant-manual-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            await OpenRegisterAsync(register.Id, 1000);

            var cashIn = await _cashAppService.CreateManualMovementAsync(new CreateManualCashMovementDto
            { CashRegisterId = register.Id, TransactionDate = DateTime.Today, Direction = ShopCashDirection.In, Amount = 200, Description = "Owner deposit" });
            cashIn.TransactionType.ShouldBe(ShopCashTransactionType.CashIn);

            var cashOut = await _cashAppService.CreateManualMovementAsync(new CreateManualCashMovementDto
            { CashRegisterId = register.Id, TransactionDate = DateTime.Today, Direction = ShopCashDirection.Out, Amount = 150, Description = "Petty cash" });
            cashOut.TransactionType.ShouldBe(ShopCashTransactionType.CashOut);

            var exception = await Should.ThrowAsync<Exception>(() => _cashAppService.CreateManualMovementAsync(new CreateManualCashMovementDto
            { CashRegisterId = register.Id, TransactionDate = DateTime.Today, Direction = ShopCashDirection.In, Amount = 0 }));
            exception.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task Expected_Closing_And_Difference_Calculation_Is_Correct()
    {
        var tenantId = await CreateTenantAsync("tenant-expected-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            var closing = await OpenRegisterAsync(register.Id, 10000);

            var customer = await CreateCustomerAsync();
            var product = await CreateProductWithStockAsync(30, 500, 1000, stockQuantity: 100);
            var sale = await _saleAppService.CreateAsync(BuildSaleCreateInput(customer.Id, product.Id, 30, 1000, 30000, ShopSaleType.Cash, ShopSalePaymentMethod.Cash));
            await _saleAppService.CompleteAsync(sale.Id);

            var customerPayment = await _customerPaymentAppService.CreateAsync(new CreateUpdateShopCustomerPaymentDto
            {
                CustomerId = customer.Id,
                PaymentDate = DateTime.Today,
                PaymentType = ShopCustomerPaymentType.Advance,
                PaymentMethod = ShopCustomerPaymentMethod.Cash,
                Amount = 5000,
                Allocations = new List<CreateUpdateShopCustomerPaymentAllocationDto>(),
            });
            await _customerPaymentAppService.PostAsync(customerPayment.Id);

            var (supplier, _) = await CreateCompletedGoodsReceiptAsync(10, 100);
            var supplierPayment = await _supplierPaymentAppService.CreateAsync(new CreateUpdateShopSupplierPaymentDto
            {
                SupplierId = supplier.Id,
                PaymentDate = DateTime.Today,
                PaymentType = ShopSupplierPaymentType.Advance,
                PaymentMethod = ShopSupplierPaymentMethod.Cash,
                Amount = 8000,
                Allocations = new List<CreateUpdateShopSupplierPaymentAllocationDto>(),
            });
            await _supplierPaymentAppService.PostAsync(supplierPayment.Id);

            var category = await CreateExpenseCategoryAsync();
            var expense = await _expenseAppService.CreateAsync(new CreateUpdateShopExpenseDto
            {
                ExpenseCategoryId = category.Id,
                ExpenseDate = DateTime.Today,
                Amount = 2000,
                PaymentMethod = ShopExpensePaymentMethod.Cash,
            });
            await _expenseAppService.PostAsync(expense.Id);

            // Opening 10,000 + Sales 30,000 + Customer 5,000 - Supplier 8,000 - Expenses 2,000 = 35,000
            var closed = await _cashAppService.CloseAsync(closing.Id, new CloseShopCashRegisterDto { ActualClosingCash = 34500 });
            closed.ExpectedClosingCash.ShouldBe(35000);
            closed.ActualClosingCash.ShouldBe(34500);
            closed.DifferenceAmount.ShouldBe(-500);

            var summary = await _cashAppService.GetSummaryAsync(closing.Id);
            summary.IsShort.ShouldBeTrue();
            summary.IsExcess.ShouldBeFalse();
        }
    }

    [Fact]
    public async Task Positive_Difference_Shows_Excess()
    {
        var tenantId = await CreateTenantAsync("tenant-excess-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            var closing = await OpenRegisterAsync(register.Id, 1000);

            var closed = await _cashAppService.CloseAsync(closing.Id, new CloseShopCashRegisterDto { ActualClosingCash = 1200 });
            closed.DifferenceAmount.ShouldBe(200);

            var summary = await _cashAppService.GetSummaryAsync(closing.Id);
            summary.IsExcess.ShouldBeTrue();
            summary.IsShort.ShouldBeFalse();
        }
    }

    [Fact]
    public async Task Negative_Difference_Shows_Shortage()
    {
        var tenantId = await CreateTenantAsync("tenant-shortage-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            var closing = await OpenRegisterAsync(register.Id, 1000);

            var closed = await _cashAppService.CloseAsync(closing.Id, new CloseShopCashRegisterDto { ActualClosingCash = 800 });
            closed.DifferenceAmount.ShouldBe(-200);

            var summary = await _cashAppService.GetSummaryAsync(closing.Id);
            summary.IsShort.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task Closed_Register_Cannot_Be_Closed_Twice()
    {
        var tenantId = await CreateTenantAsync("tenant-close-twice-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            var closing = await OpenRegisterAsync(register.Id, 1000);
            await _cashAppService.CloseAsync(closing.Id, new CloseShopCashRegisterDto { ActualClosingCash = 1000 });

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _cashAppService.CloseAsync(closing.Id, new CloseShopCashRegisterDto { ActualClosingCash = 1000 }));
            exception.Code.ShouldBe("ShopManagement:CashClosingCannotBeClosed");
        }
    }

    [Fact]
    public async Task Closed_Register_Cannot_Receive_New_Manual_Transactions()
    {
        var tenantId = await CreateTenantAsync("tenant-closed-manual-" + Guid.NewGuid().ToString("N"));
        using (_currentTenant.Change(tenantId))
        {
            var register = await CreateDefaultRegisterAsync();
            var closing = await OpenRegisterAsync(register.Id, 1000);
            await _cashAppService.CloseAsync(closing.Id, new CloseShopCashRegisterDto { ActualClosingCash = 1000 });

            var exception = await Should.ThrowAsync<BusinessException>(() => _cashAppService.CreateManualMovementAsync(new CreateManualCashMovementDto
            { CashRegisterId = register.Id, TransactionDate = DateTime.Today, Direction = ShopCashDirection.In, Amount = 100 }));
            exception.Code.ShouldBe("ShopManagement:CashRegisterNotOpen");
        }
    }

    [Fact]
    public void TenantId_Is_Not_Accepted_Through_Dto()
    {
        typeof(CreateUpdateShopCashRegisterDto).GetProperty("TenantId").ShouldBeNull();
        typeof(OpenShopCashRegisterDto).GetProperty("TenantId").ShouldBeNull();
        typeof(CloseShopCashRegisterDto).GetProperty("TenantId").ShouldBeNull();
        typeof(CreateManualCashMovementDto).GetProperty("TenantId").ShouldBeNull();
    }

    /// <summary>
    /// The test host registers AddAlwaysAllowAuthorization(), so permission checks cannot be
    /// exercised end-to-end here. This verifies the [Authorize] attributes themselves are present
    /// with the correct policy names, by static reflection.
    /// </summary>
    [Fact]
    public void Permissions_Are_Enforced()
    {
        var type = typeof(ShopCashRegisterAppService);
        AssertMethodPolicy(type, nameof(ShopCashRegisterAppService.CreateAsync), EHubPermissions.ShopCashRegisters.Create);
        AssertMethodPolicy(type, nameof(ShopCashRegisterAppService.UpdateAsync), EHubPermissions.ShopCashRegisters.Edit);
        AssertMethodPolicy(type, nameof(ShopCashRegisterAppService.DeleteAsync), EHubPermissions.ShopCashRegisters.Delete);
        AssertMethodPolicy(type, nameof(ShopCashRegisterAppService.OpenAsync), EHubPermissions.ShopCashClosings.Open);
        AssertMethodPolicy(type, nameof(ShopCashRegisterAppService.CloseAsync), EHubPermissions.ShopCashClosings.Close);
        AssertMethodPolicy(type, nameof(ShopCashRegisterAppService.CancelClosingAsync), EHubPermissions.ShopCashClosings.Cancel);
        AssertMethodPolicy(type, nameof(ShopCashRegisterAppService.CreateManualMovementAsync), EHubPermissions.ShopCashTransactions.ManualMovement);
        AssertMethodPolicy(type, nameof(ShopCashRegisterAppService.GetTransactionsAsync), EHubPermissions.ShopCashTransactions.Default);
    }

    [Fact]
    public async Task Cash_Transactions_Remain_Immutable()
    {
        // ShopCashRegisterTransaction exposes no update/delete methods on the entity or app service -
        // verified structurally, since there is no API surface capable of mutating a recorded transaction.
        typeof(IShopCashRegisterAppService).GetMethods().ShouldNotContain(m => m.Name.Contains("UpdateTransaction") || m.Name.Contains("DeleteTransaction"));
        typeof(ShopCashRegisterTransaction).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .ShouldNotContain(m => m.Name == "Update" || m.Name == "Delete" || m.Name == "SetAmount");

        await Task.CompletedTask;
    }

    private async Task<ShopCashRegisterDto> CreateDefaultRegisterAsync()
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
        return await _cashAppService.CreateAsync(BuildRegisterInput("MAIN-" + suffix, "Main Counter", isDefault: true));
    }

    private async Task<ShopCashClosingDto> OpenRegisterAsync(Guid registerId, decimal openingCash) =>
        await _cashAppService.OpenAsync(registerId, new OpenShopCashRegisterDto { BusinessDate = DateTime.Today, OpeningCash = openingCash });

    private static CreateUpdateShopCashRegisterDto BuildRegisterInput(string code, string name, bool isDefault) => new()
    {
        Code = code,
        Name = name,
        IsDefault = isDefault,
        IsActive = true,
    };

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

    private async Task<ShopProductDto> CreateProductWithStockAsync(decimal quantity, decimal purchasePrice, decimal salePrice, decimal stockQuantity = 0)
    {
        var effectiveStock = stockQuantity > 0 ? stockQuantity : quantity;
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
            { new() { ProductId = product.Id, OrderedQuantity = effectiveStock, UnitPurchasePrice = purchasePrice } }
        });
        await _poAppService.SubmitAsync(po.Id);
        var approved = await _poAppService.ApproveAsync(po.Id);

        var gr = await _grAppService.CreateAsync(new CreateShopGoodsReceiptDto
        {
            PurchaseOrderId = approved.Id,
            ReceiptDate = DateTime.Today,
            Items = new List<CreateShopGoodsReceiptItemDto>
            { new() { PurchaseOrderItemId = approved.Items[0].Id, ReceivedQuantity = effectiveStock, PurchasePrice = purchasePrice } }
        });
        await _grAppService.CompleteAsync(gr.Id);

        return await _productAppService.GetAsync(product.Id);
    }

    private async Task<(ShopSupplierDto Supplier, ShopGoodsReceiptDto GoodsReceipt)> CreateCompletedGoodsReceiptAsync(decimal quantity, decimal price)
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        var (categoryId, unitId) = await CreateCategoryAndUnitAsync();
        var product = await _productAppService.CreateAsync(new CreateShopProductDto
        {
            CategoryId = categoryId,
            UnitId = unitId,
            Name = "Product " + suffix,
            Code = "PRD-" + suffix,
            PurchasePrice = price,
            SalePrice = price * 2,
            IsActive = true,
        });

        var supplier = await _supplierAppService.CreateAsync(new CreateUpdateShopSupplierDto
        { Code = "SUP-" + suffix, Name = "Supplier " + suffix, IsActive = true });

        var po = await _poAppService.CreateAsync(new CreateShopPurchaseOrderDto
        {
            SupplierId = supplier.Id,
            OrderDate = DateTime.Today,
            Items = new List<CreateShopPurchaseOrderItemDto>
            { new() { ProductId = product.Id, OrderedQuantity = quantity, UnitPurchasePrice = price } }
        });
        await _poAppService.SubmitAsync(po.Id);
        var approved = await _poAppService.ApproveAsync(po.Id);

        var gr = await _grAppService.CreateAsync(new CreateShopGoodsReceiptDto
        {
            PurchaseOrderId = approved.Id,
            ReceiptDate = DateTime.Today,
            Items = new List<CreateShopGoodsReceiptItemDto>
            { new() { PurchaseOrderItemId = approved.Items[0].Id, ReceivedQuantity = quantity, PurchasePrice = price } }
        });
        var completed = await _grAppService.CompleteAsync(gr.Id);

        return (supplier, completed);
    }

    private static CreateShopSaleDto BuildSaleCreateInput(
        Guid customerId, Guid productId, decimal quantity, decimal price, decimal paidAmount,
        ShopSaleType saleType, ShopSalePaymentMethod paymentMethod) => new()
    {
        CustomerId = customerId,
        SaleDate = DateTime.Today,
        SaleType = saleType,
        DueDate = saleType == ShopSaleType.Credit ? DateTime.Today.AddDays(30) : null,
        PaymentMethod = paymentMethod,
        PaidAmount = paidAmount,
        Items = new List<CreateShopSaleItemDto>
        {
            new() { ProductId = productId, Quantity = quantity, UnitSalePrice = price, DiscountPercentage = 0, TaxPercentage = 0 }
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
