using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.Staffs;

[RemoteService(IsEnabled = true)]
[ControllerName("Staffs")]
[Area("app")]
[Route("api/app/staffs")]
public class StaffController(IStaffAppService staffAppService) : AbpController, IStaffAppService
{
    [HttpPost]
    public Task<StaffDto> CreateAsync(CreateStaffDto input)
    {
        return staffAppService.CreateAsync(input);
    }

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
    {
        return staffAppService.DeleteAsync(id);
    }

    [HttpGet("{id}")]
    public Task<StaffDto> GetAsync(Guid id)
    {
        return staffAppService.GetAsync(id);
    }

    [HttpGet]
    public Task<PagedResultDto<StaffDto>> GetListAsync(GetStaffListDto input)
    {
        return staffAppService.GetListAsync(input);
    }

    [HttpPut("{id}")]
    public Task UpdateAsync(Guid id, UpdateStaffDto input)
    {
        return staffAppService.UpdateAsync(id, input);
    }
}
