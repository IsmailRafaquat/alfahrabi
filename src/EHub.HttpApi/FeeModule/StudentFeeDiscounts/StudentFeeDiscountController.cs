using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.FeeModule.StudentFeeDiscounts;

[RemoteService(IsEnabled = true)]
[ControllerName("StudentFeeDiscount")]
[Area("app")]
[Route("api/fee-module/student-fee-discounts")]
public class StudentFeeDiscountController : AbpController, IStudentFeeDiscountAppService
{
    private readonly IStudentFeeDiscountAppService _appService;

    public StudentFeeDiscountController(IStudentFeeDiscountAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public Task<PagedResultDto<StudentFeeDiscountDto>> GetListAsync(GetStudentFeeDiscountListInput input)
    {
        return _appService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public Task<StudentFeeDiscountDto> GetAsync(Guid id)
    {
        return _appService.GetAsync(id);
    }

    [HttpPost]
    public Task<StudentFeeDiscountDto> CreateAsync(CreateUpdateStudentFeeDiscountDto input)
    {
        return _appService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public Task UpdateAsync(Guid id, CreateUpdateStudentFeeDiscountDto input)
    {
        return _appService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
    {
        return _appService.DeleteAsync(id);
    }

    [HttpPost("bulk-assign")]
    public Task<BulkAssignStudentFeeDiscountResultDto> BulkAssignAsync(BulkAssignStudentFeeDiscountDto input)
    {
        return _appService.BulkAssignAsync(input);
    }
}
