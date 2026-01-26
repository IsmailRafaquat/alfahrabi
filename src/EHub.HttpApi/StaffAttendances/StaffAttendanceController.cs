using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;

namespace EHub.StaffAttendances;

[RemoteService(IsEnabled = true)]
[ControllerName("StaffAttendance")]
[Area("app")]
[Route("api/app/staff-attendance")]
public class StaffAttendanceController : AbpController
{
    private readonly IStaffAttendanceAppService _staffAttendanceAppService;

    public StaffAttendanceController(IStaffAttendanceAppService staffAttendanceAppService)
    {
        _staffAttendanceAppService = staffAttendanceAppService;
    }

    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await _staffAttendanceAppService.DeleteAsync(id);
    }

    [HttpPost("download-excel-template")]
    public Task<IRemoteStreamContent> DownLoadTemplateAsync([FromBody] GenerateStaffAttendanceTemplateDto input)
    {
        return _staffAttendanceAppService.DownLoadTemplateAsync(input);
    }

    [HttpGet("{id}")]
    public async Task<StaffAttendanceDto> GetAsync(Guid id)
    {
        return await _staffAttendanceAppService.GetAsync(id);
    }

    [HttpGet]
    public async Task<PagedResultDto<StaffAttendanceDto>> GetListAsync(GetStaffAttendanceListDto input)
    {
        return await _staffAttendanceAppService.GetListAsync(input);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("import-excel")]
    public async Task<ImportStaffAttendanceResultDto> ImportFromExcelAsync([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new UserFriendlyException("Please upload a valid Excel file.");

        await using var stream = file.OpenReadStream();

        var remote = new RemoteStreamContent(stream, file.FileName, file.ContentType);

        return await _staffAttendanceAppService.ImportFromExcelAsync(remote);
    }

    [HttpGet("attendance-leaderboard")]
    public Task<StaffAttendanceLeaderboardDto> GetAttendanceLeaderboardAsync([FromQuery] GetStaffAttendanceLeaderboardDto input)
    {
        return _staffAttendanceAppService.GetAttendanceLeaderboardAsync(input);
    }

    [HttpPost]
    public async Task<StaffAttendanceDto> MarkAsync(MarkStaffAttendanceDto input)
    {
        return await _staffAttendanceAppService.MarkAsync(input);
    }
}
