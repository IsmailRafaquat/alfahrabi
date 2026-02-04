using EHub.AttendanceStatuss;
using EHub.Expenses.ExpenseCategories;
using EHub.Expenses.ExpenseEntries;
using EHub.Expenses.StaffSalaryPayments;
using EHub.FeeModule.StudentMonthlyFeeLines;
using EHub.StaffAttendances;
using EHub.Staffs;
using EHub.StudentAttendances;
using EHub.Students;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.Dashboards;

[RemoteService(IsEnabled = false)]
public class DashboardAppService : ApplicationService, IDashboardAppService
{
    private readonly IRepository<ExpenseEntry, Guid> _expenseRepo;
    private readonly IRepository<StaffSalaryPayment, Guid> _salaryRepo;

    private readonly IRepository<StudentMonthlyFeeLine, Guid> _feeLineRepo;

    private readonly IRepository<StudentAttendance, Guid> _studentAttendanceRepo;
    private readonly IRepository<StaffAttendance, Guid> _staffAttendanceRepo;

    private readonly IRepository<Student, Guid> _studentRepo;
    private readonly IRepository<Staff, Guid> _staffRepo;
    private readonly IRepository<ExpenseCategory, Guid> _expenseCategoryRepo;

    public DashboardAppService(
        IRepository<ExpenseEntry, Guid> expenseRepo,
        IRepository<StaffSalaryPayment, Guid> salaryRepo,
        IRepository<StudentMonthlyFeeLine, Guid> feeLineRepo,
        IRepository<StudentAttendance, Guid> studentAttendanceRepo,
        IRepository<StaffAttendance, Guid> staffAttendanceRepo,
        IRepository<Student, Guid> studentRepo,
        IRepository<Staff, Guid> staffRepo,
        IRepository<ExpenseCategory, Guid> expenseCategoryRepo)
    {
        _expenseRepo = expenseRepo;
        _expenseCategoryRepo = expenseCategoryRepo;
        _salaryRepo = salaryRepo;
        _feeLineRepo = feeLineRepo;
        _studentAttendanceRepo = studentAttendanceRepo;
        _staffAttendanceRepo = staffAttendanceRepo;
        _studentRepo = studentRepo;
        _staffRepo = staffRepo;
    }

    public async Task<DashboardDto> GetAsync(DashboardInput input)
    {
        var (from, to) = DashboardDateParser.ResolveRange(input.Month, input.FromDate, input.ToDate);

        // ----------------------------
        // EXPENSES (ExpenseEntry + SalaryPayment)
        // ----------------------------
        var expenseQ = (await _expenseRepo.GetQueryableAsync())
            .Where(x => x.ExpenseDate.Date >= from.Date && x.ExpenseDate.Date <= to.Date);

        var salaryQ = (await _salaryRepo.GetQueryableAsync())
            .Where(x => x.PaymentDate.Date >= from.Date && x.PaymentDate.Date <= to.Date);

        var otherTotal = await AsyncExecuter.SumAsync(expenseQ.Select(x => (decimal?)x.Amount)) ?? 0m;
        var salaryTotal = await AsyncExecuter.SumAsync(salaryQ.Select(x => (decimal?)x.SalaryAmount)) ?? 0m;

        var otherCount = await AsyncExecuter.CountAsync(expenseQ);
        var salaryCount = await AsyncExecuter.CountAsync(salaryQ);

        // Trend by day (merge)
        var expenseByDay =
            (await AsyncExecuter.ToListAsync(
                expenseQ.GroupBy(x => x.ExpenseDate.Date)
                        .Select(g => new { Date = g.Key, Amount = g.Sum(x => x.Amount) })
            ))
            .Concat(await AsyncExecuter.ToListAsync(
                salaryQ.GroupBy(x => x.PaymentDate.Date)
                       .Select(g => new { Date = g.Key, Amount = g.Sum(x => x.SalaryAmount) })
            ))
            .GroupBy(x => x.Date)
            .Select(g => new TimePointDto { Date = g.Key, Amount = g.Sum(x => x.Amount) })
            .OrderBy(x => x.Date)
            .ToList();

        var categoryQ = await _expenseCategoryRepo.GetQueryableAsync();

        var expenseTrendByCategory = await AsyncExecuter.ToListAsync(
            from e in expenseQ
            join c in categoryQ on e.ExpenseCategoryId equals c.Id
            group e by new { Day = e.ExpenseDate.Date, c.Name } into g
            select new TimePointByCategoryDto
            {
                Date = g.Key.Day,
                CategoryName = g.Key.Name,
                Amount = g.Sum(x => x.Amount)
            }
        );

        expenseTrendByCategory = expenseTrendByCategory
            .OrderBy(x => x.Date)
            .ThenBy(x => x.CategoryName)
            .ToList();

        var staffQ2 = await _staffRepo.GetQueryableAsync();

        var salaryTrendByStaff = await AsyncExecuter.ToListAsync(
            from p in salaryQ
            join s in staffQ2 on p.StaffId equals s.Id
            group new { p, s } by new
            {
                Day = p.PaymentDate.Date,
                StaffName = ((s.FirstName ?? "") + " " + (s.LastName ?? "")).Trim()
            } into g
            select new TimePointByNameDto
            {
                Date = g.Key.Day,
                Name = g.Key.StaffName,
                Amount = g.Sum(x => x.p.SalaryAmount)
            }
        );

        salaryTrendByStaff = salaryTrendByStaff
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Name)
            .ToList();


