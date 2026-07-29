using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.ProductBatches;
using EHub.ShopManagement.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Content;

namespace EHub.ShopManagement.Reports;

public partial class ShopReportAppService
{
    // ------------------------------------------------------------------
    // 3. Current Stock Report
    // ------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopReports.Stock)]
    public async Task<ShopStockReportResultDto> GetStockReportAsync(GetShopStockReportInput input)
    {
        var tenantId = RequireTenant();
        var canViewCost = await CanViewCostAsync();

        var filtered = await BuildStockReportQueryAsync(tenantId, input);
        var totals = await ComputeStockTotalsAsync(tenantId, filtered, canViewCost);

        var sorting = ApplySorting(input.Sorting, "Name asc");
        var totalCount = await AsyncExecuter.CountAsync(filtered);
        var paged = await AsyncExecuter.ToListAsync(ApplyDynamicSort(filtered, sorting).Skip(input.SkipCount).Take(input.MaxResultCount));
        var items = await ProjectStockItemsAsync(tenantId, paged, canViewCost);

        return new ShopStockReportResultDto { Items = items, TotalCount = totalCount, Totals = totals };
    }

    [Authorize(EHubPermissions.ShopReports.Stock), Authorize(EHubPermissions.ShopReports.Export)]
    public async Task<IRemoteStreamContent> ExportStockReportAsync(GetShopStockReportInput input, ShopReportExportFormat format)
    {
        var tenantId = RequireTenant();
        var canViewCost = await CanViewCostAsync();
        var filtered = await BuildStockReportQueryAsync(tenantId, input);
        var totals = await ComputeStockTotalsAsync(tenantId, filtered, canViewCost);

        var count = await AsyncExecuter.CountAsync(filtered);
        EnsureWithinExportLimit(count);

        var all = await AsyncExecuter.ToListAsync(ApplyDynamicSort(filtered, ApplySorting(input.Sorting, "Name asc")));
        var items = await ProjectStockItemsAsync(tenantId, all, canViewCost);
        var setting = await GetSettingAsync(tenantId);

        var columns = new List<ShopReportExportColumn>
        {
            new("Product Code", "productCode"), new("Product", "productName"), new("Category", "categoryName"), new("Unit", "unitName"),
            new("Current Stock", "currentStock"), new("Reorder Level", "reorderLevel"), new("Status", "stockStatus"),
        };
        if (canViewCost)
        {
            columns.Add(new("Average Cost", "averageCost"));
            columns.Add(new("Stock Value", "stockValue"));
        }
        columns.Add(new("Sale Price", "salePrice"));
        if (canViewCost)
        {
            columns.Add(new("Potential Sale Value", "potentialSaleValue"));
            columns.Add(new("Potential Margin", "potentialMargin"));
        }

        var request = new ShopReportExportRequest
        {
            ReportTitle = "Stock Report",
            ShopName = setting?.ShopDisplayName,
            ShopAddress = CombineAddress(setting),
            ShopPhone = setting?.Phone,
            GeneratedDate = Clock.Now,
            Columns = columns,
            Rows = items.Select(x => new Dictionary<string, object?>
            {
                ["productCode"] = x.ProductCode, ["productName"] = x.ProductName, ["categoryName"] = x.CategoryName, ["unitName"] = x.UnitName,
                ["currentStock"] = x.CurrentStock, ["reorderLevel"] = x.ReorderLevel, ["stockStatus"] = x.StockStatus.ToString(),
                ["averageCost"] = x.AverageCost, ["stockValue"] = x.StockValue, ["salePrice"] = x.SalePrice,
                ["potentialSaleValue"] = x.PotentialSaleValue, ["potentialMargin"] = x.PotentialMargin,
            }).ToList(),
            TotalsLines = BuildStockTotalsLines(totals, canViewCost),
        };

        return await _exportService.ExportAsync(request, format);
    }

    private static List<(string, string)> BuildStockTotalsLines(ShopStockReportTotalsDto totals, bool canViewCost)
    {
        var lines = new List<(string, string)>
        {
            ("Total Products", totals.TotalProducts.ToString()), ("In Stock", totals.InStockProducts.ToString()),
            ("Low Stock", totals.LowStockProducts.ToString()), ("Out of Stock", totals.OutOfStockProducts.ToString()),
            ("Total Stock Quantity", totals.TotalStockQuantity.ToString("N4")),
        };
        if (canViewCost)
        {
            lines.Add(("Total Stock Value", (totals.TotalStockValue ?? 0).ToString("N2")));
            lines.Add(("Total Potential Sale Value", totals.TotalPotentialSaleValue.ToString("N2")));
            lines.Add(("Total Potential Margin", (totals.TotalPotentialMargin ?? 0).ToString("N2")));
        }
        return lines;
    }

