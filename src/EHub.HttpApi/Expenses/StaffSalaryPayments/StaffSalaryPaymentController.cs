using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.Expenses.StaffSalaryPayments;

[RemoteService(IsEnabled = true)]
[ControllerName("StaffSalaryPayments")]
[Area("app")]
[Route("api/app/staff-salary-payments")]
public class StaffSalaryPaymentController(IStaffSalaryPaymentAppService appService)
    : AbpController, IStaffSalaryPaymentAppService
{
    [HttpGet("{id}")]
    public Task<StaffSalaryPaymentDto> GetAsync(Guid id) => appService.GetAsync(id);

    [HttpGet]
    public Task<PagedResultDto<StaffSalaryPaymentDto>> GetListAsync(GetStaffSalaryPaymentListInput input)
        => appService.GetListAsync(input);

    [HttpPost]
    public Task<StaffSalaryPaymentDto> CreateAsync(CreateUpdateStaffSalaryPaymentDto input)
        => appService.CreateAsync(input);

    [HttpPut("{id}")]
    public Task<StaffSalaryPaymentDto> UpdateAsync(Guid id, CreateUpdateStaffSalaryPaymentDto input)
        => appService.UpdateAsync(id, input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id) => appService.DeleteAsync(id);
}
