using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.StudentDocuments;

[RemoteService(IsEnabled = true)]
[ControllerName("StudentDocuments")]
[Area("app")]
[Route("api/app/student-documents")]
public class StudentDocumentController : AbpController, IStudentDocumentAppService
{
    private readonly IStudentDocumentAppService _appService;

    public StudentDocumentController(IStudentDocumentAppService appService)
    {
        _appService = appService;
    }

    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await _appService.DeleteAsync(id);
    }

    [HttpPut("update/{id}")]
    public async Task<StudentDocumentDto> UpdateAsync(Guid id, UpdateStudentDocumentDto input)
    {
        return await _appService.UpdateAsync(id, input);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("upload")]
    public async Task<StudentDocumentDto> UploadAsync(
        [FromForm] IFormFile file,
        [FromForm] CreateStudentDocumentDto input)
    {
        return await _appService.UploadAsync(file, input);
    }
}
