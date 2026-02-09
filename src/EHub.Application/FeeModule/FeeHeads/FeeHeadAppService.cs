using EHub.FeeModule.FeeStructureItems;
using EHub.FeeModule.StudentMonthlyFeeLines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.FeeModule.FeeHeads;

[RemoteService(IsEnabled = false)]
public class FeeHeadAppService : ApplicationService, IFeeHeadAppService
{
    private readonly IRepository<FeeHead, Guid> _repository;
    private readonly IRepository<FeeStructureItem, Guid> _feeStructureItemRepo;
    private readonly IRepository<StudentMonthlyFeeLine, Guid> _studentMonthlyFeeLineRepo;

    public FeeHeadAppService(
        IRepository<FeeHead, Guid> repository,
        IRepository<FeeStructureItem, Guid> feeStructureItemRepo,
        IRepository<StudentMonthlyFeeLine, Guid> studentMonthlyFeeLineRepo)
    {
        _repository = repository;
        _feeStructureItemRepo = feeStructureItemRepo;
        _studentMonthlyFeeLineRepo = studentMonthlyFeeLineRepo;
    }

    public async Task<FeeHeadDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<FeeHead, FeeHeadDto>(entity);
    }

    public async Task<PagedResultDto<FeeHeadDto>> GetListAsync(GetFeeHeadListInput input)
    {
        var queryable = await _repository.GetQueryableAsync();

        queryable = queryable
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Filter!))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "Name asc" : input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        return new PagedResultDto<FeeHeadDto>(
            totalCount,
            items.Select(x => ObjectMapper.Map<FeeHead, FeeHeadDto>(x)).ToList()
        );
    }

    public async Task<FeeHeadDto> CreateAsync(CreateUpdateFeeHeadDto input)
    {
        await EnsureNameNotExistsAsync(input.Name);

        var entity = new FeeHead(
            GuidGenerator.Create(),
            input.Name,
            input.IsActive
        );

        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<FeeHead, FeeHeadDto>(entity);
    }

    public async Task<FeeHeadDto> UpdateAsync(Guid id, CreateUpdateFeeHeadDto input)
    {
        await EnsureNameNotExistsAsync(input.Name, excludeId: id);

        var entity = await _repository.GetAsync(id);

        entity.ChangeName(input.Name);

        if (input.IsActive) entity.Activate();
        else entity.Deactivate();

        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<FeeHead, FeeHeadDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var blockers = new List<string>();

        if (await _feeStructureItemRepo.AnyAsync(x => x.FeeHeadId == id))
            blockers.Add("Fee Structure Items");

        if (await _studentMonthlyFeeLineRepo.AnyAsync(x => x.FeeHeadId == id))
            blockers.Add("Student Monthly Fees");

        if (blockers.Count > 0)
        {
            throw new UserFriendlyException(
                $"This fee head cannot be deleted because it is used in: {string.Join(", ", blockers)}. " +
                "Please remove those related records first, then try again."
            );
        }

        await _repository.DeleteAsync(id);
    }


    public async Task SetActiveAsync(Guid id, bool isActive)
    {
        var entity = await _repository.GetAsync(id);

        if (isActive) entity.Activate();
        else entity.Deactivate();

        await _repository.UpdateAsync(entity, autoSave: true);
    }

    private async Task EnsureNameNotExistsAsync(string name, Guid? excludeId = null)
    {
        var normalized = name?.Trim();
        if (normalized.IsNullOrWhiteSpace())
        {
            return;
        }

        var queryable = await _repository.GetQueryableAsync();

        // Per-tenant uniqueness (ABP CurrentTenant filter will also apply if you use it,
        // but we explicitly include TenantId for clarity).
        queryable = queryable.Where(x => x.TenantId == CurrentTenant.Id);

        queryable = queryable.Where(x => x.Name == normalized);

        if (excludeId.HasValue)
        {
            queryable = queryable.Where(x => x.Id != excludeId.Value);
        }

        var exists = await AsyncExecuter.AnyAsync(queryable);
        if (exists)
        {
            throw new UserFriendlyException($"Fee head '{normalized}' already exists.");
        }
    }

    public async Task<List<FeeHeadLookupDto>> GetFeeLookupAsync()
    {
        var queryable = await _repository.GetQueryableAsync();

        queryable = queryable.Where(x => x.IsActive == true);

        var items = await AsyncExecuter.ToListAsync(
            queryable.OrderBy(x => x.Name)
        );

        return items.Select(x => new FeeHeadLookupDto
        {
            Id = x.Id,
            Name = x.Name,
        }).ToList();
    }
}
