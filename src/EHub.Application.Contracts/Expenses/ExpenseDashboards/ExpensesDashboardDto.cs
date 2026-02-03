using System;
using System.Collections.Generic;

namespace EHub.Expenses.ExpenseDashboards;

public class ExpensesDashboardDto
{
    // KPI
    public decimal TotalExpenses { get; set; }
    public decimal TotalOtherExpenses { get; set; }
    public decimal TotalSalaryPayments { get; set; }
    public int TotalTransactions { get; set; }

    // Charts
    public List<ExpensesByCategoryDto> ByCategory { get; set; } = new();

    // Trend
    public List<ExpensesByDayDto> ByDay { get; set; } = new();

    // Table
    public List<RecentExpenseDto> RecentExpenses { get; set; } = new();
}

public class ExpensesByCategoryDto
{
    public Guid ExpenseCategoryId { get; set; }
    public string ExpenseCategoryName { get; set; } = default!;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class ExpensesByDayDto
{
    public DateTime Date { get; set; }     // date only
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class RecentExpenseDto
{
    public DateTime Date { get; set; }
    public string Source { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string CategoryOrStaff { get; set; } = default!;
    public decimal Amount { get; set; }
}