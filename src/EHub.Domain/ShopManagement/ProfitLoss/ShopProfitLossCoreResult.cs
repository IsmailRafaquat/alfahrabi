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

    /// <summary>COGS from posted Sale stock-out activity only, before any return reversal.</summary>
    public decimal CostOfGoodsSoldBeforeReturns { get; set; }
    /// <summary>COGS reversed by posted Sale Return stock-in activity - always sourced from the
    /// original Sale item's UnitCostSnapshot (never the product's current/average cost), since
    /// ShopSaleReturnItem copies UnitCostSnapshot straight from the ShopSaleItem it returns.</summary>
    public decimal ReturnedCostOfGoodsSold { get; set; }
    /// <summary>CostOfGoodsSoldBeforeReturns - ReturnedCostOfGoodsSold. This is the authoritative
    /// net COGS figure GrossProfit is computed from - kept as its own field (not just derived at
    /// the DTO layer) so ProfitLossNotificationGenerator and every other direct consumer of
    /// ShopProfitLossCoreResult see the exact same net figure the P&amp;L statement uses.</summary>
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
