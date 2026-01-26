using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.FeeModule.LateFeePolicies;

[RemoteService(IsEnabled = true)]
[ControllerName("LateFeePolicy")]
[Area("app")]
[Route("api/fee-module/late-fee-policies")]
public class LateFeePolicyController : AbpController, ILateFeePolicyAppService
{
    private readonly ILateFeePolicyAppService _appService;

    public LateFeePolicyController(ILateFeePolicyAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public Task<PagedResultDto<LateFeePolicyDto>> GetListAsync(GetLateFeePolicyListDto input)
    {
        return _appService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public Task<LateFeePolicyDto> GetAsync(Guid id)
    {
        return _appService.GetAsync(id);
    }

    [HttpPost]
    public Task<LateFeePolicyDto> CreateAsync(CreateUpdateLateFeePolicyDto input)
    {
        return _appService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public Task UpdateAsync(Guid id, CreateUpdateLateFeePolicyDto input)
    {
        return _appService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
    {
        return _appService.DeleteAsync(id);
    }

    [HttpGet("applicable")]
    public Task<LateFeePolicyDto?> GetApplicablePolicyAsync(
        int? gradeLevel,
        int? section,
        int? shift,
        int? term)
    {
        return _appService.GetApplicablePolicyAsync(gradeLevel, section, shift, term);
    }
}
