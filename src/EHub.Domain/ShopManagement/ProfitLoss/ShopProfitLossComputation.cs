using System.Collections.Generic;
using EHub.ShopManagement.Expenses;
using EHub.ShopManagement.SaleReturns;
using EHub.ShopManagement.Sales;

namespace EHub.ShopManagement.ProfitLoss;

/// <summary>
/// The aggregate <see cref="ShopProfitLossCoreResult"/> for a period, plus the raw entity lists that
/// were loaded to compute it. Callers that need more than the summary numbers for the same period
/// (trend charts, expense breakdown, product contribution) reuse these lists instead of re-querying
/// the database for data that was already loaded.
/// </summary>
public class ShopProfitLossComputation
{
    public ShopProfitLossCoreResult Core { get; set; } = new();
    public List<ShopSale> Sales { get; set; } = new();
    public List<ShopSaleItem> SaleItems { get; set; } = new();
    public List<ShopSaleReturn> SaleReturns { get; set; } = new();
    public List<ShopSaleReturnItem> SaleReturnItems { get; set; } = new();
    public List<ShopExpense> Expenses { get; set; } = new();
}
