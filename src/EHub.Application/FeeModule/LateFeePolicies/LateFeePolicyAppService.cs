using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.FeeModule.LateFeePolicies;

[RemoteService(IsEnabled = false)]
public class LateFeePolicyAppService : ApplicationService, ILateFeePolicyAppService
{
    private readonly ILateFeePolicyRepository _repo;
    private readonly LateFeePolicyManager _manager;

    public LateFeePolicyAppService(
        ILateFeePolicyRepository repo,
        LateFeePolicyManager manager)
    {
        _repo = repo;
        _manager = manager;
    }

    public async Task<LateFeePolicyDto> GetAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return ObjectMapper.Map<LateFeePolicy, LateFeePolicyDto>(entity!);
    }

    public async Task<PagedResultDto<LateFeePolicyDto>> GetListAsync(GetLateFeePolicyListDto input)
    {
        if (input.Sorting.IsNullOrWhiteSpace())
        {
            input.Sorting = nameof(LateFeePolicy.CreationTime) + " DESC";
        }

        var totalCount = await _repo.GetCountAsync(
            input.Filter,
            input.GradeLevel,
            input.Section,
            input.Shift,
            input.Term,
            input.Type,
            input.IsActive,
            input.IsGlobal
        );

        var list = await _repo.GetListAsync(
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            input.Filter,
            input.GradeLevel,
            input.Section,
            input.Shift,
            input.Term,
            input.Type,
            input.IsActive,
            input.IsGlobal
        );

        var items = list.Select(x => ObjectMapper.Map<LateFeePolicy, LateFeePolicyDto>(x)).ToList();
        return new PagedResultDto<LateFeePolicyDto>(totalCount, items);
    }

    public async Task<LateFeePolicyDto> CreateAsync(CreateUpdateLateFeePolicyDto input)
    {
        var entity = await _manager.CreateAsync(
            input.GraceDays,
            input.Type,
            input.Value,
            input.GradeLevel,
            input.Section,
            input.Shift,
            input.Term,
            input.IsActive
        );

        entity = await _repo.InsertAsync(entity);

        return ObjectMapper.Map<LateFeePolicy, LateFeePolicyDto>(entity);
    }

    public async Task UpdateAsync(Guid id, CreateUpdateLateFeePolicyDto input)
    {
        var entity = await _repo.GetAsync(id);

        await _manager.UpdateAsync(
            entity,
            input.GraceDays,
            input.Type,
            input.Value,
            input.GradeLevel,
            input.Section,
            input.Shift,
            input.Term,
            input.IsActive
        );

        await _repo.UpdateAsync(entity, autoSave: true);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repo.DeleteAsync(id);
    }

    public async Task<LateFeePolicyDto?> GetApplicablePolicyAsync(
        int? gradeLevel,
        int? section,
        int? shift,
        int? term)
    {
        var entity = await _repo.FindApplicablePolicyAsync(gradeLevel, section, shift, term);
        if (entity == null) return null;

        return ObjectMapper.Map<LateFeePolicy, LateFeePolicyDto>(entity);
    }
}
