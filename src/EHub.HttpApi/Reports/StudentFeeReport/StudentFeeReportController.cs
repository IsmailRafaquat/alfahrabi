using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.Reports.StudentFeeReport;

[RemoteService(IsEnabled = true)]
[ControllerName("StudentFeeReport")]
[Area("app")]
[Route("api/app/student-fee-report")]
public class StudentFeeReportController : AbpController
{
    private readonly IStudentFeeReportAppService _appService;

    public StudentFeeReportController(IStudentFeeReportAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public Task<List<StudentFeeClassReportDto>> GetListAsync([FromQuery] StudentFeeReportFilterDto input)
    {
        return _appService.GetListAsync(input);
    }
}
