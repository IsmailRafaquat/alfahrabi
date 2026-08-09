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
/// This is a Domain-layer service, so it deliberately queries only through <c>GetQueryableAsync</c>
/// plus base <see cref="System.Linq"/> operators (no EF Core-specific AsNoTracking/async query
/// executers, which aren't referenced from the Domain project). Line-item COGS is computed by
/// joining SaleItems/SaleReturnItems straight to the already-filtered Sales/SaleReturns queryable -
/// one indexed SQL JOIN - rather than first materializing a list of parent IDs into memory and
/// re-querying with a Contains(list), which forces a large SQL "IN (...)" and an extra round trip
/// worth of row data for busy periods.
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

    /// <summary>
    /// Computes one period's figures and returns the lightweight row projections alongside them
    /// (see <see cref="ShopProfitLossSaleRow"/> and friends), so a caller building several views of
    /// the same period (statement, trend, product contribution, expense breakdown) can reuse this one
    /// set of loaded data instead of re-querying per view. Only the columns those views actually read
    /// are selected - never the full Sale/SaleItem/SaleReturn/SaleReturnItem/Expense entity - so this
    /// stays light even for a period with a lot of transactions.
    /// <paramref name="closingInventoryValue"/> is optional: it is NOT period-dependent (it's always
    /// the live current valuation), so a caller that needs several periods in one request (e.g.
    /// current + previous-period comparison) should compute it once via
    /// <see cref="ComputeClosingInventoryValueAsync"/> and pass it in every time, rather than paying
    /// for the full product/batch scan on every call.
    /// <paramref name="includeInventoryReconciliation"/> lets a caller whose viewer has no ViewCost
    /// permission (so OpeningInventoryValue/NetPurchases/ClosingInventoryValue never reach the DTO
    /// anyway) skip the goods-receipt/purchase-return queries and the closing-inventory product/batch
    /// scan entirely, rather than paying for figures that get thrown away downstream.
    /// </summary>
    public async Task<ShopProfitLossComputation> ComputeDetailedAsync(Guid tenantId, DateTime from, DateTime toExclusive, decimal? closingInventoryValue = null, bool includeInventoryReconciliation = true)
    {
        var salesQuery = (await _saleRepository.GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.Status == ShopSaleStatus.Completed && x.SaleDate >= from && x.SaleDate < toExclusive);
        var sales = salesQuery
            .Select(x => new ShopProfitLossSaleRow(x.Id, x.SaleDate, x.SubTotal, x.DiscountAmount, x.TaxAmount))
            .ToList();

        var grossSales = sales.Sum(x => x.SubTotal);
        var salesDiscounts = sales.Sum(x => x.DiscountAmount);
        var salesTax = sales.Sum(x => x.TaxAmount);

        var saleReturnsQuery = (await _saleReturnRepository.GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.Status == ShopSaleReturnStatus.Completed && x.ReturnDate >= from && x.ReturnDate < toExclusive);
        var saleReturns = saleReturnsQuery
            .Select(x => new ShopProfitLossSaleReturnRow(x.Id, x.ReturnDate, x.GrandTotal))
            .ToList();
        var salesReturns = saleReturns.Sum(x => x.GrandTotal);

        var netSales = Round(grossSales - salesDiscounts - salesReturns);

        // Joined straight to salesQuery/saleReturnsQuery instead of materializing an ID list and
        // re-querying with Contains - one indexed SQL JOIN instead of a large "IN (...)" clause.
        var saleItems = sales.Count == 0
            ? new List<ShopProfitLossSaleItemRow>()
            : (await _saleItemRepository.GetQueryableAsync())
                .Where(i => i.TenantId == tenantId)
                .Join(salesQuery, i => i.SaleId, s => s.Id, (i, s) => i)
                .Select(i => new ShopProfitLossSaleItemRow(i.SaleId, i.ProductId, i.Quantity, i.UnitCostSnapshot, i.LineSubTotal, i.DiscountAmount))
                .ToList();
        var saleCogs = saleItems.Sum(i => i.Quantity * i.UnitCostSnapshot);

        var saleReturnItems = saleReturns.Count == 0
            ? new List<ShopProfitLossSaleReturnItemRow>()
            : (await _saleReturnItemRepository.GetQueryableAsync())
                .Where(i => i.TenantId == tenantId)
                .Join(saleReturnsQuery, i => i.SaleReturnId, r => r.Id, (i, r) => i)
                .Select(i => new ShopProfitLossSaleReturnItemRow(i.SaleReturnId, i.ProductId, i.ReturnQuantity, i.UnitCostSnapshot, i.LineSubTotal, i.DiscountAmount))
                .ToList();
        var returnCogs = saleReturnItems.Sum(i => i.ReturnQuantity * i.UnitCostSnapshot);

        var costOfGoodsSold = Round(saleCogs - returnCogs);
        var grossProfit = Round(netSales - costOfGoodsSold);

        var expenses = (await _expenseRepository.GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.Status == ShopExpenseStatus.Posted && x.ExpenseDate >= from && x.ExpenseDate < toExclusive)
            .Select(x => new ShopProfitLossExpenseRow(x.ExpenseDate, x.ExpenseCategoryId, x.Amount))
            .ToList();
        var operatingExpenses = expenses.Sum(x => x.Amount);

        const decimal otherIncome = 0m; // No existing "other income" source/category - documented limitation.
        var netProfit = Round(grossProfit + otherIncome - operatingExpenses);

        var (netPurchases, resolvedClosingInventoryValue, openingInventoryValue) = includeInventoryReconciliation
            ? await ComputeInventoryReconciliationAsync(tenantId, from, toExclusive, costOfGoodsSold, closingInventoryValue)
            : (0m, 0m, 0m);

        var core = new ShopProfitLossCoreResult
        {
            GrossSales = grossSales,
            SalesDiscounts = salesDiscounts,
            SalesTax = salesTax,
            SalesReturns = salesReturns,
            NetSales = netSales,
            CostOfGoodsSoldBeforeReturns = Round(saleCogs),
            ReturnedCostOfGoodsSold = Round(returnCogs),
            CostOfGoodsSold = costOfGoodsSold,
            GrossProfit = grossProfit,
            OperatingExpenses = operatingExpenses,
            OtherIncome = otherIncome,
            NetProfit = netProfit,
            ResultStatus = ResultStatusFor(netProfit),
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

    /// <summary>
    /// Lightweight alternative to <see cref="ComputeDetailedAsync"/> for callers that only need the
    /// aggregate revenue/cost/expense figures - the summary endpoint, the exported statement, the
    /// previous-period side of a comparison, and the profit/loss notification check - and never touch
    /// the row-level Sale/SaleItem/SaleReturn/Expense detail that ComputeDetailedAsync loads for the
    /// trend/expense-breakdown/product-contribution sections. Every query here sums server-side
    /// instead of returning rows, so nothing beyond the totals themselves ever crosses into memory.
    /// <paramref name="includeInventoryReconciliation"/> mirrors the same flag on
    /// <see cref="ComputeDetailedAsync"/>: leave it false (the default) unless the caller actually
    /// surfaces OpeningInventoryValue/NetPurchases/ClosingInventoryValue, since turning it on adds
    /// back the goods-receipt/purchase-return sums and the closing-inventory product/batch scan.
    /// </summary>
    public async Task<ShopProfitLossCoreResult> ComputeAggregateAsync(Guid tenantId, DateTime from, DateTime toExclusive, bool includeInventoryReconciliation = false, decimal? closingInventoryValue = null)
    {
        // Nothing below fetches a single Sale/SaleReturn row: the three sales columns are summed
        // server-side in one grouped query, and SaleItems/SaleReturnItems are joined straight to the
        // filtered Sales/SaleReturns queryable for their COGS sum - so this whole method is five
        // aggregate-only round trips that each return one scalar row, never period-sized row data.
        var salesQuery = (await _saleRepository.GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.Status == ShopSaleStatus.Completed && x.SaleDate >= from && x.SaleDate < toExclusive);

        var salesAgg = salesQuery
            .GroupBy(_ => 1)
            .Select(g => new { GrossSales = g.Sum(x => x.SubTotal), SalesDiscounts = g.Sum(x => x.DiscountAmount), SalesTax = g.Sum(x => x.TaxAmount) })
            .FirstOrDefault();
        var grossSales = salesAgg?.GrossSales ?? 0m;
        var salesDiscounts = salesAgg?.SalesDiscounts ?? 0m;
        var salesTax = salesAgg?.SalesTax ?? 0m;

        var saleCogs = (await _saleItemRepository.GetQueryableAsync())
            .Where(i => i.TenantId == tenantId)
            .Join(salesQuery, i => i.SaleId, s => s.Id, (i, s) => i)
            .Sum(i => (decimal?)(i.Quantity * i.UnitCostSnapshot)) ?? 0m;

        var saleReturnsQuery = (await _saleReturnRepository.GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.Status == ShopSaleReturnStatus.Completed && x.ReturnDate >= from && x.ReturnDate < toExclusive);

        var salesReturns = saleReturnsQuery.Sum(x => (decimal?)x.GrandTotal) ?? 0m;

        var returnCogs = (await _saleReturnItemRepository.GetQueryableAsync())
            .Where(i => i.TenantId == tenantId)
            .Join(saleReturnsQuery, i => i.SaleReturnId, r => r.Id, (i, r) => i)
            .Sum(i => (decimal?)(i.ReturnQuantity * i.UnitCostSnapshot)) ?? 0m;

        var operatingExpenses = (await _expenseRepository.GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.Status == ShopExpenseStatus.Posted && x.ExpenseDate >= from && x.ExpenseDate < toExclusive)
            .Sum(x => (decimal?)x.Amount) ?? 0m;

        var netSales = Round(grossSales - salesDiscounts - salesReturns);
        var costOfGoodsSold = Round(saleCogs - returnCogs);
        var grossProfit = Round(netSales - costOfGoodsSold);
        const decimal otherIncome = 0m;
        var netProfit = Round(grossProfit + otherIncome - operatingExpenses);

        var (netPurchases, resolvedClosingInventoryValue, openingInventoryValue) = includeInventoryReconciliation
            ? await ComputeInventoryReconciliationAsync(tenantId, from, toExclusive, costOfGoodsSold, closingInventoryValue)
            : (0m, 0m, 0m);

        return new ShopProfitLossCoreResult
        {
            GrossSales = grossSales,
            SalesDiscounts = salesDiscounts,
            SalesTax = salesTax,
            SalesReturns = salesReturns,
            NetSales = netSales,
            CostOfGoodsSoldBeforeReturns = Round(saleCogs),
            ReturnedCostOfGoodsSold = Round(returnCogs),
            CostOfGoodsSold = costOfGoodsSold,
            GrossProfit = grossProfit,
            OperatingExpenses = operatingExpenses,
            OtherIncome = otherIncome,
            NetProfit = netProfit,
            ResultStatus = ResultStatusFor(netProfit),
            OpeningInventoryValue = openingInventoryValue,
            NetPurchases = netPurchases,
            ClosingInventoryValue = resolvedClosingInventoryValue,
        };
    }

    /// <summary>Goods receipts minus purchase returns for the period, plus the resulting opening/closing inventory reconciliation. Three plain, single-table Sum queries - shared by both compute methods above.</summary>
    private async Task<(decimal NetPurchases, decimal ClosingInventoryValue, decimal OpeningInventoryValue)> ComputeInventoryReconciliationAsync(
        Guid tenantId, DateTime from, DateTime toExclusive, decimal costOfGoodsSold, decimal? closingInventoryValue)
    {
        var purchases = (await _goodsReceiptRepository.GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.Status == ShopGoodsReceiptStatus.Completed && x.ReceiptDate >= from && x.ReceiptDate < toExclusive)
            .Sum(x => (decimal?)x.GrandTotal) ?? 0m;

        var purchaseReturnTotal = (await _purchaseReturnRepository.GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.Status == ShopPurchaseReturnStatus.Completed && x.ReturnDate >= from && x.ReturnDate < toExclusive)
            .Sum(x => (decimal?)x.GrandTotal) ?? 0m;

        var netPurchases = Round(purchases - purchaseReturnTotal);
        var resolvedClosingInventoryValue = closingInventoryValue ?? await ComputeClosingInventoryValueAsync(tenantId);
        var openingInventoryValue = Round(resolvedClosingInventoryValue - netPurchases + costOfGoodsSold);

        return (netPurchases, resolvedClosingInventoryValue, openingInventoryValue);
    }

    /// <summary>Live current batch/product valuation - not period-dependent. Compute once per request and pass into <see cref="ComputeDetailedAsync"/> or <see cref="ComputeAggregateAsync"/> when you need more than one period.</summary>
    public async Task<decimal> ComputeClosingInventoryValueAsync(Guid tenantId)
    {
        // Sums only, computed server-side - nothing needs individual product/batch rows here, so
        // there is no reason to materialize the whole active catalog into memory just to multiply
        // and add two columns.
        var nonBatchValue = (await _productRepository.GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.IsActive && !x.TrackBatch)
            .Sum(x => (decimal?)(x.CurrentStock * x.PurchasePrice)) ?? 0m;

        var batchValue = (await _productBatchRepository.GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.AvailableQuantity > 0)
            .Sum(x => (decimal?)(x.AvailableQuantity * x.UnitCost)) ?? 0m;

        return Round(nonBatchValue + batchValue);
    }

    private static ShopProfitLossResultStatus ResultStatusFor(decimal netProfit) =>
        netProfit > 0 ? ShopProfitLossResultStatus.Profit
        : netProfit == 0 ? ShopProfitLossResultStatus.BreakEven
        : ShopProfitLossResultStatus.Loss;

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
