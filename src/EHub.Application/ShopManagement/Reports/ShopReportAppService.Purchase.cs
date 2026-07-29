using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.PurchaseReturns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Content;

namespace EHub.ShopManagement.Reports;

public partial class ShopReportAppService
{
    // ------------------------------------------------------------------
    // 2. Purchase Report (uses posted Goods Receipts - the actual inventory-purchase event;
    //    Purchase Orders are only ordering documents and are never counted as spend here)
    // ------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopReports.Purchases)]
    public async Task<ShopPurchaseReportResultDto> GetPurchaseReportAsync(GetShopPurchaseReportInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);
        var filtered = await BuildPurchaseReportQueryAsync(tenantId, range, input);
        var sorting = ApplySorting(input.Sorting, "ReceiptDate desc, CreationTime desc");

        // PaymentStatus/HasPendingAmount depend on Paid/Pending, which are computed (Goods Receipt has
        // no persisted Paid/Pending column) rather than SQL-filterable columns. When either is active,
        // fall back to loading the full SQL-filtered candidate set, computing paid/pending for all of
        // it in bulk, filtering and paginating in memory - otherwise stay on the cheap DB-paged path.
        if (input.PaymentStatus.HasValue || input.HasPendingAmount.HasValue)
        {
            var all = await AsyncExecuter.ToListAsync(ApplyDynamicSort(filtered, sorting));
            var allItems = await ProjectPurchaseItemsAsync(all);
            var postFiltered = ApplyComputedPurchaseFilters(allItems, input.HasPendingAmount, input.PaymentStatus);

            var totals = ComputePurchaseTotalsFromItems(postFiltered);
            var page = postFiltered.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            return new ShopPurchaseReportResultDto { Items = page, TotalCount = postFiltered.Count, Totals = totals };
        }
        else
        {
            var totals = await ComputePurchaseTotalsAsync(filtered);
            var totalCount = await AsyncExecuter.CountAsync(filtered);
            var paged = await AsyncExecuter.ToListAsync(ApplyDynamicSort(filtered, sorting).Skip(input.SkipCount).Take(input.MaxResultCount));
            var items = await ProjectPurchaseItemsAsync(paged);
            return new ShopPurchaseReportResultDto { Items = items, TotalCount = totalCount, Totals = totals };
        }
    }

    [Authorize(EHubPermissions.ShopReports.Purchases), Authorize(EHubPermissions.ShopReports.Export)]
    public async Task<IRemoteStreamContent> ExportPurchaseReportAsync(GetShopPurchaseReportInput input, ShopReportExportFormat format)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);
        var filtered = await BuildPurchaseReportQueryAsync(tenantId, range, input);
        var sorting = ApplySorting(input.Sorting, "ReceiptDate desc");

        var all = await AsyncExecuter.ToListAsync(ApplyDynamicSort(filtered, sorting));
        EnsureWithinExportLimit(all.Count);

        var allItems = await ProjectPurchaseItemsAsync(all);
        var items = ApplyComputedPurchaseFilters(allItems, input.HasPendingAmount, input.PaymentStatus);
        var totals = ComputePurchaseTotalsFromItems(items);
        var setting = await GetSettingAsync(tenantId);

        var request = new ShopReportExportRequest
        {
            ReportTitle = "Purchase Report",
            ShopName = setting?.ShopDisplayName,
            ShopAddress = CombineAddress(setting),
            ShopPhone = setting?.Phone,
            DateFrom = range.From,
            DateTo = range.ToExclusive.AddDays(-1),
            GeneratedDate = Clock.Now,
            Columns = new List<ShopReportExportColumn>
            {
                new("Goods Receipt", "goodsReceiptNumber"), new("Date", "goodsReceiptDate"), new("Purchase Order", "purchaseOrderNumber"),
                new("Supplier", "supplierName"), new("Items", "totalItems"), new("Quantity", "totalQuantity"),
                new("Gross", "grossAmount"), new("Return", "purchaseReturnAmount"), new("Net Purchase", "finalPurchaseAmount"),
                new("Paid", "paidAmount"), new("Pending", "pendingAmount"), new("Status", "status"),
            },
            Rows = items.Select(x => new Dictionary<string, object?>
            {
                ["goodsReceiptNumber"] = x.GoodsReceiptNumber, ["goodsReceiptDate"] = x.GoodsReceiptDate, ["purchaseOrderNumber"] = x.PurchaseOrderNumber,
                ["supplierName"] = x.SupplierName, ["totalItems"] = x.TotalItems, ["totalQuantity"] = x.TotalQuantity,
                ["grossAmount"] = x.GrossAmount, ["purchaseReturnAmount"] = x.PurchaseReturnAmount, ["finalPurchaseAmount"] = x.FinalPurchaseAmount,
                ["paidAmount"] = x.PaidAmount, ["pendingAmount"] = x.PendingAmount, ["status"] = x.Status.ToString(),
            }).ToList(),
            TotalsLines = new List<(string, string)>
            {
                ("Purchase Count", totals.PurchaseCount.ToString()), ("Gross Purchases", totals.GrossPurchases.ToString("N2")),
                ("Purchase Returns", totals.PurchaseReturnAmount.ToString("N2")), ("Final Net Purchases", totals.FinalNetPurchases.ToString("N2")),
                ("Paid", totals.PaidAmount.ToString("N2")), ("Pending", totals.PendingAmount.ToString("N2")),
            },
        };

        return await _exportService.ExportAsync(request, format);
    }

    private async Task<IQueryable<ShopGoodsReceipt>> BuildPurchaseReportQueryAsync(Guid tenantId, ShopReportDateRange range, GetShopPurchaseReportInput input)
    {
        var query = (await _goodsReceiptRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ReceiptDate >= range.From && x.ReceiptDate < range.ToExclusive);

        query = input.GoodsReceiptStatus.HasValue
            ? query.Where(x => x.Status == input.GoodsReceiptStatus.Value)
            : query.Where(x => x.Status == ShopGoodsReceiptStatus.Completed);

        if (input.SupplierId.HasValue) query = query.Where(x => x.SupplierId == input.SupplierId.Value);
        if (input.PurchaseOrderId.HasValue) query = query.Where(x => x.PurchaseOrderId == input.PurchaseOrderId.Value);
        if (input.MinimumAmount.HasValue) query = query.Where(x => x.GrandTotal >= input.MinimumAmount.Value);
        if (input.MaximumAmount.HasValue) query = query.Where(x => x.GrandTotal <= input.MaximumAmount.Value);

        if (input.ProductId.HasValue || input.ProductCategoryId.HasValue)
        {
            var itemQuery = (await _goodsReceiptItemRepository.GetQueryableAsync()).AsNoTracking();
            var productQuery = (await _productRepository.GetQueryableAsync()).AsNoTracking();

            var matchingIds = itemQuery.Where(i => i.TenantId == tenantId);
            if (input.ProductId.HasValue) matchingIds = matchingIds.Where(i => i.ProductId == input.ProductId.Value);
            if (input.ProductCategoryId.HasValue)
            {
                var categoryProductIds = productQuery.Where(p => p.TenantId == tenantId && p.CategoryId == input.ProductCategoryId.Value).Select(p => p.Id);
                matchingIds = matchingIds.Where(i => categoryProductIds.Contains(i.ProductId));
            }

            var grIds = matchingIds.Select(i => i.GoodsReceiptId);
            query = query.Where(x => grIds.Contains(x.Id));
        }

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var term = input.Filter;
            var supplierQuery = (await _supplierRepository.GetQueryableAsync()).AsNoTracking();
            var matchingSupplierIds = supplierQuery.Where(s => s.TenantId == tenantId && s.Name.Contains(term)).Select(s => s.Id);

            var itemQuery = (await _goodsReceiptItemRepository.GetQueryableAsync()).AsNoTracking();
            var matchingIdsByProduct = itemQuery.Where(i => i.TenantId == tenantId && (i.ProductCodeSnapshot.Contains(term) || i.ProductNameSnapshot.Contains(term))).Select(i => i.GoodsReceiptId);

            query = query.Where(x =>
                x.GoodsReceiptNumber.Contains(term) ||
                matchingSupplierIds.Contains(x.SupplierId) ||
                matchingIdsByProduct.Contains(x.Id));
        }

        return query;
    }

    private static List<ShopPurchaseReportItemDto> ApplyComputedPurchaseFilters(
        List<ShopPurchaseReportItemDto> items, bool? hasPendingAmount, ShopGoodsReceiptPaymentStatus? paymentStatus)
    {
        IEnumerable<ShopPurchaseReportItemDto> result = items;
        if (hasPendingAmount == true) result = result.Where(x => x.PendingAmount > 0);
        if (hasPendingAmount == false) result = result.Where(x => x.PendingAmount <= 0);
        if (paymentStatus.HasValue) result = result.Where(x => x.PaymentStatus == paymentStatus.Value);
        return result.ToList();
    }

    private static ShopPurchaseReportTotalsDto ComputePurchaseTotalsFromItems(List<ShopPurchaseReportItemDto> items)
    {
        if (items.Count == 0) return new ShopPurchaseReportTotalsDto();

        var netBeforeReturns = items.Sum(x => x.NetAmount);
        var returnAmount = items.Sum(x => x.PurchaseReturnAmount);
        var finalNet = Math.Round(netBeforeReturns - returnAmount, 2);

        return new ShopPurchaseReportTotalsDto
        {
            PurchaseCount = items.Count,
            GrossPurchases = items.Sum(x => x.GrossAmount),
            TotalDiscount = items.Sum(x => x.DiscountAmount),
            TotalTax = items.Sum(x => x.TaxAmount),
            NetPurchasesBeforeReturns = netBeforeReturns,
            PurchaseReturnAmount = returnAmount,
            FinalNetPurchases = finalNet,
            PaidAmount = items.Sum(x => x.PaidAmount),
            PendingAmount = items.Sum(x => x.PendingAmount),
            TotalQuantityPurchased = items.Sum(x => x.TotalQuantity),
            AveragePurchaseValue = items.Count > 0 ? Math.Round(finalNet / items.Count, 2) : 0,
        };
    }

    private async Task<List<ShopPurchaseReportItemDto>> ProjectPurchaseItemsAsync(List<ShopGoodsReceipt> receipts)
    {
        if (receipts.Count == 0) return new List<ShopPurchaseReportItemDto>();

        var tenantId = RequireTenant();
        var receiptIds = receipts.Select(x => x.Id).ToList();
        var supplierIds = receipts.Select(x => x.SupplierId).Distinct().ToList();
        var poIds = receipts.Select(x => x.PurchaseOrderId).Distinct().ToList();

        var supplierQuery = (await _supplierRepository.GetQueryableAsync()).AsNoTracking();
        var suppliers = (await AsyncExecuter.ToListAsync(supplierQuery.Where(x => supplierIds.Contains(x.Id)))).ToDictionary(x => x.Id);

        var poQuery = (await _purchaseOrderRepository.GetQueryableAsync()).AsNoTracking();
        var purchaseOrders = (await AsyncExecuter.ToListAsync(poQuery.Where(x => poIds.Contains(x.Id)))).ToDictionary(x => x.Id);

        var itemQuery = (await _goodsReceiptItemRepository.GetQueryableAsync()).AsNoTracking();
        var itemAggregates = await AsyncExecuter.ToListAsync(
            itemQuery.Where(x => x.TenantId == tenantId && receiptIds.Contains(x.GoodsReceiptId))
                .GroupBy(x => x.GoodsReceiptId)
                .Select(g => new { GoodsReceiptId = g.Key, Count = g.Count(), Quantity = g.Sum(x => x.ReceivedQuantity) }));
        var itemDict = itemAggregates.ToDictionary(x => x.GoodsReceiptId);

        var returnQuery = (await _purchaseReturnRepository.GetQueryableAsync()).AsNoTracking();
        var returns = await AsyncExecuter.ToListAsync(
            returnQuery.Where(x => x.TenantId == tenantId && receiptIds.Contains(x.GoodsReceiptId) && x.Status == ShopPurchaseReturnStatus.Completed));
        var returnDict = returns.GroupBy(x => x.GoodsReceiptId).ToDictionary(g => g.Key, g => g.Sum(x => x.GrandTotal));

        var paidAmounts = await _supplierPaymentManager.GetPostedAllocatedAmountsAsync(tenantId, receiptIds, excludePaymentId: null);

        return receipts.Select(gr =>
        {
            suppliers.TryGetValue(gr.SupplierId, out var supplier);
            purchaseOrders.TryGetValue(gr.PurchaseOrderId, out var po);
            itemDict.TryGetValue(gr.Id, out var itemAgg);
            var returned = returnDict.GetValueOrDefault(gr.Id);
            var paid = paidAmounts.GetValueOrDefault(gr.Id);
            var pending = Math.Max(0, gr.GrandTotal - paid - returned);
            var finalPurchase = Math.Round(gr.GrandTotal - returned, 2);

            return new ShopPurchaseReportItemDto
            {
                GoodsReceiptId = gr.Id,
                GoodsReceiptNumber = gr.GoodsReceiptNumber,
                GoodsReceiptDate = gr.ReceiptDate,
                PurchaseOrderId = gr.PurchaseOrderId,
                PurchaseOrderNumber = po?.PurchaseOrderNumber,
                SupplierId = gr.SupplierId,
                SupplierName = supplier?.Name ?? string.Empty,
                GrossAmount = gr.SubTotal,
                DiscountAmount = gr.DiscountAmount,
                TaxAmount = gr.TaxAmount,
                NetAmount = gr.GrandTotal,
                PaidAmount = paid,
                PendingAmount = pending,
                PurchaseReturnAmount = returned,
                FinalPurchaseAmount = finalPurchase,
                TotalItems = itemAgg?.Count ?? 0,
                TotalQuantity = itemAgg?.Quantity ?? 0,
                Status = gr.Status,
                PaymentStatus = ComputePaymentStatus(paid, pending),
                CreationTime = gr.CreationTime,
            };
        }).ToList();
    }

    private async Task<ShopPurchaseReportTotalsDto> ComputePurchaseTotalsAsync(IQueryable<ShopGoodsReceipt> filtered)
    {
        var tenantId = RequireTenant();

        var agg = await AsyncExecuter.FirstOrDefaultAsync(
            filtered.GroupBy(x => 1).Select(g => new
            {
                Count = g.Count(),
                Gross = g.Sum(x => x.SubTotal),
                Discount = g.Sum(x => x.DiscountAmount),
                Tax = g.Sum(x => x.TaxAmount),
                Net = g.Sum(x => x.GrandTotal),
            }));

        if (agg == null || agg.Count == 0) return new ShopPurchaseReportTotalsDto();

        var receiptIds = await AsyncExecuter.ToListAsync(filtered.Select(x => x.Id));

        var itemQuery = (await _goodsReceiptItemRepository.GetQueryableAsync()).AsNoTracking();
        var totalQuantity = await AsyncExecuter.SumAsync(
            itemQuery.Where(x => x.TenantId == tenantId && receiptIds.Contains(x.GoodsReceiptId)).Select(x => (decimal?)x.ReceivedQuantity)) ?? 0m;

        var returnQuery = (await _purchaseReturnRepository.GetQueryableAsync()).AsNoTracking();
        var returnAmount = await AsyncExecuter.SumAsync(
            returnQuery.Where(x => x.TenantId == tenantId && receiptIds.Contains(x.GoodsReceiptId) && x.Status == ShopPurchaseReturnStatus.Completed)
                .Select(x => (decimal?)x.GrandTotal)) ?? 0m;

        var paidAmounts = await _supplierPaymentManager.GetPostedAllocatedAmountsAsync(tenantId, receiptIds, excludePaymentId: null);
        var totalPaid = paidAmounts.Values.Sum();
        var totalPending = Math.Max(0, agg.Net - totalPaid - returnAmount);

        var finalNet = Math.Round(agg.Net - returnAmount, 2);

        return new ShopPurchaseReportTotalsDto
        {
            PurchaseCount = agg.Count,
            GrossPurchases = agg.Gross,
            TotalDiscount = agg.Discount,
            TotalTax = agg.Tax,
            NetPurchasesBeforeReturns = agg.Net,
            PurchaseReturnAmount = returnAmount,
            FinalNetPurchases = finalNet,
            PaidAmount = totalPaid,
            PendingAmount = totalPending,
            TotalQuantityPurchased = totalQuantity,
            AveragePurchaseValue = agg.Count > 0 ? Math.Round(finalNet / agg.Count, 2) : 0,
        };
    }
}
