using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EHub.StaffDocuments;

public interface IStaffDocumentAppService : IApplicationService
{
    Task<StaffDocumentDto> UploadAsync(IFormFile file, CreateStaffDocumentDto input);
    Task DeleteAsync(Guid id);
    Task<StaffDocumentDto> UpdateAsync(Guid id, UpdateStaffDocumentDto input);
}
