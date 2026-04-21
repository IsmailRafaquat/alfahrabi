using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EHub.Reports.ExpenseReport;

public interface IExpenseReportRepository
{
    Task<List<ExpenseReport>> GetExpenseReportAsync(
        DateTime periodStart,
        DateTime periodEnd,
        string? filter,
        List<Guid>? expenseCategoryIds = null
    );
}
