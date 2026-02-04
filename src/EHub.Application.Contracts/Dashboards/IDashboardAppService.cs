using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EHub.Dashboards;

public interface IDashboardAppService : IApplicationService
{
    Task<DashboardDto> GetAsync(DashboardInput input);
}
