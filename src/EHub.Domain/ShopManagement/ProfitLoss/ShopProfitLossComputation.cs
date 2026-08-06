using System.Collections.Generic;

namespace EHub.ShopManagement.ProfitLoss;

/// <summary>
/// The aggregate <see cref="ShopProfitLossCoreResult"/> for a period, plus the lightweight row
/// projections that were loaded to compute it (see <see cref="ShopProfitLossSaleRow"/> and friends).
/// Callers that need more than the summary numbers for the same period (trend charts, expense
/// breakdown, product contribution) reuse these rows instead of re-querying the database for data
/// that was already loaded.
/// </summary>
public class ShopProfitLossComputation
{
    public ShopProfitLossCoreResult Core { get; set; } = new();
    public List<ShopProfitLossSaleRow> Sales { get; set; } = new();
    public List<ShopProfitLossSaleItemRow> SaleItems { get; set; } = new();
    public List<ShopProfitLossSaleReturnRow> SaleReturns { get; set; } = new();
    public List<ShopProfitLossSaleReturnItemRow> SaleReturnItems { get; set; } = new();
    public List<ShopProfitLossExpenseRow> Expenses { get; set; } = new();
}
