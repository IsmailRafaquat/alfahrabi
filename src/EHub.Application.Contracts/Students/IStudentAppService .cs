using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace EHub.Students;

public interface IStudentAppService : IApplicationService
{
    Task<StudentDto> GetAsync(Guid id);

    Task<PagedResultDto<StudentDto>> GetListAsync(GetStudentListDto input);

    Task<StudentDto> CreateAsync(CreateStudentDto input);

    Task UpdateAsync(Guid id, UpdateStudentDto input);

    Task DeleteAsync(Guid id);
    Task<List<StudentLookupDto>> GetStudentLookupAsync();
    Task<IRemoteStreamContent> DownloadImportTemplateAsync(GenerateStudentImportTemplateDto input);
    Task<ImportStudentResultDto> ImportFromExcelAsync(IRemoteStreamContent file);

}
