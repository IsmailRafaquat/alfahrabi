using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace EHub.StudentDocuments;

[RemoteService(isEnabled: false)]
public class StudentDocumentAppService : ApplicationService, IStudentDocumentAppService
{
    private readonly StudentDocumentManager _manager;

    public StudentDocumentAppService(StudentDocumentManager manager)
    {
        _manager = manager;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _manager.DeleteAsync(id);
    }

    public async Task<StudentDocumentDto> UpdateAsync(Guid id, UpdateStudentDocumentDto input)
    {
        var data = await _manager.UpdateAsync(
            id,
            input.StudentId,
            input.DocumentType,
            input.IssueDate,
            input.ExpireDate,
            input.Description,
            input.IsVerified);

        return ObjectMapper.Map<StudentDocument, StudentDocumentDto>(data);
    }

    public async Task<StudentDocumentDto> UploadAsync(IFormFile file, CreateStudentDocumentDto input)
    {
        var result = await _manager.CreateAsync(
            input.StudentId,
            input.DocumentType,
            input.IssueDate,
            input.ExpireDate,
            input.Description,
            input.IsVerified,
            file);

        return ObjectMapper.Map<StudentDocument, StudentDocumentDto>(result);
    }
}
