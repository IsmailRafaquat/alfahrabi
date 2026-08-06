using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.ProductBatches;
using EHub.ShopManagement.StockTransactions;
using EHub.ShopManagement.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Content;

namespace EHub.ShopManagement.Reports;

public partial class ShopReportAppService
{
    // ------------------------------------------------------------------
    // 4. Stock Movement Report (ShopStockTransaction is the authoritative immutable stock ledger)
    // ------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopReports.StockMovements)]
    public async Task<ShopStockMovementReportResultDto> GetStockMovementReportAsync(GetShopStockMovementReportInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);
        var canViewCost = await CanViewCostAsync();

        var filtered = await BuildStockMovementQueryAsync(tenantId, range, input);
        var totals = await ComputeStockMovementTotalsAsync(filtered, canViewCost);

        var sorting = ApplySorting(input.Sorting, "TransactionDate desc, CreationTime desc");
        var totalCount = await AsyncExecuter.CountAsync(filtered);
        var paged = await AsyncExecuter.ToListAsync(ApplyDynamicSort(filtered, sorting).Skip(input.SkipCount).Take(input.MaxResultCount));
        var items = await ProjectStockMovementItemsAsync(paged, canViewCost);

        return new ShopStockMovementReportResultDto { Items = items, TotalCount = totalCount, Totals = totals };
    }

    [Authorize(EHubPermissions.ShopReports.StockMovements), Authorize(EHubPermissions.ShopReports.Export)]
    public async Task<IRemoteStreamContent> ExportStockMovementReportAsync(GetShopStockMovementReportInput input, ShopReportExportFormat format)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);
        var canViewCost = await CanViewCostAsync();

        var filtered = await BuildStockMovementQueryAsync(tenantId, range, input);
        var totals = await ComputeStockMovementTotalsAsync(filtered, canViewCost);

        var count = await AsyncExecuter.CountAsync(filtered);
        EnsureWithinExportLimit(count);

        var all = await AsyncExecuter.ToListAsync(ApplyDynamicSort(filtered, ApplySorting(input.Sorting, "TransactionDate desc")));
        var items = await ProjectStockMovementItemsAsync(all, canViewCost);
        var setting = await GetSettingAsync(tenantId);

        var columns = new List<ShopReportExportColumn>
        {
            new("Date", "transactionDate"), new("Product", "productName"), new("Batch", "batchNumber"), new("Type", "transactionType"),
            new("Reference", "referenceNumber"), new("Qty In", "quantityIn"), new("Qty Out", "quantityOut"), new("Balance", "productBalanceQuantity"),
        };
        if (canViewCost) { columns.Add(new("Unit Cost", "unitCost")); columns.Add(new("Total Cost", "totalCost")); }

        var request = new ShopReportExportRequest
        {
            ReportTitle = "Stock Movement Report",
            ShopName = setting?.ShopDisplayName,
            ShopAddress = CombineAddress(setting),
            ShopPhone = setting?.Phone,
            DateFrom = range.From,
            DateTo = range.ToExclusive.AddDays(-1),
            GeneratedDate = Clock.Now,
            Columns = columns,
            Rows = items.Select(x => new Dictionary<string, object?>
            {
                ["transactionDate"] = x.TransactionDate, ["productName"] = x.ProductName, ["batchNumber"] = x.BatchNumber,
                ["transactionType"] = x.TransactionType.ToString(), ["referenceNumber"] = x.ReferenceNumber,
                ["quantityIn"] = x.QuantityIn, ["quantityOut"] = x.QuantityOut, ["productBalanceQuantity"] = x.ProductBalanceQuantity,
                ["unitCost"] = x.UnitCost, ["totalCost"] = x.TotalCost,
            }).ToList(),
            TotalsLines = new List<(string, string)>
            {
                ("Transactions", totals.TransactionCount.ToString()), ("Total Qty In", totals.TotalQuantityIn.ToString("N4")),
                ("Total Qty Out", totals.TotalQuantityOut.ToString("N4")), ("Net Movement", totals.NetQuantityMovement.ToString("N4")),
            },
        };

        return await _exportService.ExportAsync(request, format);
    }

    private async Task<IQueryable<ShopStockTransaction>> BuildStockMovementQueryAsync(Guid tenantId, ShopReportDateRange range, GetShopStockMovementReportInput input)
    {
        var query = (await _stockTransactionRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TransactionDate >= range.From && x.TransactionDate < range.ToExclusive);

        if (input.ProductId.HasValue) query = query.Where(x => x.ProductId == input.ProductId.Value);
        if (input.ProductBatchId.HasValue) query = query.Where(x => x.ProductBatchId == input.ProductBatchId.Value);
        if (input.TransactionType.HasValue) query = query.Where(x => x.TransactionType == input.TransactionType.Value);
        if (input.ReferenceType.HasValue) query = query.Where(x => x.ReferenceType == input.ReferenceType.Value);
        if (!string.IsNullOrWhiteSpace(input.ReferenceNumber)) query = query.Where(x => x.ReferenceNumber.Contains(input.ReferenceNumber));
        if (input.CreatedByUserId.HasValue) query = query.Where(x => x.CreatedByUserId == input.CreatedByUserId.Value);

        query = input.QuantityDirection switch
        {
            ShopStockQuantityDirection.StockIn => query.Where(x => x.QuantityIn > 0),
            ShopStockQuantityDirection.StockOut => query.Where(x => x.QuantityOut > 0),
            _ => query,
        };

        if (input.ProductCategoryId.HasValue)
        {
            var productQuery = (await _productRepository.GetQueryableAsync()).AsNoTracking();
            var productIds = productQuery.Where(p => p.TenantId == tenantId && p.CategoryId == input.ProductCategoryId.Value).Select(p => p.Id);
            query = query.Where(x => productIds.Contains(x.ProductId));
        }

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var term = input.Filter;
            query = query.Where(x => x.ReferenceNumber.Contains(term) || (x.BatchNumber != null && x.BatchNumber.Contains(term)) || (x.Notes != null && x.Notes.Contains(term)));
        }

        return query;
    }

    private async Task<List<ShopStockMovementReportItemDto>> ProjectStockMovementItemsAsync(List<ShopStockTransaction> transactions, bool canViewCost)
    {
        if (transactions.Count == 0) return new List<ShopStockMovementReportItemDto>();

        var productIds = transactions.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = (await _productRepository.GetQueryableAsync()).AsNoTracking();
        var products = (await AsyncExecuter.ToListAsync(productQuery.Where(x => productIds.Contains(x.Id)))).ToDictionary(x => x.Id);

        var unitIds = products.Values.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = (await _unitRepository.GetQueryableAsync()).AsNoTracking();
        var units = (await AsyncExecuter.ToListAsync(unitQuery.Where(x => unitIds.Contains(x.Id)))).ToDictionary(x => x.Id);

        var userIds = transactions.Where(x => x.CreatedByUserId.HasValue).Select(x => x.CreatedByUserId!.Value);
        var userNames = await GetUserNamesAsync(userIds);

        return transactions.Select(t =>
        {
            products.TryGetValue(t.ProductId, out var product);
            var unitName = product != null && units.TryGetValue(product.UnitId, out var unit) ? unit.Name : string.Empty;

            return new ShopStockMovementReportItemDto
            {
                StockTransactionId = t.Id,
                TransactionDate = t.TransactionDate,
                ProductId = t.ProductId,
                ProductCode = product?.Code ?? string.Empty,
                ProductName = product?.Name ?? string.Empty,
                UnitName = unitName,
                ProductBatchId = t.ProductBatchId,
                BatchNumber = t.BatchNumber,
                ExpiryDate = t.ExpiryDate,
                TransactionType = t.TransactionType,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                ReferenceNumber = t.ReferenceNumber,
                QuantityIn = t.QuantityIn,
                QuantityOut = t.QuantityOut,
                ProductBalanceQuantity = t.BalanceQuantity,
                BatchBalanceQuantity = t.BatchBalanceQuantity,
                UnitCost = canViewCost ? t.UnitCost : null,
                TotalCost = canViewCost ? t.TotalCost : null,
                Notes = t.Notes,
                CreatedByUserName = t.CreatedByUserId.HasValue ? userNames.GetValueOrDefault(t.CreatedByUserId.Value) : null,
                CreationTime = t.CreationTime,
            };
        }).ToList();
    }

    private async Task<ShopStockMovementReportTotalsDto> ComputeStockMovementTotalsAsync(IQueryable<ShopStockTransaction> filtered, bool canViewCost)
    {
        var agg = await AsyncExecuter.FirstOrDefaultAsync(
            filtered.GroupBy(x => 1).Select(g => new
            {
                Count = g.Count(),
                QtyIn = g.Sum(x => x.QuantityIn),
                QtyOut = g.Sum(x => x.QuantityOut),
                ValueIn = g.Where(x => x.QuantityIn > 0).Sum(x => x.QuantityIn * x.UnitCost),
                ValueOut = g.Where(x => x.QuantityOut > 0).Sum(x => x.QuantityOut * x.UnitCost),
            }));

        if (agg == null || agg.Count == 0) return new ShopStockMovementReportTotalsDto();

        return new ShopStockMovementReportTotalsDto
        {
            TransactionCount = agg.Count,
            TotalQuantityIn = agg.QtyIn,
            TotalQuantityOut = agg.QtyOut,
            NetQuantityMovement = agg.QtyIn - agg.QtyOut,
            TotalStockInValue = canViewCost ? Math.Round(agg.ValueIn, 2) : null,
            TotalStockOutValue = canViewCost ? Math.Round(agg.ValueOut, 2) : null,
        };
    }

    // ------------------------------------------------------------------
    // 5. Batch and Expiry Report
    // ------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopReports.BatchExpiry)]
    public async Task<ShopBatchExpiryReportResultDto> GetBatchExpiryReportAsync(GetShopBatchExpiryReportInput input)
    {
        var tenantId = RequireTenant();
        var canViewCost = await CanViewCostAsync();
        var today = Clock.Now.Date;

        var filtered = await BuildBatchExpiryQueryAsync(tenantId, today, input);
        var totals = await ComputeBatchExpiryTotalsAsync(filtered, today, canViewCost);

        var sorting = ApplySorting(input.Sorting, "ExpiryDate asc");
        var totalCount = await AsyncExecuter.CountAsync(filtered);
        var paged = await AsyncExecuter.ToListAsync(ApplyDynamicSort(filtered, sorting).Skip(input.SkipCount).Take(input.MaxResultCount));
        var items = await ProjectBatchExpiryItemsAsync(paged, today, canViewCost);

        return new ShopBatchExpiryReportResultDto { Items = items, TotalCount = totalCount, Totals = totals };
    }

    [Authorize(EHubPermissions.ShopReports.BatchExpiry), Authorize(EHubPermissions.ShopReports.Export)]
    public async Task<IRemoteStreamContent> ExportBatchExpiryReportAsync(GetShopBatchExpiryReportInput input, ShopReportExportFormat format)
    {
        var tenantId = RequireTenant();
        var canViewCost = await CanViewCostAsync();
        var today = Clock.Now.Date;

        var filtered = await BuildBatchExpiryQueryAsync(tenantId, today, input);
        var totals = await ComputeBatchExpiryTotalsAsync(filtered, today, canViewCost);

        var count = await AsyncExecuter.CountAsync(filtered);
        EnsureWithinExportLimit(count);

        var all = await AsyncExecuter.ToListAsync(ApplyDynamicSort(filtered, ApplySorting(input.Sorting, "ExpiryDate asc")));
        var items = await ProjectBatchExpiryItemsAsync(all, today, canViewCost);
        var setting = await GetSettingAsync(tenantId);

        var columns = new List<ShopReportExportColumn>
        {
            new("Product", "productName"), new("Batch", "batchNumber"), new("Manufacturing Date", "manufacturingDate"), new("Expiry Date", "expiryDate"),
            new("Days to Expiry", "daysToExpiry"), new("Available Qty", "availableQuantity"),
        };
        if (canViewCost) { columns.Add(new("Unit Cost", "unitCost")); columns.Add(new("Stock Value", "stockValue")); }
        columns.Add(new("Supplier", "supplierName"));
        columns.Add(new("Status", "status"));

        var request = new ShopReportExportRequest
        {
            ReportTitle = "Batch and Expiry Report",
            ShopName = setting?.ShopDisplayName,
            ShopAddress = CombineAddress(setting),
            ShopPhone = setting?.Phone,
            GeneratedDate = Clock.Now,
            Columns = columns,
            Rows = items.Select(x => new Dictionary<string, object?>
            {
                ["productName"] = x.ProductName, ["batchNumber"] = x.BatchNumber, ["manufacturingDate"] = x.ManufacturingDate,
                ["expiryDate"] = x.ExpiryDate, ["daysToExpiry"] = x.DaysToExpiry, ["availableQuantity"] = x.AvailableQuantity,
                ["unitCost"] = x.UnitCost, ["stockValue"] = x.StockValue, ["supplierName"] = x.SupplierName, ["status"] = x.Status.ToString(),
            }).ToList(),
            TotalsLines = new List<(string, string)>
            {
                ("Total Batches", totals.TotalBatches.ToString()), ("Near Expiry", totals.NearExpiryBatches.ToString()),
                ("Expired", totals.ExpiredBatches.ToString()), ("Total Available Qty", totals.TotalAvailableQuantity.ToString("N4")),
            },
        };

        return await _exportService.ExportAsync(request, format);
    }

    private async Task<IQueryable<ShopProductBatch>> BuildBatchExpiryQueryAsync(Guid tenantId, DateTime today, GetShopBatchExpiryReportInput input)
    {
        var query = (await _productBatchRepository.GetQueryableAsync()).AsNoTracking().Where(x => x.TenantId == tenantId);

        if (input.ProductId.HasValue) query = query.Where(x => x.ProductId == input.ProductId.Value);
        if (input.SupplierId.HasValue) query = query.Where(x => x.SupplierId == input.SupplierId.Value);
        if (input.BatchStatus.HasValue) query = query.Where(x => x.Status == input.BatchStatus.Value);
        if (input.ExpiryFrom.HasValue) query = query.Where(x => x.ExpiryDate != null && x.ExpiryDate >= input.ExpiryFrom.Value.Date);
        if (input.ExpiryTo.HasValue) query = query.Where(x => x.ExpiryDate != null && x.ExpiryDate <= input.ExpiryTo.Value.Date);
        if (input.NearExpiryOnly) query = query.Where(x => x.Status == ShopProductBatchStatus.NearExpiry);
        if (input.ExpiredOnly) query = query.Where(x => x.Status == ShopProductBatchStatus.Expired);
        if (input.ActiveOnly) query = query.Where(x => x.Status == ShopProductBatchStatus.Active);
        if (input.HasAvailableStock) query = query.Where(x => x.AvailableQuantity > 0);
        if (!input.IncludeBlocked) query = query.Where(x => !x.IsBlocked);

        if (input.ProductCategoryId.HasValue)
        {
            var productQuery = (await _productRepository.GetQueryableAsync()).AsNoTracking();
            var productIds = productQuery.Where(p => p.TenantId == tenantId && p.CategoryId == input.ProductCategoryId.Value).Select(p => p.Id);
            query = query.Where(x => productIds.Contains(x.ProductId));
        }

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var term = input.Filter;
            var productQuery = (await _productRepository.GetQueryableAsync()).AsNoTracking();
            var matchingProductIds = productQuery.Where(p => p.TenantId == tenantId && (p.Code.Contains(term) || p.Name.Contains(term))).Select(p => p.Id);
            var supplierQuery = (await _supplierRepository.GetQueryableAsync()).AsNoTracking();
            var matchingSupplierIds = supplierQuery.Where(s => s.TenantId == tenantId && s.Name.Contains(term)).Select(s => s.Id);

            query = query.Where(x =>
                x.BatchNumber.Contains(term) ||
                matchingProductIds.Contains(x.ProductId) ||
                (x.SupplierId.HasValue && matchingSupplierIds.Contains(x.SupplierId.Value)) ||
                (x.Notes != null && x.Notes.Contains(term)));
        }

        return query;
    }

    private async Task<List<ShopBatchExpiryReportItemDto>> ProjectBatchExpiryItemsAsync(List<ShopProductBatch> batches, DateTime today, bool canViewCost)
    {
        if (batches.Count == 0) return new List<ShopBatchExpiryReportItemDto>();

        var productIds = batches.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = (await _productRepository.GetQueryableAsync()).AsNoTracking();
        var products = (await AsyncExecuter.ToListAsync(productQuery.Where(x => productIds.Contains(x.Id)))).ToDictionary(x => x.Id);

        var supplierIds = batches.Where(x => x.SupplierId.HasValue).Select(x => x.SupplierId!.Value).Distinct().ToList();
        var supplierQuery = (await _supplierRepository.GetQueryableAsync()).AsNoTracking();
        var suppliers = (await AsyncExecuter.ToListAsync(supplierQuery.Where(x => supplierIds.Contains(x.Id)))).ToDictionary(x => x.Id);

        return batches.Select(b =>
        {
            products.TryGetValue(b.ProductId, out var product);
            ShopSupplier? supplier = null;
            if (b.SupplierId.HasValue) suppliers.TryGetValue(b.SupplierId.Value, out supplier);

            return new ShopBatchExpiryReportItemDto
            {
                ProductBatchId = b.Id,
                ProductId = b.ProductId,
                ProductCode = product?.Code ?? string.Empty,
                ProductName = product?.Name ?? string.Empty,
                BatchNumber = b.BatchNumber,
                ManufacturingDate = b.ManufacturingDate,
                ExpiryDate = b.ExpiryDate,
                DaysToExpiry = b.ExpiryDate.HasValue ? (int)(b.ExpiryDate.Value.Date - today).TotalDays : null,
                ReceivedQuantity = b.ReceivedQuantity,
                IssuedQuantity = b.IssuedQuantity,
                ReservedQuantity = b.ReservedQuantity,
                AvailableQuantity = b.AvailableQuantity,
                UnitCost = canViewCost ? b.UnitCost : null,
                StockValue = canViewCost ? Math.Round(b.AvailableQuantity * b.UnitCost, 2) : null,
                Status = b.Status,
                SupplierId = b.SupplierId,
                SupplierName = supplier?.Name,
                FirstReceivedDate = b.FirstReceivedDate,
                LastMovementDate = b.LastMovementDate,
                IsBlocked = b.IsBlocked,
                BlockReason = b.BlockReason,
            };
        }).ToList();
    }

    private async Task<ShopBatchExpiryReportTotalsDto> ComputeBatchExpiryTotalsAsync(IQueryable<ShopProductBatch> filtered, DateTime today, bool canViewCost)
    {
        var agg = await AsyncExecuter.FirstOrDefaultAsync(
            filtered.GroupBy(x => 1).Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(x => x.Status == ShopProductBatchStatus.Active),
                NearExpiry = g.Count(x => x.Status == ShopProductBatchStatus.NearExpiry),
                Expired = g.Count(x => x.Status == ShopProductBatchStatus.Expired),
                Blocked = g.Count(x => x.IsBlocked),
                Exhausted = g.Count(x => x.Status == ShopProductBatchStatus.Exhausted),
                TotalAvailable = g.Sum(x => x.AvailableQuantity),
                NearExpiryQty = g.Where(x => x.Status == ShopProductBatchStatus.NearExpiry).Sum(x => x.AvailableQuantity),
                ExpiredQty = g.Where(x => x.Status == ShopProductBatchStatus.Expired).Sum(x => x.AvailableQuantity),
                TotalValue = g.Sum(x => x.AvailableQuantity * x.UnitCost),
                ExpiredValue = g.Where(x => x.Status == ShopProductBatchStatus.Expired).Sum(x => x.AvailableQuantity * x.UnitCost),
            }));

        if (agg == null || agg.Total == 0) return new ShopBatchExpiryReportTotalsDto();

        return new ShopBatchExpiryReportTotalsDto
        {
            TotalBatches = agg.Total,
            ActiveBatches = agg.Active,
            NearExpiryBatches = agg.NearExpiry,
            ExpiredBatches = agg.Expired,
            BlockedBatches = agg.Blocked,
            ExhaustedBatches = agg.Exhausted,
            TotalAvailableQuantity = agg.TotalAvailable,
            NearExpiryQuantity = agg.NearExpiryQty,
            ExpiredQuantity = agg.ExpiredQty,
            TotalBatchStockValue = canViewCost ? Math.Round(agg.TotalValue, 2) : null,
            ExpiredStockValue = canViewCost ? Math.Round(agg.ExpiredValue, 2) : null,
        };
    }
}
