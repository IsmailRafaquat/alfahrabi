using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.FeeModule.StudentMonthlyFees;

[RemoteService]
[Area("app")]
[ControllerName("StudentMonthlyFee")]
[Route("api/fee-module/student-monthly-fees")]
public class StudentMonthlyFeeController : AbpController, IStudentMonthlyFeeAppService
{
    private readonly IStudentMonthlyFeeAppService _appService;

    public StudentMonthlyFeeController(IStudentMonthlyFeeAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public Task<PagedResultDto<StudentMonthlyFeeDto>> GetListAsync(GetStudentMonthlyFeeListInput input)
        => _appService.GetListAsync(input);

    [HttpGet("{id}")]
    public Task<StudentMonthlyFeeDto> GetAsync(Guid id)
        => _appService.GetAsync(id);

    [HttpPost]
    public Task<StudentMonthlyFeeDto> CreateAsync(CreateUpdateStudentMonthlyFeeDto input)
        => _appService.CreateAsync(input);

    [HttpPut("{id}")]
    public Task UpdateAsync(Guid id, CreateUpdateStudentMonthlyFeeDto input)
        => _appService.UpdateAsync(id, input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
        => _appService.DeleteAsync(id);

    [HttpPost("bulk-generate")]
    public Task<BulkGenerateStudentMonthlyFeeResultDto> BulkGenerateAsync(BulkGenerateStudentMonthlyFeeDto input)
        => _appService.BulkGenerateAsync(input);
}
