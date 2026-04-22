using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EHub.Reports.SalaryReport;

public interface IStaffSalaryReportAppService : IApplicationService
{
    Task<StaffSalaryReportDto> GetStaffSalaryReportAsync(StaffSalaryReportFilterDto input);
}
