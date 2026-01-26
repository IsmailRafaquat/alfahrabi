using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.FeeModule.StudentFeeProfiles;

[RemoteService(IsEnabled = true)]
[ControllerName("StudentFeeProfile")]
[Area("app")]
[Route("api/fee-module/student-fee-profiles")]
public class StudentFeeProfileController : AbpController, IStudentFeeProfileAppService
{
    private readonly IStudentFeeProfileAppService _appService;

    public StudentFeeProfileController(IStudentFeeProfileAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public Task<PagedResultDto<StudentFeeProfileDto>> GetListAsync(GetStudentFeeProfileListInput input)
    {
        return _appService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public Task<StudentFeeProfileDto> GetAsync(Guid id)
    {
        return _appService.GetAsync(id);
    }

    [HttpPost]
    public Task<StudentFeeProfileDto> CreateAsync(CreateUpdateStudentFeeProfileDto input)
    {
        return _appService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public Task UpdateAsync(Guid id, CreateUpdateStudentFeeProfileDto input)
    {
        return _appService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
    {
        return _appService.DeleteAsync(id);
    }

    [HttpPost("bulk-assign")]
    public Task<BulkAssignStudentFeeProfileResultDto> BulkAssignAsync(BulkAssignStudentFeeProfileDto input)
    {
        return _appService.BulkAssignAsync(input);
    }

}
