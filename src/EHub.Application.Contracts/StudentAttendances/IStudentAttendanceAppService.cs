using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace EHub.StudentAttendances;

public interface IStudentAttendanceAppService : IApplicationService
{
    Task<StudentAttendanceDto> GetAsync(Guid id);
    Task<PagedResultDto<StudentAttendanceDto>> GetListAsync(GetStudentAttendanceListDto input);
    Task<StudentAttendanceDto> MarkAsync(MarkStudentAttendanceDto input);
    Task DeleteAsync(Guid id);
    Task<IRemoteStreamContent> DownLoadTemplateAsync(GenerateStudentAttendanceTemplateDto input);
    Task<ImportStudentAttendanceResultDto> ImportFromExcelAsync(IRemoteStreamContent file);
    Task<StudentAttendanceLeaderboardDto> GetAttendanceLeaderboardAsync(GetAttendanceLeaderboardDto input);
    Task<List<ClassStudentAttendanceRowDto>> GetClassStudentsForAttendanceAsync(GetClassStudentsForAttendanceInput input);
    Task BulkMarkClassAttendanceAsync(BulkMarkClassStudentAttendanceDto input);
}
