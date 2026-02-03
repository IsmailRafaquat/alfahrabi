using EHub.Expenses.ExpenseCategories;
using EHub.Expenses.ExpenseEntries;
using EHub.Expenses.StaffSalaryPayments;
using EHub.Staffs;
using System;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.Expenses.ExpenseDashboards;

[RemoteService(IsEnabled = false)]
public class ExpensesDashboardAppService : ApplicationService, IExpensesDashboardAppService
{
    private readonly IRepository<ExpenseEntry, Guid> _expenseRepo;
    private readonly IRepository<ExpenseCategory, Guid> _categoryRepo;
    private readonly IRepository<StaffSalaryPayment, Guid> _salaryRepo;
    private readonly IRepository<Staff, Guid> _staffRepo;

    public ExpensesDashboardAppService(
        IRepository<ExpenseEntry, Guid> expenseRepo,
        IRepository<ExpenseCategory, Guid> categoryRepo,
        IRepository<StaffSalaryPayment, Guid> salaryRepo,
        IRepository<Staff, Guid> staffRepo)
    {
        _expenseRepo = expenseRepo;
        _categoryRepo = categoryRepo;
        _salaryRepo = salaryRepo;
        _staffRepo = staffRepo;
    }

    public async Task<ExpensesDashboardDto> GetAsync(GetExpensesDashboardInput input)
    {
        var expenseQ = await _expenseRepo.GetQueryableAsync();
        var catQ = await _categoryRepo.GetQueryableAsync();
        var salaryQ = await _salaryRepo.GetQueryableAsync();
        var staffQ = await _staffRepo.GetQueryableAsync();

        // -------------------------
        // Normalize date filters (STRING -> DateTime?)
        // -------------------------
        var monthStart = DashboardDateParser.ParseMonthStart(input.Month);

        DateTime? from = DashboardDateParser.ParseDate(input.FromDate)?.Date;
        DateTime? to = DashboardDateParser.ParseDate(input.ToDate)?.Date;

        if (monthStart.HasValue)
        {
            var monthFrom = monthStart.Value.Date;
            var monthTo = monthStart.Value.AddMonths(1).AddDays(-1).Date;

            from ??= monthFrom;
            to ??= monthTo;
        }

        // If From/To both exist and swapped, fix it
        if (from.HasValue && to.HasValue && from.Value > to.Value)
        {
            (from, to) = (to, from);
        }

        // -------------------------
        // Apply filters: Expense Entries
        // -------------------------
        expenseQ = expenseQ
            .WhereIf(input.ExpenseCategoryId.HasValue, x => x.ExpenseCategoryId == input.ExpenseCategoryId!.Value)
            .WhereIf(from.HasValue, x => x.ExpenseDate.Date >= from!.Value)
            .WhereIf(to.HasValue, x => x.ExpenseDate.Date <= to!.Value);

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var f = input.Filter!.Trim();
            expenseQ = expenseQ.Where(x =>
                x.Title.Contains(f) ||
                (x.PaidTo != null && x.PaidTo.Contains(f)) ||
                (x.Remarks != null && x.Remarks.Contains(f))
            );
        }