        // ----------------------------
        // EARNINGS (Collected Fees = PaidAmount)
        // ----------------------------
        // NOTE: StudentMonthlyFeeLine doesn't have date fields in your snippet.
        // Usually the parent StudentMonthlyFee has Month/DueDate and line is linked.
        // If you DO have PaymentDate/CreationTime on line use that.
        // Here we’ll use CreationTime as "payment posting date" (common in ABP audited entities).
        var feeLineQ = (await _feeLineRepo.GetQueryableAsync())
            .Where(x => x.CreationTime.Date >= from.Date && x.CreationTime.Date <= to.Date);

        var earnedTotal = await AsyncExecuter.SumAsync(feeLineQ.Select(x => (decimal?)x.PaidAmount)) ?? 0m;
        var feeLineCount = await AsyncExecuter.CountAsync(feeLineQ);

        var earnTrend =
            await AsyncExecuter.ToListAsync(
                feeLineQ.GroupBy(x => x.CreationTime.Date)
                        .Select(g => new TimePointDto
                        {
                            Date = g.Key,
                            Amount = g.Sum(x => x.PaidAmount)
                        })
            );

        // ----------------------------
        // STAFF ATTENDANCE (distribution + top/bottom)
        // ----------------------------
        var staffQ = (await _staffRepo.GetQueryableAsync())
            .WhereIf(input.Department.HasValue, s => (int)s.Department == input.Department!.Value);

        var staffAttendanceQ = (await _staffAttendanceRepo.GetQueryableAsync())
            .Where(a => a.AttendanceDate.Date >= from.Date && a.AttendanceDate.Date <= to.Date);

        // Restrict attendance to filtered staff
        staffAttendanceQ =
            from a in staffAttendanceQ
            join s in staffQ on a.StaffId equals s.Id
            select a;

        var staffStatus = await BuildStaffStatusDistributionAsync(staffAttendanceQ);

        var staffLeaderboardBase =
            from a in staffAttendanceQ
            join s in staffQ on a.StaffId equals s.Id
            group new { a, s } by new { s.Id, s.EmployeeCode, s.FirstName, s.LastName } into g
            select new AttendanceLeaderboardItemDto
            {
                Id = g.Key.Id,
                Code = g.Key.EmployeeCode,
                Name = ((g.Key.FirstName ?? "") + " " + (g.Key.LastName ?? "")).Trim(),

                TotalDays = g.Count(),
                PresentDays = g.Count(x => x.a.Status == AttendanceStatus.Present),
                AbsentDays = g.Count(x => x.a.Status == AttendanceStatus.Absent),
                LateDays = g.Count(x => x.a.Status == AttendanceStatus.Late),

                AttendanceRate = g.Count() == 0
                    ? 0
                    : (double)g.Count(x => x.a.Status == AttendanceStatus.Present) * 100.0 / g.Count()
            };

        var topStaff = await AsyncExecuter.ToListAsync(
            staffLeaderboardBase.OrderByDescending(x => x.AttendanceRate)
                               .ThenByDescending(x => x.PresentDays)
                               .ThenBy(x => x.Name)
                               .Take(10)
        );

        var bottomStaff = await AsyncExecuter.ToListAsync(
            staffLeaderboardBase.OrderBy(x => x.AttendanceRate)
                               .ThenByDescending(x => x.AbsentDays)
                               .ThenBy(x => x.Name)
                               .Take(10)
        );

