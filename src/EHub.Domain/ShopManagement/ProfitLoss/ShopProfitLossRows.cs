using System;

namespace EHub.ShopManagement.ProfitLoss;

/// <summary>
/// Lightweight, read-only projections of just the columns <see cref="ShopProfitLossCalculator.ComputeDetailedAsync"/>'s
/// callers (trend, expense breakdown, product contribution) actually read - never the full Sale/
/// SaleItem/SaleReturn/SaleReturnItem/Expense entity. EF Core never change-tracks a projection like
/// this, and the query only selects these columns instead of every column on the table, so loading a
/// busy period's transactions this way is both lighter over the wire and cheaper to hold in memory
/// than loading full tracked entities.
/// </summary>
public record ShopProfitLossSaleRow(Guid Id, DateTime SaleDate, decimal SubTotal, decimal DiscountAmount, decimal TaxAmount);

public record ShopProfitLossSaleItemRow(Guid SaleId, Guid ProductId, decimal Quantity, decimal UnitCostSnapshot, decimal LineSubTotal, decimal DiscountAmount);

public record ShopProfitLossSaleReturnRow(Guid Id, DateTime ReturnDate, decimal GrandTotal);

public record ShopProfitLossSaleReturnItemRow(Guid SaleReturnId, Guid ProductId, decimal ReturnQuantity, decimal UnitCostSnapshot, decimal LineSubTotal, decimal DiscountAmount);

public record ShopProfitLossExpenseRow(DateTime ExpenseDate, Guid ExpenseCategoryId, decimal Amount);
