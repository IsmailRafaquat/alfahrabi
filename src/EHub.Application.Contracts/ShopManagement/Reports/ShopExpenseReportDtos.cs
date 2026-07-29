using System;
using System.Collections.Generic;
using EHub.ShopManagement.Expenses;

namespace EHub.ShopManagement.Reports;

public class GetShopExpenseReportInput : ShopReportInputBase
{
    public Guid? ExpenseCategoryId { get; set; }
    public ShopExpensePaymentMethod? PaymentSource { get; set; }
    public Guid? CashRegisterId { get; set; }
    public Guid? BankAccountId { get; set; }
    public ShopExpenseStatus? ExpenseStatus { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public ShopExpenseReportGroupBy GroupBy { get; set; } = ShopExpenseReportGroupBy.None;
}

public class ShopExpenseReportItemDto
{
    public Guid ExpenseId { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public Guid ExpenseCategoryId { get; set; }
    public string ExpenseCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public ShopExpensePaymentMethod PaymentSource { get; set; }
    public Guid? BankAccountId { get; set; }
    public string? BankAccountName { get; set; }
    public string? ReferenceNumber { get; set; }
    public ShopExpenseStatus Status { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreationTime { get; set; }
}

public class ShopExpenseReportGroupItemDto
{
    public string GroupKey { get; set; } = string.Empty;
    public string GroupLabel { get; set; } = string.Empty;
    public int ExpenseCount { get; set; }
    public decimal Amount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class ShopExpenseReportTotalsDto
{
    public int ExpenseCount { get; set; }
    public decimal ExpenseAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalExpenseAmount { get; set; }
    public decimal CashExpenses { get; set; }
    public decimal BankExpenses { get; set; }
    public decimal OtherSourceExpenses { get; set; }
    public decimal AverageExpense { get; set; }
}

public class ShopExpenseReportResultDto
{
    public List<ShopExpenseReportItemDto> Items { get; set; } = new();
    public List<ShopExpenseReportGroupItemDto>? Groups { get; set; }
    public long TotalCount { get; set; }
    public ShopExpenseReportTotalsDto Totals { get; set; } = new();
}
