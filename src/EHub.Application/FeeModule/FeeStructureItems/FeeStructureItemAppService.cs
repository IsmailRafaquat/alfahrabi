using EHub.FeeModule.FeeHeads;
using EHub.FeeModule.FeeStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.FeeModule.FeeStructureItems;

[RemoteService(IsEnabled = false)]
public class FeeStructureItemAppService : ApplicationService, IFeeStructureItemAppService
{
    private readonly IRepository<FeeStructureItem, Guid> _repository;
    private readonly IRepository<FeeHead, Guid> _feeHeadRepository;
    private readonly IRepository<FeeStructure, Guid> _feeStructureRepository;

    public FeeStructureItemAppService(
        IRepository<FeeStructureItem, Guid> repository,
        IRepository<FeeHead, Guid> feeHeadRepository,
        IRepository<FeeStructure, Guid> feeStructureRepository)
    {
        _repository = repository;
        _feeHeadRepository = feeHeadRepository;
        _feeStructureRepository = feeStructureRepository;
    }

    public async Task<FeeStructureItemDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        var dto = ObjectMapper.Map<FeeStructureItem, FeeStructureItemDto>(entity);

        // Optional FeeHeadName for UI
        var head = await _feeHeadRepository.FindAsync(entity.FeeHeadId);
        dto.FeeHeadName = head?.Name;

        return dto;
    }

    public async Task<PagedResultDto<FeeStructureItemDto>> GetListAsync(GetFeeStructureItemListInput input)
    {
        var queryable = await _repository.GetQueryableAsync();

        queryable = queryable
            .WhereIf(input.FeeStructureId.HasValue, x => x.FeeStructureId == input.FeeStructureId!.Value)
            .WhereIf(input.FeeHeadId.HasValue, x => x.FeeHeadId == input.FeeHeadId!.Value)
            .WhereIf(input.IsMandatory.HasValue, x => x.IsMandatory == input.IsMandatory!.Value);

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "CreationTime desc" : input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        var dtos = items.Select(ObjectMapper.Map<FeeStructureItem, FeeStructureItemDto>).ToList();

        // Optional FeeHeadName batch fill
        var feeHeadIds = dtos.Select(x => x.FeeHeadId).Distinct().ToList();
        var heads = await _feeHeadRepository.GetListAsync(x => feeHeadIds.Contains(x.Id));
        var headDict = heads.ToDictionary(x => x.Id, x => x.Name);

        var feeStructureIds = dtos.Select(x => x.FeeStructureId).Distinct().ToList();
        var feeStructures = await _feeStructureRepository.GetListAsync(x => feeStructureIds.Contains(x.Id));
        var structureDict = feeStructures.ToDictionary(x => x.Id, x => $"{x.GradeLevel} {x.Shift} {x.Term}");

        foreach (var d in dtos)
        {
            if (headDict.TryGetValue(d.FeeHeadId, out var name))
                d.FeeHeadName = name;

            if (structureDict.TryGetValue(d.FeeStructureId, out var structureName))
                d.FeeStructureName = structureName;

        }

        return new PagedResultDto<FeeStructureItemDto>(totalCount, dtos);
    }

    public async Task<FeeStructureItemDto> CreateAsync(CreateUpdateFeeStructureItemDto input)
    {
        await EnsureNotDuplicateAsync(input.FeeStructureId, input.FeeHeadId, excludeId: null);

        var entity = new FeeStructureItem(
            GuidGenerator.Create(),
            input.FeeStructureId,
            input.FeeHeadId,
            input.MonthlyAmount,
            input.IsMandatory
        )
        {
            TenantId = CurrentTenant.Id
        };

        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<FeeStructureItem, FeeStructureItemDto>(entity);
    }

    public async Task<FeeStructureItemDto> UpdateAsync(Guid id, CreateUpdateFeeStructureItemDto input)
    {
        await EnsureNotDuplicateAsync(input.FeeStructureId, input.FeeHeadId, excludeId: id);

        var entity = await _repository.GetAsync(id);

        // FeeStructureId change is allowed (if you want to forbid, remove this line)
        // Because properties are private, we can recreate via domain methods pattern:
        // easiest is to enforce FeeStructureId immutable -> decide your rule.
        // Here: allow change by setting via reflection? Not recommended.
        // We'll enforce immutable FeeStructureId for clean domain.

        if (entity.FeeStructureId != input.FeeStructureId)
            throw new UserFriendlyException("FeeStructure cannot be changed for an existing item.");

        entity.ChangeFeeHead(input.FeeHeadId)
              .ChangeMonthlyAmount(input.MonthlyAmount)
              .ChangeMandatory(input.IsMandatory);

        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<FeeStructureItem, FeeStructureItemDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    private async Task EnsureNotDuplicateAsync(Guid feeStructureId, Guid feeHeadId, Guid? excludeId)
    {
        var q = await _repository.GetQueryableAsync();

        q = q.Where(x =>
            x.TenantId == CurrentTenant.Id &&
            x.FeeStructureId == feeStructureId &&
            x.FeeHeadId == feeHeadId
        );

        if (excludeId.HasValue)
        {
            q = q.Where(x => x.Id != excludeId.Value);
        }

        var exists = await AsyncExecuter.AnyAsync(q);

        if (exists)
        {
            throw new UserFriendlyException("This fee head already exists in the selected fee structure.");
        }
    }
}
