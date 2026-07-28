using System;
using System.Collections.Generic;

namespace EHub.ShopManagement.Dashboard;

public class ShopDashboardDto
{
    public ShopDashboardPeriod Period { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }

    public string ShopDisplayName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; }

    public ShopDashboardSummaryDto Summary { get; set; } = new();
    public ShopDashboardSalesExpenseChartDto? SalesExpenseChart { get; set; }
    public ShopDashboardSalesTrendDto? SalesTrend { get; set; }
    public List<ShopDashboardTopProductDto> TopProducts { get; set; } = new();
    public List<ShopDashboardLowStockProductDto> LowStockProducts { get; set; } = new();
    public List<ShopDashboardBatchAlertDto> NearExpiryBatches { get; set; } = new();
    public List<ShopDashboardBatchAlertDto> ExpiredBatches { get; set; } = new();
    public ShopDashboardBalanceSummaryDto? Balances { get; set; }
    public List<ShopDashboardRecentSaleDto> RecentSales { get; set; } = new();
    public List<ShopDashboardRecentPurchaseDto> RecentPurchases { get; set; } = new();
    public List<ShopDashboardRecentExpenseDto> RecentExpenses { get; set; } = new();
    public ShopDashboardInventorySummaryDto? Inventory { get; set; }
}
