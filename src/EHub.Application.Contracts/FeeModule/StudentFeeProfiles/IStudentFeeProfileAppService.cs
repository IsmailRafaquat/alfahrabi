using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.FeeModule.StudentFeeProfiles;

public interface IStudentFeeProfileAppService : IApplicationService
{
    Task<StudentFeeProfileDto> GetAsync(Guid id);

    Task<PagedResultDto<StudentFeeProfileDto>> GetListAsync(GetStudentFeeProfileListInput input);

    Task<StudentFeeProfileDto> CreateAsync(CreateUpdateStudentFeeProfileDto input);

    Task UpdateAsync(Guid id, CreateUpdateStudentFeeProfileDto input);

    Task DeleteAsync(Guid id);
    Task<BulkAssignStudentFeeProfileResultDto> BulkAssignAsync(BulkAssignStudentFeeProfileDto input);
}