        // ----------------------------
        // STUDENT ATTENDANCE (distribution + by class + top 3 overall)
        // ----------------------------
        var studentsQ = (await _studentRepo.GetQueryableAsync())
            .WhereIf(input.GradeLevel.HasValue, s => (int)s.GradeLevel == input.GradeLevel!.Value)
            .WhereIf(input.Section.HasValue, s => (int)s.Section == input.Section!.Value)
            .WhereIf(input.Shift.HasValue, s => (int)s.Shift == input.Shift!.Value)
            .WhereIf(input.Term.HasValue, s => (int)s.Term == input.Term!.Value);

        var studentAttendanceQ = (await _studentAttendanceRepo.GetQueryableAsync())
            .Where(a => a.AttendanceDate.Date >= from.Date && a.AttendanceDate.Date <= to.Date);

        studentAttendanceQ =
            from a in studentAttendanceQ
            join s in studentsQ on a.StudentId equals s.Id
            select a;

        var studentStatus = await BuildStudentStatusDistributionAsync(studentAttendanceQ);

        var studentAttendanceByClass =
       await AsyncExecuter.ToListAsync(
           from a in studentAttendanceQ
           join s in studentsQ on a.StudentId equals s.Id
           group new { a, s } by new
           {
               GradeLevel = (int)s.GradeLevel,
               Section = (int)s.Section
           } into g
           select new ClassAttendanceDto
           {
               GradeLevel = g.Key.GradeLevel,
               Section = g.Key.Section,
               TotalDays = g.Count(),
               PresentDays = g.Count(x => x.a.Status == AttendanceStatuss.AttendanceStatus.Present),
               AttendanceRate = g.Count() == 0
                   ? 0
                   : (double)g.Count(x => x.a.Status == AttendanceStatuss.AttendanceStatus.Present) * 100.0 / g.Count()
           }
       );

        studentAttendanceByClass = studentAttendanceByClass
            .OrderByDescending(x => x.AttendanceRate)
            .ThenByDescending(x => x.PresentDays)
            .ToList();


        // Top 3 students overall - ONLY from attendance rows
        var topStudentsOverall =
            await AsyncExecuter.ToListAsync(
                from a in studentAttendanceQ
                join s in studentsQ on a.StudentId equals s.Id
                group new { a, s } by new
                {
                    s.Id,
                    s.AdmissionNo,
                    s.FirstName,
                    s.LastName,
                    GradeLevel = (int)s.GradeLevel,
                    Section = (int)s.Section
                } into g
                select new StudentTopDto
                {
                    StudentId = g.Key.Id,
                    AdmissionNo = g.Key.AdmissionNo,
                    FullName = ((g.Key.FirstName ?? "") + " " + (g.Key.LastName ?? "")).Trim(),
                    GradeLevel = g.Key.GradeLevel,
                    Section = g.Key.Section,
                    TotalDays = g.Count(),
                    PresentDays = g.Count(x => x.a.Status == AttendanceStatuss.AttendanceStatus.Present),
                    AttendanceRate = g.Count() == 0
                        ? 0
                        : (double)g.Count(x => x.a.Status == AttendanceStatuss.AttendanceStatus.Present) * 100.0 / g.Count()
                }
            );

        topStudentsOverall = topStudentsOverall
            .OrderByDescending(x => x.AttendanceRate)
            .ThenByDescending(x => x.PresentDays)
            .ThenBy(x => x.FullName)
            .Take(3)
            .ToList();

        // ----------------------------
        // Compose
        // ----------------------------
        var dto = new DashboardDto
        {
            Kpis = new DashboardKpisDto
            {
                TotalExpense = otherTotal + salaryTotal,
                TotalEarned = earnedTotal,
                NetProfit = earnedTotal - (otherTotal + salaryTotal),
                TotalTransactions = otherCount + salaryCount + feeLineCount,
            },

            ExpenseTrend = expenseByDay,
            EarnTrend = earnTrend.OrderBy(x => x.Date).ToList(),

            StaffStatus = staffStatus,
            StudentStatus = studentStatus,

            TopStaff = topStaff,
            BottomStaff = bottomStaff,

            StudentAttendanceByClass = studentAttendanceByClass,
            TopStudentsOverall = topStudentsOverall,
            ExpenseTrendByCategory = expenseTrendByCategory,
            SalaryTrendByStaff = salaryTrendByStaff,
        };

