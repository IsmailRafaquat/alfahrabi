using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.FeeModule.FeeStructureItems;

[RemoteService(IsEnabled = true)]
[ControllerName("FeeStructureItems")]
[Area("app")]
[Route("api/app/fee-structure-items")]
public class FeeStructureItemController(IFeeStructureItemAppService appService)
    : AbpController, IFeeStructureItemAppService
{
    [HttpPost]
    public Task<FeeStructureItemDto> CreateAsync(CreateUpdateFeeStructureItemDto input)
        => appService.CreateAsync(input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
        => appService.DeleteAsync(id);

    [HttpGet("{id}")]
    public Task<FeeStructureItemDto> GetAsync(Guid id)
        => appService.GetAsync(id);

    [HttpGet]
    public Task<PagedResultDto<FeeStructureItemDto>> GetListAsync(GetFeeStructureItemListInput input)
        => appService.GetListAsync(input);

    [HttpPut("{id}")]
    public Task<FeeStructureItemDto> UpdateAsync(Guid id, CreateUpdateFeeStructureItemDto input)
        => appService.UpdateAsync(id, input);
}