        // -------------------------
        // Apply filters: Salary Payments
        // -------------------------
        salaryQ = salaryQ
            .WhereIf(from.HasValue, x => x.PaymentDate.Date >= from!.Value)
            .WhereIf(to.HasValue, x => x.PaymentDate.Date <= to!.Value);

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var f = input.Filter!.Trim();
            salaryQ =
                from p in salaryQ
                join st in staffQ on p.StaffId equals st.Id
                where (st.FirstName + " " + st.LastName).Contains(f)
                select p;
        }

        // -------------------------
        // Totals (use AsyncExecuter)
        // -------------------------
        var otherTotal = await AsyncExecuter.SumAsync(expenseQ.Select(x => (decimal?)x.Amount)) ?? 0m;
        var salaryTotal = await AsyncExecuter.SumAsync(salaryQ.Select(x => (decimal?)x.SalaryAmount)) ?? 0m;

        var otherCount = await AsyncExecuter.CountAsync(expenseQ);
        var salaryCount = await AsyncExecuter.CountAsync(salaryQ);

        var dto = new ExpensesDashboardDto
        {
            TotalOtherExpenses = otherTotal,
            TotalSalaryPayments = salaryTotal,
            TotalExpenses = otherTotal + salaryTotal,
            TotalTransactions = otherCount + salaryCount
        };

        // -------------------------
        // ByCategory (ExpenseEntry only)
        // -------------------------
        dto.ByCategory =
            await AsyncExecuter.ToListAsync(
                (from e in expenseQ
                 join c in catQ on e.ExpenseCategoryId equals c.Id
                 group new { e, c } by new { e.ExpenseCategoryId, c.Name } into g
                 select new ExpensesByCategoryDto
                 {
                     ExpenseCategoryId = g.Key.ExpenseCategoryId,
                     ExpenseCategoryName = g.Key.Name,
                     Amount = g.Sum(x => x.e.Amount),
                     Count = g.Count()
                 })
                .OrderByDescending(x => x.Amount)
                .Take(50)
            );

        // -------------------------
        // ByDay (ExpenseEntry + SalaryPayment combined)
        // -------------------------
        var byDayOther =
            await AsyncExecuter.ToListAsync(
                expenseQ
                    .GroupBy(x => x.ExpenseDate.Date)
                    .Select(g => new { Date = g.Key, Amount = g.Sum(x => x.Amount), Count = g.Count() })
            );

        var byDaySalary =
            await AsyncExecuter.ToListAsync(
                salaryQ
                    .GroupBy(x => x.PaymentDate.Date)
                    .Select(g => new { Date = g.Key, Amount = g.Sum(x => x.SalaryAmount), Count = g.Count() })
            );

        dto.ByDay =
            byDayOther
                .Concat(byDaySalary)
                .GroupBy(x => x.Date)
                .Select(g => new ExpensesByDayDto
                {
                    Date = g.Key,
                    Amount = g.Sum(x => x.Amount),
                    Count = g.Sum(x => x.Count)
                })
                .OrderBy(x => x.Date)
                .ToList();

        // -------------------------
        // Recent transactions (merged)
        // -------------------------
        var recentOther =
            await AsyncExecuter.ToListAsync(
                (from e in expenseQ
                 join c in catQ on e.ExpenseCategoryId equals c.Id
                 select new RecentExpenseDto
                 {
                     Date = e.ExpenseDate,
                     Source = "Expense Entry",
                     Title = e.Title,
                     CategoryOrStaff = c.Name,
                     Amount = e.Amount
                 })
                .OrderByDescending(x => x.Date)
                .Take(50)
            );

        var recentSalary =
            await AsyncExecuter.ToListAsync(
                (from p in salaryQ
                 join st in staffQ on p.StaffId equals st.Id
                 select new RecentExpenseDto
                 {
                     Date = p.PaymentDate,
                     Source = "Salary Payment",
                     Title = "Salary",
                     CategoryOrStaff = (st.FirstName + " " + st.LastName).Trim(),
                     Amount = p.SalaryAmount
                 })
                .OrderByDescending(x => x.Date)
                .Take(50)
            );

        dto.RecentExpenses =
            recentOther
                .Concat(recentSalary)
                .OrderByDescending(x => x.Date)
                .Take(20)
                .ToList();

        return dto;
    }
    internal static class DashboardDateParser
    {
        public static DateTime? ParseDate(string? input)
        {
            if (input.IsNullOrWhiteSpace())
                return null;

            var s = input!.Trim();

            // Remove timezone name like: "(Pakistan Standard Time)"
            var paren = s.IndexOf(" (", StringComparison.Ordinal);
            if (paren > 0)
                s = s[..paren].Trim();

            // Try exact formats first (fast and safe)
            var formats = new[]
            {
            "yyyy-MM-dd",
            "yyyy-MM",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
            "yyyy-MM-ddTHH:mm:ssK",
        };

            if (DateTime.TryParseExact(
                    s,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                    out var exact))
            {
                return exact;
            }

            // Fallback: parses strings like "Sun Feb 01 2026 00:00:00 GMT+0500"
            if (DateTimeOffset.TryParse(
                    s,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                    out var dto))
            {
                return dto.LocalDateTime;
            }

            return null;
        }

        public static DateTime? ParseMonthStart(string? input)
        {
            var dt = ParseDate(input);
            if (!dt.HasValue) return null;
            return new DateTime(dt.Value.Year, dt.Value.Month, 1);
        }
    }
}
