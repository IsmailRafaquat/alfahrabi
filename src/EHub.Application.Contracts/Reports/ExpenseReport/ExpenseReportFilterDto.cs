using System;
using System.Collections.Generic;

namespace EHub.Reports.ExpenseReport;

public class ExpenseReportFilterDto
{
    public string? Filter { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public List<Guid>? ExpenseCategoryIds { get; set; }
}

public class ExpenseReportDto
{
    public Guid ExpenseCategoryId { get; set; }
    public string? ExpenseCategoryName { get; set; }
    public List<ExpenseReportMonthDto> MonthColumns { get; set; } = [];
}

public class ExpenseReportMonthDto
{
    public DateTime Month { get; set; }
    public List<ExpenseReportDetailDto> Details { get; set; } = [];
}

public class ExpenseReportDetailDto
{
    public Guid ExpenseEntryId { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Title { get; set; }
    public decimal Amount { get; set; }
    public string? PaidTo { get; set; }
    public string? Remarks { get; set; }
}
