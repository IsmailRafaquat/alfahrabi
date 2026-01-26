using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.FeeModule.FeeStructures;

[RemoteService(IsEnabled = true)]
[ControllerName("FeeStructures")]
[Area("app")]
[Route("api/app/fee-structures")]
public class FeeStructureController(IFeeStructureAppService appService)
    : AbpController, IFeeStructureAppService
{
    [HttpPost]
    public Task<FeeStructureDto> CreateAsync(CreateUpdateFeeStructureDto input)
        => appService.CreateAsync(input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
        => appService.DeleteAsync(id);

    [HttpGet("{id}")]
    public Task<FeeStructureDto> GetAsync(Guid id)
        => appService.GetAsync(id);

    [HttpGet]
    public Task<PagedResultDto<FeeStructureDto>> GetListAsync(GetFeeStructureListInput input)
        => appService.GetListAsync(input);

    [HttpPut("{id}")]
    public Task<FeeStructureDto> UpdateAsync(Guid id, CreateUpdateFeeStructureDto input)
        => appService.UpdateAsync(id, input);

    [HttpPut("{id}/active")]
    public Task SetActiveAsync(Guid id, [FromBody] bool isActive)
        => appService.SetActiveAsync(id, isActive);

    [HttpGet("fee-structure-lookup")]
    public Task<List<FeeStructureLookupDto>> GetFeeStructureLookupAsync()
    {
        return appService.GetFeeStructureLookupAsync();
    }
}