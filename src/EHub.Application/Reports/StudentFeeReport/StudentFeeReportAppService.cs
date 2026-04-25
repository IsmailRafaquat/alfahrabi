using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace EHub.Reports.StudentFeeReport;

[RemoteService(IsEnabled = false)]
public class StudentFeeReportAppService : ApplicationService, IStudentFeeReportAppService
{
    private readonly IStudentFeeReportRepository _repository;

    public StudentFeeReportAppService(IStudentFeeReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<StudentFeeClassReportDto>> GetListAsync(StudentFeeReportFilterDto input)
    {
        var today = DateTime.Today;

        var start = input.MonthStart ?? new DateTime(today.Year, 1, 1);
        var end = input.MonthEnd ?? new DateTime(today.Year, 12, 1);

        var data = await _repository.GetStudentFeeReportAsync(
            start,
            end,
            input.ClassId,
            input.Filter
        );

        return data.Select(c => new StudentFeeClassReportDto
        {
            ClassId = c.ClassId,
            ClassName = c.ClassName,
            ExpectedAmount = c.ExpectedAmount,
            CollectedAmount = c.CollectedAmount,
            PendingAmount = c.PendingAmount,
            Students = c.Students.Select(s => new StudentFeeStudentReportDto
            {
                StudentId = s.StudentId,
                StudentName = s.StudentName,
                AdmissionNo = s.AdmissionNo,
                ExpectedAmount = s.ExpectedAmount,
                CollectedAmount = s.CollectedAmount,
                PendingAmount = s.PendingAmount,
                PendingMonths = s.PendingMonths
            }).ToList()
        }).ToList();
    }
}
