using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;

namespace EHub.StudentAttendances;

[RemoteService(IsEnabled = true)]
[ControllerName("StudentAttendance")]
[Area("app")]
[Route("api/app/student-attendance")]
public class StudentAttendanceController : AbpController
{
    private readonly IStudentAttendanceAppService _studentAttendanceAppService;

    public StudentAttendanceController(IStudentAttendanceAppService studentAttendanceAppService)
    {
        _studentAttendanceAppService = studentAttendanceAppService;
    }

    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await _studentAttendanceAppService.DeleteAsync(id);
    }

    [HttpPost("download-excel-template")]
    public Task<IRemoteStreamContent> DownLoadTemplateAsync([FromBody] GenerateStudentAttendanceTemplateDto input)
    {
        return _studentAttendanceAppService.DownLoadTemplateAsync(input);
    }

    [HttpGet("{id}")]
    public async Task<StudentAttendanceDto> GetAsync(Guid id)
    {
        return await _studentAttendanceAppService.GetAsync(id);
    }

    [HttpGet]
    public async Task<PagedResultDto<StudentAttendanceDto>> GetListAsync(GetStudentAttendanceListDto input)
    {
        return await _studentAttendanceAppService.GetListAsync(input);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("import-excel")]
    public async Task<ImportStudentAttendanceResultDto> ImportFromExcelAsync([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new UserFriendlyException("Please upload a valid Excel file.");

        await using var stream = file.OpenReadStream();

        var remote = new RemoteStreamContent(stream, file.FileName, file.ContentType);

        return await _studentAttendanceAppService.ImportFromExcelAsync(remote);
    }

    [HttpGet("attendance-leaderboard")]
    public Task<StudentAttendanceLeaderboardDto> GetAttendanceLeaderboardAsync([FromQuery] GetAttendanceLeaderboardDto input)
    {
        return _studentAttendanceAppService.GetAttendanceLeaderboardAsync(input);
    }


    [HttpPost]
    public async Task<StudentAttendanceDto> MarkAsync(MarkStudentAttendanceDto input)
    {
        return await _studentAttendanceAppService.MarkAsync(input);
    }

    [HttpGet("class-students-for-attendance")]
    public Task<List<ClassStudentAttendanceRowDto>> GetClassStudentsForAttendanceAsync(GetClassStudentsForAttendanceInput input)
    {
        return _studentAttendanceAppService.GetClassStudentsForAttendanceAsync(input);
    }

    [HttpPost("bulk-mark-class-attendance")]
    public Task BulkMarkClassAttendanceAsync(BulkMarkClassStudentAttendanceDto input)
    {
        return _studentAttendanceAppService.BulkMarkClassAttendanceAsync(input);
    }
}
