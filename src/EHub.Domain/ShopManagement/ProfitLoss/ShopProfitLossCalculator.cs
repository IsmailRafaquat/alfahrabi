using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.Expenses;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.ProductBatches;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.PurchaseReturns;
using EHub.ShopManagement.SaleReturns;
using EHub.ShopManagement.Sales;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.ProfitLoss;

/// <summary>
/// Computes raw Profit and Loss figures directly from posted transactions - no persisted totals,
/// no permission gating (that happens at the application-service/DTO layer). Reused by both
/// <c>ShopProfitLossAppService</c> and <c>ProfitLossNotificationGenerator</c> so the notification
/// job evaluates the exact same numbers a user would see, without needing a signed-in user context.
///
/// This is a Domain-layer service, so it deliberately queries only through the framework-agnostic
/// <see cref="IRepository{TEntity,TKey}.GetListAsync"/> predicate overload (no EF Core-specific
/// AsNoTracking/IAsyncQueryableExecuter, which aren't referenced from the Domain project) - the
/// underlying EF Core repository implementation still translates each predicate into a SQL WHERE
/// clause, so this stays bounded to the selected date range rather than loading full tables.
///
/// Net Sales = Gross Sales (Sale.SubTotal) - Discounts - Sale Returns (SaleReturn.GrandTotal).
/// COGS = Sum(SaleItem.Quantity * SaleItem.UnitCostSnapshot) - Sum(SaleReturnItem.ReturnQuantity * SaleReturnItem.UnitCostSnapshot).
/// UnitCostSnapshot is the cost actually captured at sale completion (batch-allocated cost for
/// batch-tracked products, last purchase price otherwise) - never the current product price.
/// Opening/Closing inventory values are a reconciliation-only approximation: Closing is the
/// *current* batch/product valuation (not a true point-in-time historical snapshot, which this
/// schema does not retain), and Opening is derived by rearranging
/// Opening + NetPurchases - Closing = COGS. These two fields are supporting figures only and are
/// never used as the authoritative COGS source.
/// </summary>
public class ShopProfitLossCalculator : ITransientDependency
{
    private readonly IRepository<ShopSale, Guid> _saleRepository;
    private readonly IRepository<ShopSaleItem, Guid> _saleItemRepository;
    private readonly IRepository<ShopSaleReturn, Guid> _saleReturnRepository;
    private readonly IRepository<ShopSaleReturnItem, Guid> _saleReturnItemRepository;
    private readonly IRepository<ShopExpense, Guid> _expenseRepository;
    private readonly IRepository<ShopGoodsReceipt, Guid> _goodsReceiptRepository;
    private readonly IRepository<ShopPurchaseReturn, Guid> _purchaseReturnRepository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopProductBatch, Guid> _productBatchRepository;

    public ShopProfitLossCalculator(
        IRepository<ShopSale, Guid> saleRepository,
        IRepository<ShopSaleItem, Guid> saleItemRepository,
        IRepository<ShopSaleReturn, Guid> saleReturnRepository,
        IRepository<ShopSaleReturnItem, Guid> saleReturnItemRepository,
        IRepository<ShopExpense, Guid> expenseRepository,
        IRepository<ShopGoodsReceipt, Guid> goodsReceiptRepository,
        IRepository<ShopPurchaseReturn, Guid> purchaseReturnRepository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopProductBatch, Guid> productBatchRepository)
    {
        _saleRepository = saleRepository;
        _saleItemRepository = saleItemRepository;
        _saleReturnRepository = saleReturnRepository;
        _saleReturnItemRepository = saleReturnItemRepository;
        _expenseRepository = expenseRepository;
        _goodsReceiptRepository = goodsReceiptRepository;
        _purchaseReturnRepository = purchaseReturnRepository;
        _productRepository = productRepository;
        _productBatchRepository = productBatchRepository;
    }

    /// <summary>Convenience wrapper over <see cref="ComputeDetailedAsync"/> for callers (e.g. the notification generator) that only need the aggregate numbers.</summary>
    public async Task<ShopProfitLossCoreResult> ComputeAsync(Guid tenantId, DateTime from, DateTime toExclusive, decimal? closingInventoryValue = null) =>
        (await ComputeDetailedAsync(tenantId, from, toExclusive, closingInventoryValue)).Core;