        return dto;
    }

    private async Task<AttendanceStatusDistributionDto> BuildStudentStatusDistributionAsync(
    IQueryable<StudentAttendance> attendanceQ)
    {
        var summaryQ =
            from a in attendanceQ
            group a by 1 into g
            select new
            {
                Total = g.Count(),

                Present = g.Count(x => x.Status == AttendanceStatuss.AttendanceStatus.Present),
                Absent = g.Count(x => x.Status == AttendanceStatuss.AttendanceStatus.Absent),
                Late = g.Count(x => x.Status == AttendanceStatuss.AttendanceStatus.Late),
                Excused = g.Count(x => x.Status == AttendanceStatuss.AttendanceStatus.Excused),
                Sick = g.Count(x => x.Status == AttendanceStatuss.AttendanceStatus.Sick),
                Leave = g.Count(x => x.Status == AttendanceStatuss.AttendanceStatus.Leave),
                Holiday = g.Count(x => x.Status == AttendanceStatuss.AttendanceStatus.Holiday),
            };

        var s = await AsyncExecuter.FirstOrDefaultAsync(summaryQ);
        if (s == null) return new AttendanceStatusDistributionDto();

        var known = s.Present + s.Absent + s.Late + s.Excused + s.Sick + s.Leave + s.Holiday;

        return new AttendanceStatusDistributionDto
        {
            Total = s.Total,
            Present = s.Present,
            Absent = s.Absent,
            Late = s.Late,
            Excused = s.Excused,
            Sick = s.Sick,
            Leave = s.Leave,
            Holiday = s.Holiday,
            Other = Math.Max(0, s.Total - known)
        };
    }

    private async Task<AttendanceStatusDistributionDto> BuildStaffStatusDistributionAsync(
    IQueryable<StaffAttendance> attendanceQ)
    {
        var summaryQ =
            from a in attendanceQ
            group a by 1 into g
            select new
            {
                Total = g.Count(),

                Present = g.Count(x => x.Status == AttendanceStatus.Present),
                Absent = g.Count(x => x.Status == AttendanceStatus.Absent),
                Late = g.Count(x => x.Status == AttendanceStatus.Late),
                Excused = g.Count(x => x.Status == AttendanceStatus.Excused),
                Sick = g.Count(x => x.Status == AttendanceStatus.Sick),
                Leave = g.Count(x => x.Status == AttendanceStatus.Leave),
                Holiday = g.Count(x => x.Status == AttendanceStatus.Holiday),
            };

        var s = await AsyncExecuter.FirstOrDefaultAsync(summaryQ);
        if (s == null) return new AttendanceStatusDistributionDto();

        var known = s.Present + s.Absent + s.Late + s.Excused + s.Sick + s.Leave + s.Holiday;

        return new AttendanceStatusDistributionDto
        {
            Total = s.Total,
            Present = s.Present,
            Absent = s.Absent,
            Late = s.Late,
            Excused = s.Excused,
            Sick = s.Sick,
            Leave = s.Leave,
            Holiday = s.Holiday,
            Other = Math.Max(0, s.Total - known)
        };
    }


    internal static class DashboardDateParser
    {
        public static (DateTime From, DateTime To) ResolveRange(string? month, string? fromDate, string? toDate)
        {
            var monthStart = ParseMonthStart(month);
            DateTime? from = ParseDate(fromDate)?.Date;
            DateTime? to = ParseDate(toDate)?.Date;

            if (monthStart.HasValue)
            {
                var mFrom = monthStart.Value.Date;
                var mTo = monthStart.Value.AddMonths(1).AddDays(-1).Date;

                from ??= mFrom;
                to ??= mTo;
            }

            // defaults if nothing provided
            from ??= DateTime.Today.AddDays(-30).Date;
            to ??= DateTime.Today.Date;

            if (from.Value > to.Value)
                (from, to) = (to, from);

            return (Normalize(from.Value), Normalize(to.Value));
        }

        public static DateTime? ParseDate(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var s = input.Trim();

            var paren = s.IndexOf(" (", StringComparison.Ordinal);
            if (paren > 0) s = s[..paren].Trim();

            var formats = new[]
            {
                "yyyy-MM-dd",
                "yyyy-MM",
                "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
                "yyyy-MM-ddTHH:mm:ssK",
            };

            if (DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out var exact))
                return exact;

            if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out var dto))
                return dto.LocalDateTime;

            return null;
        }

        public static DateTime? ParseMonthStart(string? input)
        {
            var dt = ParseDate(input);
            if (!dt.HasValue) return null;
            return new DateTime(dt.Value.Year, dt.Value.Month, 1);
        }

        private static DateTime Normalize(DateTime d)
            => new(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Unspecified);
    }
}
