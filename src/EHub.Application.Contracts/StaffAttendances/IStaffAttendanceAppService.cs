using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace EHub.StaffAttendances;

public interface IStaffAttendanceAppService : IApplicationService
{
    Task<StaffAttendanceDto> GetAsync(Guid id);

    Task<PagedResultDto<StaffAttendanceDto>> GetListAsync(GetStaffAttendanceListDto input);

    Task<StaffAttendanceDto> MarkAsync(MarkStaffAttendanceDto input);

    Task DeleteAsync(Guid id);

    Task<IRemoteStreamContent> DownLoadTemplateAsync(GenerateStaffAttendanceTemplateDto input);

    Task<ImportStaffAttendanceResultDto> ImportFromExcelAsync(IRemoteStreamContent file);

    Task<StaffAttendanceLeaderboardDto> GetAttendanceLeaderboardAsync(GetStaffAttendanceLeaderboardDto input);
}
