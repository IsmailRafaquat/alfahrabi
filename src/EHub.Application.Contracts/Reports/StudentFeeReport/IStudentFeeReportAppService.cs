using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EHub.Reports.StudentFeeReport;

public interface IStudentFeeReportAppService : IApplicationService
{
    Task<List<StudentFeeClassReportDto>> GetListAsync(StudentFeeReportFilterDto input);
}
