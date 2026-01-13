using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EHub.StudentDocuments;

public interface IStudentDocumentAppService : IApplicationService
{
    Task<StudentDocumentDto> UploadAsync(IFormFile file, CreateStudentDocumentDto input);
    Task DeleteAsync(Guid id);
    Task<StudentDocumentDto> UpdateAsync(Guid id, UpdateStudentDocumentDto input);
}
