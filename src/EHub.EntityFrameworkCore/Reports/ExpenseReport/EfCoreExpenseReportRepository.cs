using EHub.EntityFrameworkCore;
using EHub.Expenses.ExpenseCategories;
using EHub.Expenses.ExpenseEntries;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace EHub.Reports.ExpenseReport;

public class EfCoreExpenseReportRepository : IExpenseReportRepository
{
    private readonly IDbContextProvider<EHubDbContext> _dbContextProvider;

    public EfCoreExpenseReportRepository(IDbContextProvider<EHubDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<List<ExpenseReport>> GetExpenseReportAsync(
        DateTime periodStart,
        DateTime periodEnd,
        string? filter,
        List<Guid>? expenseCategoryIds = null)
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();

        var flatData = await (
            from expense in dbContext.Set<ExpenseEntry>()
            join category in dbContext.Set<ExpenseCategory>()
                on expense.ExpenseCategoryId equals category.Id
            where expense.ExpenseDate >= periodStart
                && expense.ExpenseDate <= periodEnd
                && (string.IsNullOrWhiteSpace(filter)
                    || category.Name.Contains(filter)
                    || expense.Title.Contains(filter)
                    || (expense.PaidTo != null && expense.PaidTo.Contains(filter))
                    || (expense.Remarks != null && expense.Remarks.Contains(filter)))
                && (expenseCategoryIds == null
                    || expenseCategoryIds.Count == 0
                    || expenseCategoryIds.Contains(category.Id))
            select new
            {
                ExpenseEntryId = expense.Id,
                ExpenseCategoryId = category.Id,
                ExpenseCategoryName = category.Name,
                expense.ExpenseDate,
                expense.Title,
                expense.Amount,
                expense.PaidTo,
                expense.Remarks,
                Month = new DateTime(expense.ExpenseDate.Year, expense.ExpenseDate.Month, 1)
            }
        ).ToListAsync();

        var result = flatData
            .GroupBy(x => new { x.ExpenseCategoryId, x.ExpenseCategoryName })
            .Select(categoryGroup => new ExpenseReport
            {
                ExpenseCategoryId = categoryGroup.Key.ExpenseCategoryId,
                ExpenseCategoryName = categoryGroup.Key.ExpenseCategoryName,
                MonthColumns = categoryGroup
                    .GroupBy(x => x.Month)
                    .OrderBy(monthGroup => monthGroup.Key)
                    .Select(monthGroup => new ExpenseReportMonthColumn
                    {
                        Month = monthGroup.Key,
                        Details = monthGroup
                            .OrderBy(x => x.ExpenseDate)
                            .ThenBy(x => x.Title)
                            .Select(detail => new ExpenseReportDetailRow
                            {
                                ExpenseEntryId = detail.ExpenseEntryId,
                                ExpenseDate = detail.ExpenseDate,
                                Title = detail.Title,
                                Amount = detail.Amount,
                                PaidTo = detail.PaidTo,
                                Remarks = detail.Remarks
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .OrderBy(x => x.ExpenseCategoryName)
            .ToList();

        return result;
    }
}
