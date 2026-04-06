using Asp.Versioning;
using EHub.FeeModule.StudentMonthlyFees;
using EHub.FeeModule.StudentRecentFeeHistory;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

[RemoteService(IsEnabled = true)]
[Area("app")]
[ControllerName("StudentMonthlyFeeLine")]
[Route("api/fee-module/student-monthly-fee-lines")]
public class StudentMonthlyFeeLineController : AbpController, IStudentMonthlyFeeLineAppService
{
    private readonly IStudentMonthlyFeeLineAppService _appService;

    public StudentMonthlyFeeLineController(IStudentMonthlyFeeLineAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public Task<PagedResultDto<StudentMonthlyFeeLineDto>> GetListAsync(GetStudentMonthlyFeeLineListInput input)
        => _appService.GetListAsync(input);

    [HttpGet("{id}")]
    public Task<StudentMonthlyFeeLineDto> GetAsync(Guid id)
        => _appService.GetAsync(id);

    [HttpPost]
    public Task<StudentMonthlyFeeLineDto> CreateAsync(CreateUpdateStudentMonthlyFeeLineDto input)
        => _appService.CreateAsync(input);

    [HttpPut("{id}")]
    public Task UpdateAsync(Guid id, CreateUpdateStudentMonthlyFeeLineDto input)
        => _appService.UpdateAsync(id, input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
        => _appService.DeleteAsync(id);

    [HttpPost("bulk-generate")]
    public Task<BulkGenerateStudentMonthlyFeeResultDto> BulkGenerateAsync(BulkGenerateStudentMonthlyFeeDto input)
    {
        return _appService.BulkGenerateAsync(input);
    }

    [HttpPost("calculate-amounts")]
    public Task<CalculatedAmountsDto> CalculateAmountsAsync(CalculateFeeLineAmountsInput input)
    {
        return _appService.CalculateAmountsAsync(input);
    }

    [HttpPost("get-dashboard")]
    public Task<CheckFeesDashboardDto> GetDashboardAsync(CheckFeesDashboardInput input)
    {
        return _appService.GetDashboardAsync(input);
    }

    [HttpGet("recent-history")]
    public Task<StudentRecentFeeHistoryDto> GetRecentHistoryAsync(Guid studentMonthlyFeeId, int monthsCount = 6)
    {
        return _appService.GetRecentHistoryAsync(studentMonthlyFeeId, monthsCount);
    }
}