    private async Task<IQueryable<ShopProduct>> BuildStockReportQueryAsync(Guid tenantId, GetShopStockReportInput input)
    {
        var productQuery = (await _productRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!input.IncludeInactiveProducts) productQuery = productQuery.Where(x => x.IsActive);
        if (input.ProductId.HasValue) productQuery = productQuery.Where(x => x.Id == input.ProductId.Value);
        if (input.ProductCategoryId.HasValue) productQuery = productQuery.Where(x => x.CategoryId == input.ProductCategoryId.Value);
        if (input.UnitId.HasValue) productQuery = productQuery.Where(x => x.UnitId == input.UnitId.Value);
        if (input.BatchTracked.HasValue) productQuery = productQuery.Where(x => x.TrackBatch == input.BatchTracked.Value);
        if (input.ExpiryTracked.HasValue) productQuery = productQuery.Where(x => x.TrackExpiry == input.ExpiryTracked.Value);
        if (input.MinimumStock.HasValue) productQuery = productQuery.Where(x => x.CurrentStock >= input.MinimumStock.Value);
        if (input.MaximumStock.HasValue) productQuery = productQuery.Where(x => x.CurrentStock <= input.MaximumStock.Value);

        if (input.SupplierId.HasValue)
        {
            var batchQuery = (await _productBatchRepository.GetQueryableAsync()).AsNoTracking();
            var productIds = batchQuery.Where(b => b.TenantId == tenantId && b.SupplierId == input.SupplierId.Value).Select(b => b.ProductId);
            productQuery = productQuery.Where(x => productIds.Contains(x.Id));
        }

        switch (input.StockStatus)
        {
            case ShopStockReportStatus.InStock:
                productQuery = productQuery.Where(x => x.CurrentStock > x.ReorderLevel);
                break;
            case ShopStockReportStatus.LowStock:
                productQuery = productQuery.Where(x => x.CurrentStock > 0 && x.CurrentStock <= x.ReorderLevel);
                break;
            case ShopStockReportStatus.OutOfStock:
                productQuery = productQuery.Where(x => x.CurrentStock <= 0);
                break;
            case ShopStockReportStatus.NegativeStock:
                productQuery = productQuery.Where(x => x.CurrentStock < 0);
                break;
        }

        if (input.StockStatus == ShopStockReportStatus.All)
        {
            if (input.InStockOnly) productQuery = productQuery.Where(x => x.CurrentStock > x.ReorderLevel);
            if (input.LowStockOnly) productQuery = productQuery.Where(x => x.CurrentStock > 0 && x.CurrentStock <= x.ReorderLevel);
            if (input.OutOfStockOnly) productQuery = productQuery.Where(x => x.CurrentStock <= 0);
        }

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var term = input.Filter;
            productQuery = productQuery.Where(x => x.Code.Contains(term) || x.Name.Contains(term) || (x.Barcode != null && x.Barcode.Contains(term)));
        }

