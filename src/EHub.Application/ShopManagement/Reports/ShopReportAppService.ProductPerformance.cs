using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.ProductBatches;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.PurchaseReturns;
using EHub.ShopManagement.SaleReturns;
using EHub.ShopManagement.Sales;
using EHub.ShopManagement.StockTransactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Content;

namespace EHub.ShopManagement.Reports;

public partial class ShopReportAppService
{
    // ------------------------------------------------------------------
    // 14. Product Performance Report (quantities from the authoritative stock ledger; no profit shown)
    // ------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopReports.ProductPerformance)]
    public async Task<ShopProductPerformanceReportResultDto> GetProductPerformanceReportAsync(GetShopProductPerformanceReportInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);

        var items = await BuildProductPerformanceItemsAsync(tenantId, range, input);

        if (input.SoldOnly) items = items.Where(x => x.SaleQuantity > 0).ToList();
        if (input.PurchasedOnly) items = items.Where(x => x.PurchasedQuantity > 0).ToList();

        var totals = ComputeProductPerformanceTotals(items);
        var sorted = SortProductPerformance(items, input.Sorting);
        var page = sorted.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

        return new ShopProductPerformanceReportResultDto { Items = page, TotalCount = sorted.Count, Totals = totals };
    }

    [Authorize(EHubPermissions.ShopReports.ProductPerformance), Authorize(EHubPermissions.ShopReports.Export)]
    public async Task<IRemoteStreamContent> ExportProductPerformanceReportAsync(GetShopProductPerformanceReportInput input, ShopReportExportFormat format)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);

        var items = await BuildProductPerformanceItemsAsync(tenantId, range, input);
        if (input.SoldOnly) items = items.Where(x => x.SaleQuantity > 0).ToList();
        if (input.PurchasedOnly) items = items.Where(x => x.PurchasedQuantity > 0).ToList();

        EnsureWithinExportLimit(items.Count);
        var sorted = SortProductPerformance(items, input.Sorting);
        var totals = ComputeProductPerformanceTotals(sorted);

        var setting = await GetSettingAsync(tenantId);
        var request = new ShopReportExportRequest
        {
            ReportTitle = "Product Performance Report",
            ShopName = setting?.ShopDisplayName,
            ShopAddress = CombineAddress(setting),
            ShopPhone = setting?.Phone,
            DateFrom = range.From,
            DateTo = range.ToExclusive.AddDays(-1),
            GeneratedDate = Clock.Now,
            Columns = new List<ShopReportExportColumn>
            {
                new("Product", "productName"), new("Category", "categoryName"), new("Opening Stock", "openingStock"),
                new("Purchased", "purchasedQuantity"), new("Sold", "saleQuantity"), new("Sale Returns", "saleReturnQuantity"),
                new("Closing Stock", "closingStock"), new("Net Sales Amount", "netSalesAmount"), new("Current Stock", "currentStock"),
            },
            Rows = sorted.Select(x => new Dictionary<string, object?>
            {
                ["productName"] = x.ProductName, ["categoryName"] = x.CategoryName, ["openingStock"] = x.OpeningStock,
                ["purchasedQuantity"] = x.PurchasedQuantity, ["saleQuantity"] = x.SaleQuantity, ["saleReturnQuantity"] = x.SaleReturnQuantity,
                ["closingStock"] = x.ClosingStock, ["netSalesAmount"] = x.NetSalesAmount, ["currentStock"] = x.CurrentStock,
            }).ToList(),
            TotalsLines = new List<(string, string)>
            {
                ("Products", totals.ProductCount.ToString()), ("Total Sale Quantity", totals.TotalSaleQuantity.ToString("N2")),
                ("Total Net Sales Amount", totals.TotalNetSalesAmount.ToString("N2")), ("Total Current Stock", totals.TotalCurrentStock.ToString("N2")),
            },
        };

        return await _exportService.ExportAsync(request, format);
    }

    private async Task<List<ShopProductPerformanceReportItemDto>> BuildProductPerformanceItemsAsync(Guid tenantId, ShopReportDateRange range, GetShopProductPerformanceReportInput input)
    {
        var canViewCost = await CanViewCostAsync();

        var productQuery = (await _productRepository.GetQueryableAsync()).AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!input.IncludeInactiveProducts) productQuery = productQuery.Where(x => x.IsActive);
        if (input.ProductId.HasValue) productQuery = productQuery.Where(x => x.Id == input.ProductId.Value);
        if (input.ProductCategoryId.HasValue) productQuery = productQuery.Where(x => x.CategoryId == input.ProductCategoryId.Value);
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var term = input.Filter;
            productQuery = productQuery.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
        }

        if (input.SupplierId.HasValue)
        {
            var batchProductQuery = (await _productBatchRepository.GetQueryableAsync()).AsNoTracking();
            var supplierProductIds = await AsyncExecuter.ToListAsync(
                batchProductQuery.Where(x => x.TenantId == tenantId && x.SupplierId == input.SupplierId.Value).Select(x => x.ProductId).Distinct());
            productQuery = productQuery.Where(x => supplierProductIds.Contains(x.Id));
        }

        var products = await AsyncExecuter.ToListAsync(productQuery);
        if (products.Count == 0) return new List<ShopProductPerformanceReportItemDto>();

        var productIds = products.Select(x => x.Id).ToList();

        var categoryQuery = (await _categoryRepository.GetQueryableAsync()).AsNoTracking();
        var categoryDict = (await AsyncExecuter.ToListAsync(categoryQuery.Where(x => x.TenantId == tenantId).Select(x => new { x.Id, x.Name })))
            .ToDictionary(x => x.Id, x => x.Name);

        var unitQuery = (await _unitRepository.GetQueryableAsync()).AsNoTracking();
        var unitDict = (await AsyncExecuter.ToListAsync(unitQuery.Where(x => x.TenantId == tenantId).Select(x => new { x.Id, x.Name })))
            .ToDictionary(x => x.Id, x => x.Name);

        // In-range movement by ledger transaction type - the stock ledger is the authoritative source for quantities.
        var stockTxQuery = (await _stockTransactionRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && productIds.Contains(x.ProductId));

        var inRangeByType = await AsyncExecuter.ToListAsync(
            stockTxQuery.Where(x => x.TransactionDate >= range.From && x.TransactionDate < range.ToExclusive)
                .GroupBy(x => new { x.ProductId, x.TransactionType })
                .Select(g => new { g.Key.ProductId, g.Key.TransactionType, In = g.Sum(x => x.QuantityIn), Out = g.Sum(x => x.QuantityOut) }));

        var netMovement = await AsyncExecuter.ToListAsync(
            stockTxQuery.Where(x => x.TransactionDate >= range.From && x.TransactionDate < range.ToExclusive)
                .GroupBy(x => x.ProductId)
                .Select(g => new { ProductId = g.Key, Net = g.Sum(x => x.QuantityIn) - g.Sum(x => x.QuantityOut) }));
        var netMovementDict = netMovement.ToDictionary(x => x.ProductId, x => x.Net);

        var beforeRangeRows = await AsyncExecuter.ToListAsync(
            stockTxQuery.Where(x => x.TransactionDate < range.From)
                .OrderBy(x => x.ProductId).ThenBy(x => x.TransactionDate).ThenBy(x => x.CreationTime)
                .Select(x => new { x.ProductId, x.BalanceQuantity }));
        var openingStockDict = beforeRangeRows.GroupBy(x => x.ProductId).ToDictionary(g => g.Key, g => g.Last().BalanceQuantity);

        var saleAgg = await AsyncExecuter.ToListAsync(
            (from item in (await _saleItemRepository.GetQueryableAsync()).AsNoTracking()
             join sale in (await _saleRepository.GetQueryableAsync()).AsNoTracking() on item.SaleId equals sale.Id
             where sale.TenantId == tenantId && sale.Status == ShopSaleStatus.Completed
                && sale.SaleDate >= range.From && sale.SaleDate < range.ToExclusive && productIds.Contains(item.ProductId)
             group item by item.ProductId into g
             select new { ProductId = g.Key, Amount = g.Sum(x => x.LineTotal) }));
        var saleAmountDict = saleAgg.ToDictionary(x => x.ProductId, x => x.Amount);

        var saleReturnAgg = await AsyncExecuter.ToListAsync(
            (from item in (await _saleReturnItemRepository.GetQueryableAsync()).AsNoTracking()
             join ret in (await _saleReturnRepository.GetQueryableAsync()).AsNoTracking() on item.SaleReturnId equals ret.Id
             where ret.TenantId == tenantId && ret.Status == ShopSaleReturnStatus.Completed
                && ret.ReturnDate >= range.From && ret.ReturnDate < range.ToExclusive && productIds.Contains(item.ProductId)
             group item by item.ProductId into g
             select new { ProductId = g.Key, Amount = g.Sum(x => x.LineTotal) }));
        var saleReturnAmountDict = saleReturnAgg.ToDictionary(x => x.ProductId, x => x.Amount);

        Dictionary<Guid, decimal> purchaseAmountDict = new();
        Dictionary<Guid, decimal> purchaseReturnAmountDict = new();
        if (canViewCost)
        {
            var purchaseAgg = await AsyncExecuter.ToListAsync(
                (from item in (await _goodsReceiptItemRepository.GetQueryableAsync()).AsNoTracking()
                 join gr in (await _goodsReceiptRepository.GetQueryableAsync()).AsNoTracking() on item.GoodsReceiptId equals gr.Id
                 where gr.TenantId == tenantId && gr.Status == ShopGoodsReceiptStatus.Completed
                    && gr.ReceiptDate >= range.From && gr.ReceiptDate < range.ToExclusive && productIds.Contains(item.ProductId)
                 group item by item.ProductId into g
                 select new { ProductId = g.Key, Amount = g.Sum(x => x.LineTotal) }));
            purchaseAmountDict = purchaseAgg.ToDictionary(x => x.ProductId, x => x.Amount);

            var purchaseReturnAgg = await AsyncExecuter.ToListAsync(
                (from item in (await _purchaseReturnItemRepository.GetQueryableAsync()).AsNoTracking()
                 join ret in (await _purchaseReturnRepository.GetQueryableAsync()).AsNoTracking() on item.PurchaseReturnId equals ret.Id
                 where ret.TenantId == tenantId && ret.Status == ShopPurchaseReturnStatus.Completed
                    && ret.ReturnDate >= range.From && ret.ReturnDate < range.ToExclusive && productIds.Contains(item.ProductId)
                 group item by item.ProductId into g
                 select new { ProductId = g.Key, Amount = g.Sum(x => x.LineTotal) }));
            purchaseReturnAmountDict = purchaseReturnAgg.ToDictionary(x => x.ProductId, x => x.Amount);
        }

        Dictionary<Guid, decimal> batchValueDict = new();
        if (canViewCost)
        {
            var batchQuery = (await _productBatchRepository.GetQueryableAsync()).AsNoTracking();
            var batchAgg = await AsyncExecuter.ToListAsync(
                batchQuery.Where(x => x.TenantId == tenantId && productIds.Contains(x.ProductId) && x.AvailableQuantity > 0)
                    .GroupBy(x => x.ProductId)
                    .Select(g => new { ProductId = g.Key, Value = g.Sum(x => x.AvailableQuantity * x.UnitCost) }));
            batchValueDict = batchAgg.ToDictionary(x => x.ProductId, x => x.Value);
        }

        var typeLookup = inRangeByType.ToLookup(x => x.ProductId);

        var items = new List<ShopProductPerformanceReportItemDto>();
        foreach (var product in products)
        {
            decimal purchasedQty = 0, purchaseReturnQty = 0, saleQty = 0, saleReturnQty = 0, adjIn = 0, adjOut = 0;
            foreach (var row in typeLookup[product.Id])
            {
                switch (row.TransactionType)
                {
                    case ShopStockTransactionType.Purchase: purchasedQty += row.In; break;
                    case ShopStockTransactionType.PurchaseReturn: purchaseReturnQty += row.Out; break;
                    case ShopStockTransactionType.Sale: saleQty += row.Out; break;
                    case ShopStockTransactionType.SaleReturn: saleReturnQty += row.In; break;
                    case ShopStockTransactionType.StockAdjustmentIncrease: adjIn += row.In; break;
                    case ShopStockTransactionType.StockAdjustmentDecrease:
                    case ShopStockTransactionType.Damaged:
                    case ShopStockTransactionType.Expired:
                    case ShopStockTransactionType.FreeSample:
                        adjOut += row.Out;
                        break;
                }
            }

            var openingStock = openingStockDict.GetValueOrDefault(product.Id, 0);
            var closingStock = openingStock + netMovementDict.GetValueOrDefault(product.Id, 0);
            var grossSales = saleAmountDict.GetValueOrDefault(product.Id, 0);
            var saleReturnAmount = saleReturnAmountDict.GetValueOrDefault(product.Id, 0);

            decimal? purchaseAmount = canViewCost ? purchaseAmountDict.GetValueOrDefault(product.Id, 0) : null;
            decimal? purchaseReturnAmount = canViewCost ? purchaseReturnAmountDict.GetValueOrDefault(product.Id, 0) : null;

            decimal? currentStockValue = null;
            if (canViewCost)
            {
                currentStockValue = product.TrackBatch
                    ? batchValueDict.GetValueOrDefault(product.Id, 0)
                    : Math.Round(product.CurrentStock * product.PurchasePrice, 2);
            }

            items.Add(new ShopProductPerformanceReportItemDto
            {
                ProductId = product.Id,
                ProductCode = product.Code,
                ProductName = product.Name,
                CategoryName = categoryDict.GetValueOrDefault(product.CategoryId, string.Empty),
                UnitName = unitDict.GetValueOrDefault(product.UnitId, string.Empty),
                OpeningStock = openingStock,
                PurchasedQuantity = purchasedQty,
                PurchaseReturnQuantity = purchaseReturnQty,
                SaleQuantity = saleQty,
                SaleReturnQuantity = saleReturnQty,
                AdjustmentInQuantity = adjIn,
                AdjustmentOutQuantity = adjOut,
                ClosingStock = closingStock,
                GrossSalesAmount = grossSales,
                SaleReturnAmount = saleReturnAmount,
                NetSalesAmount = Math.Round(grossSales - saleReturnAmount, 2),
                PurchaseAmount = purchaseAmount,
                PurchaseReturnAmount = purchaseReturnAmount,
                NetPurchaseAmount = canViewCost ? Math.Round((purchaseAmount ?? 0) - (purchaseReturnAmount ?? 0), 2) : null,
                CurrentStock = product.CurrentStock,
                CurrentStockValue = currentStockValue,
                AverageSalePrice = saleQty > 0 ? Math.Round(grossSales / saleQty, 2) : null,
                AveragePurchaseCost = canViewCost && purchasedQty > 0 ? Math.Round((purchaseAmount ?? 0) / purchasedQty, 2) : null,
            });
        }

        return items;
    }

    private static List<ShopProductPerformanceReportItemDto> SortProductPerformance(List<ShopProductPerformanceReportItemDto> items, string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting)) return items.OrderByDescending(x => x.NetSalesAmount).ToList();
        return sorting.Trim().ToLowerInvariant() switch
        {
            "netsalesamount" => items.OrderBy(x => x.NetSalesAmount).ToList(),
            "netsalesamount desc" => items.OrderByDescending(x => x.NetSalesAmount).ToList(),
            "salequantity" => items.OrderBy(x => x.SaleQuantity).ToList(),
            "salequantity desc" => items.OrderByDescending(x => x.SaleQuantity).ToList(),
            "productname" => items.OrderBy(x => x.ProductName).ToList(),
            "productname desc" => items.OrderByDescending(x => x.ProductName).ToList(),
            _ => items.OrderByDescending(x => x.NetSalesAmount).ToList(),
        };
    }

    private static ShopProductPerformanceReportTotalsDto ComputeProductPerformanceTotals(List<ShopProductPerformanceReportItemDto> items) => new()
    {
        ProductCount = items.Count,
        TotalPurchasedQuantity = items.Sum(x => x.PurchasedQuantity),
        TotalSaleQuantity = items.Sum(x => x.SaleQuantity),
        TotalNetSalesAmount = items.Sum(x => x.NetSalesAmount),
        TotalNetPurchaseAmount = items.Any(x => x.NetPurchaseAmount.HasValue) ? items.Sum(x => x.NetPurchaseAmount ?? 0) : null,
        TotalCurrentStock = items.Sum(x => x.CurrentStock),
        TotalCurrentStockValue = items.Any(x => x.CurrentStockValue.HasValue) ? items.Sum(x => x.CurrentStockValue ?? 0) : null,
    };
}
