using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.StaffDocuments;

[RemoteService(IsEnabled = true)]
[ControllerName("StaffDocuments")]
[Area("app")]
[Route("api/app/staff-documents")]
public class StaffDocumentController : AbpController, IStaffDocumentAppService
{
    private readonly IStaffDocumentAppService _appService;

    public StaffDocumentController(IStaffDocumentAppService appService)
    {
        _appService = appService;
    }

    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await _appService.DeleteAsync(id);
    }

    [HttpPut("update/{id}")]
    public async Task<StaffDocumentDto> UpdateAsync(Guid id, UpdateStaffDocumentDto input)
    {
        return await _appService.UpdateAsync(id, input);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("upload")]
    public async Task<StaffDocumentDto> UploadAsync([FromForm] IFormFile file, [FromForm] CreateStaffDocumentDto input)
    {
        return await _appService.UploadAsync(file, input);
    }
}
