using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EHub.Reports.StudentFeeReport;

public interface IStudentFeeReportRepository
{
   Task<List<StudentFeeClassReport>> GetStudentFeeReportAsync(
      DateTime monthStart,
      DateTime monthEnd,
      int? gradeLevel,
      string? filter);
}
