using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.Students;

[RemoteService(isEnabled: true)]
[ControllerName("Students")]
[Area("app")]
[Route("api/app/students")]

public class StudentController(IStudentAppService studentAppService) : AbpController, IStudentAppService
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
}