    /// <summary>
    /// Computes one period's figures and returns the raw entity lists alongside them, so a caller
    /// building several views of the same period (statement, trend, product contribution, expense
    /// breakdown) can reuse this one set of loaded data instead of re-querying per view.
    /// <paramref name="closingInventoryValue"/> is optional: it is NOT period-dependent (it's always
    /// the live current valuation), so a caller that needs several periods in one request (e.g.
    /// current + previous-period comparison) should compute it once via
    /// <see cref="ComputeClosingInventoryValueAsync"/> and pass it in every time, rather than paying
    /// for the full product/batch scan on every call.
    /// </summary>
    public async Task<ShopProfitLossComputation> ComputeDetailedAsync(Guid tenantId, DateTime from, DateTime toExclusive, decimal? closingInventoryValue = null)
    {
        var sales = await _saleRepository.GetListAsync(
            x => x.TenantId == tenantId && x.Status == ShopSaleStatus.Completed && x.SaleDate >= from && x.SaleDate < toExclusive);

        var grossSales = sales.Sum(x => x.SubTotal);
        var salesDiscounts = sales.Sum(x => x.DiscountAmount);
        var salesTax = sales.Sum(x => x.TaxAmount);

        var saleReturns = await _saleReturnRepository.GetListAsync(
            x => x.TenantId == tenantId && x.Status == ShopSaleReturnStatus.Completed && x.ReturnDate >= from && x.ReturnDate < toExclusive);
        var salesReturns = saleReturns.Sum(x => x.GrandTotal);

        var netSales = Round(grossSales - salesDiscounts - salesReturns);

        var saleIds = sales.Select(x => x.Id).ToHashSet();
        var saleItems = saleIds.Count == 0
            ? new List<ShopSaleItem>()
            : await _saleItemRepository.GetListAsync(i => i.TenantId == tenantId && saleIds.Contains(i.SaleId));
        var saleCogs = saleItems.Sum(i => i.Quantity * i.UnitCostSnapshot);

        var saleReturnIds = saleReturns.Select(x => x.Id).ToHashSet();
        var saleReturnItems = saleReturnIds.Count == 0
            ? new List<ShopSaleReturnItem>()
            : await _saleReturnItemRepository.GetListAsync(i => i.TenantId == tenantId && saleReturnIds.Contains(i.SaleReturnId));
        var returnCogs = saleReturnItems.Sum(i => i.ReturnQuantity * i.UnitCostSnapshot);

        var costOfGoodsSold = Round(saleCogs - returnCogs);
        var grossProfit = Round(netSales - costOfGoodsSold);

        var expenses = await _expenseRepository.GetListAsync(
            x => x.TenantId == tenantId && x.Status == ShopExpenseStatus.Posted && x.ExpenseDate >= from && x.ExpenseDate < toExclusive);
        var operatingExpenses = expenses.Sum(x => x.Amount);

        const decimal otherIncome = 0m; // No existing "other income" source/category - documented limitation.
        var netProfit = Round(grossProfit + otherIncome - operatingExpenses);

        var goodsReceipts = await _goodsReceiptRepository.GetListAsync(
            x => x.TenantId == tenantId && x.Status == ShopGoodsReceiptStatus.Completed && x.ReceiptDate >= from && x.ReceiptDate < toExclusive);
        var purchases = goodsReceipts.Sum(x => x.GrandTotal);

        var purchaseReturns = await _purchaseReturnRepository.GetListAsync(
            x => x.TenantId == tenantId && x.Status == ShopPurchaseReturnStatus.Completed && x.ReturnDate >= from && x.ReturnDate < toExclusive);
        var purchaseReturnTotal = purchaseReturns.Sum(x => x.GrandTotal);

        var netPurchases = Round(purchases - purchaseReturnTotal);

        var resolvedClosingInventoryValue = closingInventoryValue ?? await ComputeClosingInventoryValueAsync(tenantId);
        var openingInventoryValue = Round(resolvedClosingInventoryValue - netPurchases + costOfGoodsSold);

        var resultStatus = netProfit > 0
            ? ShopProfitLossResultStatus.Profit
            : netProfit == 0
                ? ShopProfitLossResultStatus.BreakEven
                : ShopProfitLossResultStatus.Loss;

        var core = new ShopProfitLossCoreResult
        {
            GrossSales = grossSales,
            SalesDiscounts = salesDiscounts,
            SalesTax = salesTax,
            SalesReturns = salesReturns,
            NetSales = netSales,
            CostOfGoodsSold = costOfGoodsSold,
            GrossProfit = grossProfit,
            OperatingExpenses = operatingExpenses,
            OtherIncome = otherIncome,
            NetProfit = netProfit,
            ResultStatus = resultStatus,
            OpeningInventoryValue = openingInventoryValue,
            NetPurchases = netPurchases,
            ClosingInventoryValue = resolvedClosingInventoryValue,
        };

        return new ShopProfitLossComputation
        {
            Core = core,
            Sales = sales,
            SaleItems = saleItems,
            SaleReturns = saleReturns,
            SaleReturnItems = saleReturnItems,
            Expenses = expenses,
        };
    }

    /// <summary>Live current batch/product valuation - not period-dependent. Compute once per request and pass into <see cref="ComputeAsync"/> when you need more than one period.</summary>
    public async Task<decimal> ComputeClosingInventoryValueAsync(Guid tenantId)
    {
        var nonBatchProducts = await _productRepository.GetListAsync(x => x.TenantId == tenantId && x.IsActive && !x.TrackBatch);
        var nonBatchValue = nonBatchProducts.Sum(x => x.CurrentStock * x.PurchasePrice);

        var batches = await _productBatchRepository.GetListAsync(x => x.TenantId == tenantId && x.AvailableQuantity > 0);
        var batchValue = batches.Sum(x => x.AvailableQuantity * x.UnitCost);

        return Round(nonBatchValue + batchValue);
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
