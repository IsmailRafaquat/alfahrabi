using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.Subjects;

[RemoteService(IsEnabled = true)]
[ControllerName("Subjects")]
[Area("app")]
[Route("api/app/subjects")]
public class SubjectController(ISubjectAppService subjectAppService) : AbpController, ISubjectAppService
{
    [HttpPost]
    public Task<SubjectDto> CreateAsync(CreateSubjectDto input) => subjectAppService.CreateAsync(input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id) => subjectAppService.DeleteAsync(id);

    [HttpGet("{id}")]
    public Task<SubjectDto> GetAsync(Guid id) => subjectAppService.GetAsync(id);

    [HttpGet]
    public Task<PagedResultDto<SubjectDto>> GetListAsync(GetSubjectListDto input) => subjectAppService.GetListAsync(input);

    [HttpPut("{id}")]
    public Task UpdateAsync(Guid id, UpdateSubjectDto input) => subjectAppService.UpdateAsync(id, input);
}
