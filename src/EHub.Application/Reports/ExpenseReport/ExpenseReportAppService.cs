using EHub.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;

namespace EHub.Reports.ExpenseReport;

[RemoteService(false)]
[Authorize(EHubPermissions.Reports.Default)]
public class ExpenseReportAppService : EHubAppService, IExpenseReportAppService
{
    private readonly IExpenseReportRepository _expenseReportRepository;

    public ExpenseReportAppService(IExpenseReportRepository expenseReportRepository)
    {
        _expenseReportRepository = expenseReportRepository;
    }

    public async Task<List<ExpenseReportDto>> GetExpenseReportAsync(ExpenseReportFilterDto input)
    {
        var today = DateTime.Today;

        var periodStart = input.PeriodStart ?? new DateTime(today.Year, 1, 1);
        var periodEnd = input.PeriodEnd ?? new DateTime(today.Year, 12, 31);

        var data = await _expenseReportRepository.GetExpenseReportAsync(
            periodStart,
            periodEnd,
            input.Filter,
            input.ExpenseCategoryIds
        );

        return data.Select(x => new ExpenseReportDto
        {
            ExpenseCategoryId = x.ExpenseCategoryId,
            ExpenseCategoryName = x.ExpenseCategoryName,
            MonthColumns = x.MonthColumns.Select(m => new ExpenseReportMonthDto
            {
                Month = m.Month,
                Details = m.Details.Select(d => new ExpenseReportDetailDto
                {
                    ExpenseEntryId = d.ExpenseEntryId,
                    ExpenseDate = d.ExpenseDate,
                    Title = d.Title,
                    Amount = d.Amount,
                    PaidTo = d.PaidTo,
                    Remarks = d.Remarks
                }).ToList()
            }).ToList()
        }).ToList();
    }
}
