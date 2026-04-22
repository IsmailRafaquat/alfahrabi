using Asp.Versioning;
using EHub.Reports.SalaryReport;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.Reports.StaffSalary;

[RemoteService(IsEnabled = true)]
[ControllerName("StaffRepory")]
[Area("app")]
[Route("api/app/staff-report")]
public class StaffSalaryReportController : AbpController
{
    private readonly IStaffSalaryReportAppService _service;

    public StaffSalaryReportController(IStaffSalaryReportAppService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<StaffSalaryReportDto> GetListAsync(StaffSalaryReportFilterDto input)
    {
        return await _service.GetStaffSalaryReportAsync(input);
    }
}
