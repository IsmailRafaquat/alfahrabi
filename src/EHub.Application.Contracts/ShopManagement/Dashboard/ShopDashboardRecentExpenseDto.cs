using System;
using EHub.ShopManagement.Expenses;

namespace EHub.ShopManagement.Dashboard;

public class ShopDashboardRecentExpenseDto
{
    public Guid ExpenseId { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public string ExpenseCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public ShopExpensePaymentMethod PaymentSource { get; set; }
    public ShopExpenseStatus Status { get; set; }
}
