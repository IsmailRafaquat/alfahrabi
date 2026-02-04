using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.Dashboards;

[RemoteService(IsEnabled = true)]
[ControllerName("Dashboard")]
[Area("app")]
[Route("api/app/dashboard")]
public class DashboardController(IDashboardAppService dashboardAppService)
    : AbpController, IDashboardAppService
{
    [HttpPost]
    public Task<DashboardDto> GetAsync(DashboardInput input)
    {
        return dashboardAppService.GetAsync(input);
    }
}
