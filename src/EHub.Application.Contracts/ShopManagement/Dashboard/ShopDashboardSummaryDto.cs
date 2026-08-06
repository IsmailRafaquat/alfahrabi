namespace EHub.ShopManagement.Dashboard;

/// <summary>
/// Nullable fields are omitted (set to null) when the current user lacks the permission that
/// protects that group of values, rather than being hidden only on the frontend.
/// </summary>
public class ShopDashboardSummaryDto
{
    // Requires ShopManagement.Dashboard.ViewFinancialSummary
    public decimal? TotalSales { get; set; }
    public int? TotalSalesCount { get; set; }
    public decimal? GrossSales { get; set; }
    public decimal? SaleReturns { get; set; }
    public decimal? NetSales { get; set; }

    public decimal? TotalPurchases { get; set; }
    public int? TotalPurchasesCount { get; set; }
    public decimal? PurchaseReturns { get; set; }
    public decimal? NetPurchases { get; set; }

    public decimal? TotalExpenses { get; set; }
    public int? TotalExpensesCount { get; set; }

    // Requires ShopManagement.Dashboard.ViewBalances
    public decimal? CustomerReceivables { get; set; }
    public decimal? SupplierPayables { get; set; }
    public decimal? CashBalance { get; set; }
    public decimal? BankBalance { get; set; }
    public decimal? TotalAvailableBalance { get; set; }

    // Inventory quantity is not sensitive; value requires ShopManagement.Dashboard.ViewInventoryValue
    public decimal InventoryQuantity { get; set; }
    public decimal? InventoryValue { get; set; }

    // Requires ShopManagement.Dashboard.ViewStockAlerts
    public int? LowStockProductCount { get; set; }
    public int? OutOfStockProductCount { get; set; }
    public int? NearExpiryBatchCount { get; set; }
    public int? ExpiredBatchCount { get; set; }

    public int ActiveCustomerCount { get; set; }
    public int ActiveSupplierCount { get; set; }
}
