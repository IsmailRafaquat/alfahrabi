using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.FeeModule.FeeHeads;

[RemoteService(IsEnabled = true)]
[ControllerName("FeeHeads")]
[Area("app")]
[Route("api/app/fee-heads")]
public class FeeHeadController(IFeeHeadAppService feeHeadAppService) : AbpController, IFeeHeadAppService
{
    [HttpPost]
    public Task<FeeHeadDto> CreateAsync(CreateUpdateFeeHeadDto input)
    {
        return feeHeadAppService.CreateAsync(input);
    }

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
    {
        return feeHeadAppService.DeleteAsync(id);
    }

    [HttpGet("{id}")]
    public Task<FeeHeadDto> GetAsync(Guid id)
    {
        return feeHeadAppService.GetAsync(id);
    }

    [HttpGet]
    public Task<PagedResultDto<FeeHeadDto>> GetListAsync(GetFeeHeadListInput input)
    {
        return feeHeadAppService.GetListAsync(input);
    }

    [HttpPut("{id}")]
    public Task<FeeHeadDto> UpdateAsync(Guid id, CreateUpdateFeeHeadDto input)
    {
        return feeHeadAppService.UpdateAsync(id, input);
    }

    [HttpPut("{id}/active")]
    public Task SetActiveAsync(Guid id, [FromBody] bool isActive)
    {
        return feeHeadAppService.SetActiveAsync(id, isActive);
    }

    [HttpGet("get-fee-lookup")]
    public Task<List<FeeHeadLookupDto>> GetFeeLookupAsync()
    {
        return feeHeadAppService.GetFeeLookupAsync();
    }
}
