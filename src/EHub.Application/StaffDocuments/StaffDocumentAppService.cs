using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace EHub.StaffDocuments;

[RemoteService(isEnabled: false)]
public class StaffDocumentAppService : ApplicationService, IStaffDocumentAppService
{
    private readonly StaffDocumentManager _manager;

    public StaffDocumentAppService(StaffDocumentManager manager)
    {
        _manager = manager;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _manager.DeleteAsync(id);
    }

    public async Task<StaffDocumentDto> UpdateAsync(Guid id, UpdateStaffDocumentDto input)
    {
        var data = await _manager.UpdateAsync(
            id,
            input.StaffId,
            input.StaffDT,
            input.IssueDate,
            input.ExpireDate,
            input.Description,
            input.IsVerified);

        return ObjectMapper.Map<StaffDocument, StaffDocumentDto>(data);
    }

    public async Task<StaffDocumentDto> UploadAsync(IFormFile file, CreateStaffDocumentDto input)
    {
        var result = await _manager.CreateAsync(
            input.StaffId,
            input.StaffDT,
            input.IssueDate,
            input.ExpireDate,
            input.Description,
            input.IsVerified,
            file);

        return ObjectMapper.Map<StaffDocument, StaffDocumentDto>(result);
    }
}
