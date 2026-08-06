namespace EHub.ShopManagement.ProfitLoss;

/// <summary>
/// Raw, permission-agnostic Profit and Loss figures for a single date range. Consumed both by the
/// (permission-gated) application service and directly by <c>ProfitLossNotificationGenerator</c>,
/// which needs the real numbers regardless of any particular user's viewing permissions.
/// </summary>
public class ShopProfitLossCoreResult
{
    public decimal GrossSales { get; set; }
    public decimal SalesDiscounts { get; set; }
    public decimal SalesTax { get; set; }
    public decimal SalesReturns { get; set; }
    public decimal NetSales { get; set; }

    public decimal CostOfGoodsSold { get; set; }
    public decimal GrossProfit { get; set; }

    public decimal OperatingExpenses { get; set; }
    public decimal OtherIncome { get; set; }
    public decimal NetProfit { get; set; }
    public ShopProfitLossResultStatus ResultStatus { get; set; }

    public decimal OpeningInventoryValue { get; set; }
    public decimal NetPurchases { get; set; }
    public decimal ClosingInventoryValue { get; set; }
}
