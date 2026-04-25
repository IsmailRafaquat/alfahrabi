using EHub.EntityFrameworkCore;
using EHub.FeeModule.StudentMonthlyFeeLines;
using EHub.FeeModule.StudentMonthlyFees;
using EHub.Students;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace EHub.Reports.StudentFeeReport;

public class EfCoreStudentFeeReportRepository : IStudentFeeReportRepository
{
    private readonly IDbContextProvider<EHubDbContext> _dbContextProvider;

    public EfCoreStudentFeeReportRepository(IDbContextProvider<EHubDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<List<StudentFeeClassReport>> GetStudentFeeReportAsync(
     DateTime monthStart,
     DateTime monthEnd,
     int? gradeLevel,
     string? filter)
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();

        var flatData = await (
            from monthlyFee in dbContext.Set<StudentMonthlyFee>()

            join student in dbContext.Set<Student>()
                on monthlyFee.StudentId equals student.Id

            join line in dbContext.Set<StudentMonthlyFeeLine>()
                on monthlyFee.Id equals line.StudentMonthlyFeeId

            where monthlyFee.Month >= monthStart
                && monthlyFee.Month <= monthEnd
                && (!gradeLevel.HasValue || (int)student.GradeLevel == gradeLevel.Value)
                && (string.IsNullOrWhiteSpace(filter)
                    || student.FirstName.Contains(filter)
                    || student.AdmissionNo.Contains(filter))

            select new
            {
                ClassId = (int)student.GradeLevel,
                ClassName = student.GradeLevel.ToString(),

                StudentId = student.Id,
                StudentName = student.FirstName + " " + student.LastName,
                AdmissionNo = student.AdmissionNo,

                Month = monthlyFee.Month,

                ExpectedAmount = line.ExpectedAmount,
                CollectedAmount = line.PaidAmount,
                PendingAmount =
                    line.ExpectedAmount
                    - line.DiscountAmount
                    + line.AdjustmentAmount
                    + line.LateFeeAmount
                    - line.PaidAmount
            }
        ).ToListAsync();

        var result = flatData
            .GroupBy(x => new { x.ClassId, x.ClassName })
            .Select(classGroup =>
            {
                var students = classGroup
                    .GroupBy(x => new { x.StudentId, x.StudentName, x.AdmissionNo })
                    .Select(studentGroup =>
                    {
                        var expected = studentGroup.Sum(x => x.ExpectedAmount);
                        var collected = studentGroup.Sum(x => x.CollectedAmount);
                        var pending = studentGroup.Sum(x => x.PendingAmount);

                        var pendingMonths = studentGroup
                            .GroupBy(x => x.Month)
                            .Where(monthGroup => monthGroup.Sum(x => x.PendingAmount) > 0)
                            .OrderBy(monthGroup => monthGroup.Key)
                            .Select(monthGroup => monthGroup.Key.ToString("MMM yyyy"))
                            .ToList();

                        return new StudentFeeStudentReport
                        {
                            StudentId = studentGroup.Key.StudentId,
                            StudentName = studentGroup.Key.StudentName,
                            AdmissionNo = studentGroup.Key.AdmissionNo,
                            ExpectedAmount = expected,
                            CollectedAmount = collected,
                            PendingAmount = pending < 0 ? 0 : pending,
                            PendingMonths = pendingMonths
                        };
                    })
                    .OrderBy(x => x.StudentName)
                    .ToList();

                return new StudentFeeClassReport
                {
                    ClassId = classGroup.Key.ClassId,
                    ClassName = classGroup.Key.ClassName,
                    ExpectedAmount = students.Sum(x => x.ExpectedAmount),
                    CollectedAmount = students.Sum(x => x.CollectedAmount),
                    PendingAmount = students.Sum(x => x.PendingAmount),
                    Students = students
                };
            })
            .OrderBy(x => x.ClassId)
            .ToList();

        return result;
    }
}
