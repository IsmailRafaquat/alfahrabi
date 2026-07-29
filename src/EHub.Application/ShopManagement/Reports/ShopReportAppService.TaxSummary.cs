using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.SaleReturns;
using EHub.ShopManagement.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EHub.ShopManagement.Reports;

public partial class ShopReportAppService
{
    // ------------------------------------------------------------------
    // 13. Tax Summary Report (only using tax fields that already exist on Sale/SaleReturn/GoodsReceipt/PurchaseReturn)
    // ------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopReports.TaxSummary)]
    public async Task<ShopTaxSummaryReportDto> GetTaxSummaryReportAsync(GetShopTaxSummaryReportInput input)
    {
        var tenantId = RequireTenant();
        var range = await _dateRangeResolver.ResolveAsync(input.Period, input.DateFrom, input.DateTo);

        var saleQuery = (await _saleRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == ShopSaleStatus.Completed
                && x.SaleDate >= range.From && x.SaleDate < range.ToExclusive);
        var salesTax = await AsyncExecuter.SumAsync(saleQuery.Select(x => x.TaxAmount));

        var saleReturnQuery = (await _saleReturnRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == ShopSaleReturnStatus.Completed
                && x.ReturnDate >= range.From && x.ReturnDate < range.ToExclusive);
        var salesReturnTax = await AsyncExecuter.SumAsync(saleReturnQuery.Select(x => x.TaxAmount));

        var goodsReceiptQuery = (await _goodsReceiptRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == GoodsReceipts.ShopGoodsReceiptStatus.Completed
                && x.ReceiptDate >= range.From && x.ReceiptDate < range.ToExclusive);
        var purchaseTax = await AsyncExecuter.SumAsync(goodsReceiptQuery.Select(x => x.TaxAmount));

        var purchaseReturnQuery = (await _purchaseReturnRepository.GetQueryableAsync()).AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == PurchaseReturns.ShopPurchaseReturnStatus.Completed
                && x.ReturnDate >= range.From && x.ReturnDate < range.ToExclusive);
        var purchaseReturnTax = await AsyncExecuter.SumAsync(purchaseReturnQuery.Select(x => x.TaxAmount));

        var netSalesTax = Math.Round(salesTax - salesReturnTax, 2);
        var netPurchaseTax = Math.Round(purchaseTax - purchaseReturnTax, 2);
        const decimal expenseTaxPaid = 0; // ShopExpense has no tax field - kept for shape completeness only.

        return new ShopTaxSummaryReportDto
        {
            DateFrom = range.From,
            DateTo = range.ToExclusive.AddDays(-1),
            SalesTaxCollected = salesTax,
            SalesReturnTaxReversed = salesReturnTax,
            NetSalesTax = netSalesTax,
            PurchaseTaxPaid = purchaseTax,
            PurchaseReturnTaxReversed = purchaseReturnTax,
            NetPurchaseTax = netPurchaseTax,
            ExpenseTaxPaid = expenseTaxPaid,
            NetTaxPosition = Math.Round(netSalesTax - netPurchaseTax - expenseTaxPaid, 2),
        };
    }
}
