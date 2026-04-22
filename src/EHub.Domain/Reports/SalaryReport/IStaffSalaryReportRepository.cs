using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EHub.Reports.SalaryReport;

public interface IStaffSalaryReportRepository
{
    Task<StaffSalaryReport> GetStaffSalaryReportAsync(
       DateTime periodStart,
       DateTime periodEnd,
       string? filter,
       List<Guid>? staffIds = null
   );
}
