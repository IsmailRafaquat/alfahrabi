using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;

namespace EHub.Students;

[RemoteService(isEnabled: true)]
[ControllerName("Students")]
[Area("app")]
[Route("api/app/students")]

public class StudentController(IStudentAppService studentAppService) : AbpController
{
    [HttpPost]
    public Task<StudentDto> CreateAsync(CreateStudentDto input)
    {
        return studentAppService.CreateAsync(input);
    }

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
    {
        return studentAppService.DeleteAsync(id);
    }

    [HttpGet("{id}")]
    public Task<StudentDto> GetAsync(Guid id)
    {
        return studentAppService.GetAsync(id);
    }

    [HttpGet]
    public Task<PagedResultDto<StudentDto>> GetListAsync(GetStudentListDto input)
    {
        return studentAppService.GetListAsync(input);
    }

    [HttpGet("get-student-lookup-async")]
    public async Task<List<StudentLookupDto>> GetStudentLookupAsync()
    {
        return await studentAppService.GetStudentLookupAsync();
    }

    [HttpPut("{id}")]
    public Task UpdateAsync(Guid id, UpdateStudentDto input)
    {
        return studentAppService.UpdateAsync(id, input);
    }


    [HttpPost("download-import-template")]
    public async Task<IRemoteStreamContent> DownloadImportTemplateAsync(GenerateStudentImportTemplateDto input)
    {
        return await studentAppService.DownloadImportTemplateAsync(input);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("import-excel")]
    [Consumes("multipart/form-data")]
    public async Task<ImportStudentResultDto> ImportExcelAsync([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new UserFriendlyException("Please upload a valid Excel file.");

        await using var stream = file.OpenReadStream();

        var remote = new RemoteStreamContent(
            stream,
            file.FileName,
            file.ContentType ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );

        return await studentAppService.ImportFromExcelAsync(remote);
    }
}