        return productQuery;
    }

    private async Task<List<ShopStockReportItemDto>> ProjectStockItemsAsync(Guid tenantId, List<ShopProduct> products, bool canViewCost)
    {
        if (products.Count == 0) return new List<ShopStockReportItemDto>();

        var categoryIds = products.Select(x => x.CategoryId).Distinct().ToList();
        var unitIds = products.Select(x => x.UnitId).Distinct().ToList();
        var productIds = products.Select(x => x.Id).ToList();

        var categoryQuery = (await _categoryRepository.GetQueryableAsync()).AsNoTracking();
        var categories = (await AsyncExecuter.ToListAsync(categoryQuery.Where(x => categoryIds.Contains(x.Id)))).ToDictionary(x => x.Id);

        var unitQuery = (await _unitRepository.GetQueryableAsync()).AsNoTracking();
        var units = (await AsyncExecuter.ToListAsync(unitQuery.Where(x => unitIds.Contains(x.Id)))).ToDictionary(x => x.Id);

        var batchQuery = (await _productBatchRepository.GetQueryableAsync()).AsNoTracking();
        var batchAggregates = await AsyncExecuter.ToListAsync(
            batchQuery.Where(x => x.TenantId == tenantId && productIds.Contains(x.ProductId) && x.AvailableQuantity > 0)
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ActiveCount = g.Count(),
                    TotalAvailable = g.Sum(x => x.AvailableQuantity),
                    TotalValue = g.Sum(x => x.AvailableQuantity * x.UnitCost),
                    NearExpiryQty = g.Where(x => x.Status == ShopProductBatchStatus.NearExpiry).Sum(x => x.AvailableQuantity),
                    ExpiredQty = g.Where(x => x.Status == ShopProductBatchStatus.Expired).Sum(x => x.AvailableQuantity),
                }));
        var batchDict = batchAggregates.ToDictionary(x => x.ProductId);

        return products.Select(product =>
        {
            categories.TryGetValue(product.CategoryId, out var category);
            units.TryGetValue(product.UnitId, out var unit);
            batchDict.TryGetValue(product.Id, out var batchAgg);

            decimal? averageCost = null;
            decimal? stockValue = null;

            if (canViewCost)
            {
                if (product.TrackBatch && batchAgg != null && batchAgg.TotalAvailable > 0)
                {
                    averageCost = Math.Round(batchAgg.TotalValue / batchAgg.TotalAvailable, 2);
                    stockValue = Math.Round(batchAgg.TotalValue, 2);
                }
                else
                {
                    averageCost = product.PurchasePrice;
                    stockValue = Math.Round(product.CurrentStock * product.PurchasePrice, 2);
                }
            }

            var potentialSaleValue = Math.Round(product.CurrentStock * product.SalePrice, 2);
            var stockStatus = ComputeStockStatus(product.CurrentStock, product.ReorderLevel);

            return new ShopStockReportItemDto
            {
                ProductId = product.Id,
                ProductCode = product.Code,
                Barcode = product.Barcode,
                ProductName = product.Name,
                CategoryId = product.CategoryId,
                CategoryName = category?.Name ?? string.Empty,
                UnitId = product.UnitId,
                UnitName = unit?.Name ?? string.Empty,
                UnitShortName = unit?.ShortName ?? string.Empty,
                CurrentStock = product.CurrentStock,
                ReorderLevel = product.ReorderLevel,
                StockStatus = stockStatus,
                PurchasePrice = canViewCost ? product.PurchasePrice : null,
                AverageCost = averageCost,
                SalePrice = product.SalePrice,
                StockValue = stockValue,
                PotentialSaleValue = potentialSaleValue,
                PotentialMargin = canViewCost && stockValue.HasValue ? Math.Round(potentialSaleValue - stockValue.Value, 2) : null,
                TrackBatch = product.TrackBatch,
                TrackExpiry = product.TrackExpiry,
                ActiveBatchCount = batchAgg?.ActiveCount ?? 0,
                NearExpiryQuantity = batchAgg?.NearExpiryQty ?? 0,
                ExpiredQuantity = batchAgg?.ExpiredQty ?? 0,
                IsActive = product.IsActive,
            };
        }).ToList();
    }

    private async Task<ShopStockReportTotalsDto> ComputeStockTotalsAsync(Guid tenantId, IQueryable<ShopProduct> filtered, bool canViewCost)
    {
        var agg = await AsyncExecuter.FirstOrDefaultAsync(
            filtered.GroupBy(x => 1).Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(x => x.IsActive),
                InStock = g.Count(x => x.CurrentStock > x.ReorderLevel),
                LowStock = g.Count(x => x.CurrentStock > 0 && x.CurrentStock <= x.ReorderLevel),
                OutOfStock = g.Count(x => x.CurrentStock <= 0),
                Negative = g.Count(x => x.CurrentStock < 0),
                TotalQuantity = g.Sum(x => x.CurrentStock),
                TotalPotentialSaleValue = g.Sum(x => x.CurrentStock * x.SalePrice),
            }));

        if (agg == null || agg.Total == 0) return new ShopStockReportTotalsDto();

        var totals = new ShopStockReportTotalsDto
        {
            TotalProducts = agg.Total,
            ActiveProducts = agg.Active,
            InStockProducts = agg.InStock,
            LowStockProducts = agg.LowStock,
            OutOfStockProducts = agg.OutOfStock,
            NegativeStockProducts = agg.Negative,
            TotalStockQuantity = agg.TotalQuantity,
            TotalPotentialSaleValue = Math.Round(agg.TotalPotentialSaleValue, 2),
        };

        if (!canViewCost) return totals;

        var productIds = await AsyncExecuter.ToListAsync(filtered.Select(x => x.Id));
        var batchTrackedIds = await AsyncExecuter.ToListAsync(filtered.Where(x => x.TrackBatch).Select(x => x.Id));
        var nonBatchValue = await AsyncExecuter.SumAsync(
            filtered.Where(x => !x.TrackBatch).Select(x => (decimal?)(x.CurrentStock * x.PurchasePrice))) ?? 0m;

        var batchQuery = (await _productBatchRepository.GetQueryableAsync()).AsNoTracking();
        var batchValue = await AsyncExecuter.SumAsync(
            batchQuery.Where(x => x.TenantId == tenantId && batchTrackedIds.Contains(x.ProductId) && x.AvailableQuantity > 0)
                .Select(x => (decimal?)(x.AvailableQuantity * x.UnitCost))) ?? 0m;

        var totalStockValue = Math.Round(nonBatchValue + batchValue, 2);
        totals.TotalStockValue = totalStockValue;
        totals.TotalPotentialMargin = Math.Round(totals.TotalPotentialSaleValue - totalStockValue, 2);

        return totals;
    }

    private static ShopStockReportStatus ComputeStockStatus(decimal currentStock, decimal reorderLevel)
    {
        if (currentStock < 0) return ShopStockReportStatus.NegativeStock;
        if (currentStock == 0) return ShopStockReportStatus.OutOfStock;
        return currentStock <= reorderLevel ? ShopStockReportStatus.LowStock : ShopStockReportStatus.InStock;
    }
}
