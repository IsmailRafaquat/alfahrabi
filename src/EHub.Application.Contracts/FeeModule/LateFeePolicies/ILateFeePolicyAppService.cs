using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.FeeModule.LateFeePolicies;

public interface ILateFeePolicyAppService : IApplicationService
{
    Task<LateFeePolicyDto> GetAsync(Guid id);

    Task<PagedResultDto<LateFeePolicyDto>> GetListAsync(GetLateFeePolicyListDto input);

    Task<LateFeePolicyDto> CreateAsync(CreateUpdateLateFeePolicyDto input);

    Task UpdateAsync(Guid id, CreateUpdateLateFeePolicyDto input);

    Task DeleteAsync(Guid id);

    Task<LateFeePolicyDto?> GetApplicablePolicyAsync(
        int? gradeLevel,
        int? section,
        int? shift,
        int? term);
}
