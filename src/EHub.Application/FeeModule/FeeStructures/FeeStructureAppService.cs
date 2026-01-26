using EHub.Students;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.FeeModule.FeeStructures;

[RemoteService(IsEnabled = false)]
public class FeeStructureAppService : ApplicationService, IFeeStructureAppService
{
    private readonly IRepository<FeeStructure, Guid> _repository;

    public FeeStructureAppService(IRepository<FeeStructure, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<FeeStructureDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<FeeStructure, FeeStructureDto>(entity);
    }

    public async Task<PagedResultDto<FeeStructureDto>> GetListAsync(GetFeeStructureListInput input)
    {
        var queryable = await _repository.GetQueryableAsync();

        queryable = queryable
            .WhereIf(input.GradeLevel.HasValue, x => x.GradeLevel == input.GradeLevel!.Value)
            .WhereIf(input.Shift.HasValue, x => x.Shift == input.Shift!.Value)
            .WhereIf(input.Term.HasValue, x => x.Term == input.Term!.Value)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "EffectiveFrom desc" : input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        return new PagedResultDto<FeeStructureDto>(
            totalCount,
            items.Select(ObjectMapper.Map<FeeStructure, FeeStructureDto>).ToList()
        );
    }

    public async Task<FeeStructureDto> CreateAsync(CreateUpdateFeeStructureDto input)
    {
        await EnsureNoOverlapAsync(
            gradeLevel: input.GradeLevel,
            shift: input.Shift,
            term: input.Term,
            from: input.EffectiveFrom,
            to: input.EffectiveTo,
            excludeId: null
        );

        var entity = new FeeStructure(
            GuidGenerator.Create(),
            input.GradeLevel,
            input.Shift,
            input.Term,
            input.EffectiveFrom,
            input.EffectiveTo,
            input.IsActive
        );

        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<FeeStructure, FeeStructureDto>(entity);
    }

    public async Task<FeeStructureDto> UpdateAsync(Guid id, CreateUpdateFeeStructureDto input)
    {
        await EnsureNoOverlapAsync(
            gradeLevel: input.GradeLevel,
            shift: input.Shift,
            term: input.Term,
            from: input.EffectiveFrom,
            to: input.EffectiveTo,
            excludeId: id
        );

        var entity = await _repository.GetAsync(id);

        entity.ChangeCore(input.GradeLevel, input.Shift, input.Term);
        entity.ChangeEffectivePeriod(input.EffectiveFrom, input.EffectiveTo);

        if (input.IsActive) entity.Activate();
        else entity.Deactivate();

        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<FeeStructure, FeeStructureDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task SetActiveAsync(Guid id, bool isActive)
    {
        var entity = await _repository.GetAsync(id);

        if (isActive) entity.Activate();
        else entity.Deactivate();

        await _repository.UpdateAsync(entity, autoSave: true);
    }

    private async Task EnsureNoOverlapAsync(
        GradeLevel gradeLevel,
        Shift shift,
        Term term,
        DateTime from,
        DateTime? to,
        Guid? excludeId
    )
    {
        var fromDate = from.Date;
        var toDate = to?.Date; // null means open-ended

        if (toDate.HasValue && toDate.Value < fromDate)
        {
            throw new UserFriendlyException("Effective To must be greater than or equal to Effective From.");
        }

        var q = await _repository.GetQueryableAsync();

        q = q.Where(x =>
            x.TenantId == CurrentTenant.Id &&
            x.GradeLevel == gradeLevel &&
            x.Shift == shift &&
            x.Term == term
        );

        if (excludeId.HasValue)
        {
            q = q.Where(x => x.Id != excludeId.Value);
        }

        // Overlap logic:
        // existing [a,b] overlaps new [c,d] if:
        // a <= d (or d is null -> infinity) AND c <= b (or b is null -> infinity)
        var overlap = q.Where(x =>
            // x.EffectiveFrom <= newTo (or newTo is infinity)
            (toDate == null || x.EffectiveFrom <= toDate.Value)
            &&
            // newFrom <= x.EffectiveTo (or x.EffectiveTo is infinity)
            (x.EffectiveTo == null || fromDate <= x.EffectiveTo.Value)
        );

        var exists = await AsyncExecuter.AnyAsync(overlap);

        if (exists)
        {
            // Option 1: plain message
            throw new UserFriendlyException("Fee structure already exists for this Grade/Shift/Term in the selected date range.");

            // Option 2 (recommended): localized
            // throw new UserFriendlyException(L["FeeStructureOverlap"]);
        }
    }

    public async Task<List<FeeStructureLookupDto>> GetFeeStructureLookupAsync()
    {
        var queryable = await _repository.GetQueryableAsync();

        queryable = queryable.Where(x => x.IsActive == true);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.EffectiveFrom)
                .ThenBy(x => x.GradeLevel)
                .ThenBy(x => x.Shift)
                .ThenBy(x => x.Term)
        );

        return items.Select(x => new FeeStructureLookupDto
        {
            Id = x.Id,
            DisplayName = $"{x.GradeLevel} - {x.Shift} - {x.Term} ({x.EffectiveFrom:yyyy-MM-dd}"
                          + (x.EffectiveTo.HasValue ? $" to {x.EffectiveTo:yyyy-MM-dd}" : " onwards")
                          + ")"
        }).ToList();
    }
}
