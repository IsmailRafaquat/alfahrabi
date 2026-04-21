using System;
using System.Collections.Generic;

namespace EHub.Reports.ExpenseReport;

public class ExpenseReport
{
    public Guid ExpenseCategoryId { get; set; }
    public string? ExpenseCategoryName { get; set; }
    public List<ExpenseReportMonthColumn> MonthColumns { get; set; } = [];
}

public class ExpenseReportMonthColumn
{
    public DateTime Month { get; set; }
    public List<ExpenseReportDetailRow> Details { get; set; } = [];
}

public class ExpenseReportDetailRow
{
    public Guid ExpenseEntryId { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Title { get; set; }
    public decimal Amount { get; set; }
    public string? PaidTo { get; set; }
    public string? Remarks { get; set; }
}
