using EHub.Expenses.ExpenseEntries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using static EHub.Permissions.EHubPermissions;

namespace EHub.Expenses.ExpenseCategories;

[RemoteService(IsEnabled = false)]
public class ExpenseCategoryAppService : ApplicationService, IExpenseCategoryAppService
{
    private readonly IRepository<ExpenseCategory, Guid> _repository;
    private readonly IRepository<ExpenseEntry, Guid> _expenseRepository;

    public ExpenseCategoryAppService(
        IRepository<ExpenseCategory, Guid> repository,
        IRepository<ExpenseEntry, Guid> expenseRepository)
    {
        _repository = repository;
        _expenseRepository = expenseRepository;
    }

    public async Task<ExpenseCategoryDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<ExpenseCategory, ExpenseCategoryDto>(entity);
    }

    public async Task<PagedResultDto<ExpenseCategoryDto>> GetListAsync(GetExpenseCategoryListInput input)
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

        return new PagedResultDto<ExpenseCategoryDto>(
            totalCount,
            items.Select(x => ObjectMapper.Map<ExpenseCategory, ExpenseCategoryDto>(x)).ToList()
        );
    }

    public async Task<ExpenseCategoryDto> CreateAsync(CreateUpdateExpenseCategoryDto input)
    {
        await EnsureNameNotExistsAsync(input.Name);

        var entity = new ExpenseCategory(
            GuidGenerator.Create(),
            input.Name,
            input.IsActive,
            input.EntryLimitType
        );

        await _repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<ExpenseCategory, ExpenseCategoryDto>(entity);
    }

    public async Task<ExpenseCategoryDto> UpdateAsync(Guid id, CreateUpdateExpenseCategoryDto input)
    {
        await EnsureNameNotExistsAsync(input.Name, excludeId: id);

        var entity = await _repository.GetAsync(id);

        entity.ChangeName(input.Name);
        entity.ChangeEntryLimitType(input.EntryLimitType);

        if (input.IsActive) entity.Activate();
        else entity.Deactivate();

        await _repository.UpdateAsync(entity, autoSave: true);

        return ObjectMapper.Map<ExpenseCategory, ExpenseCategoryDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var blockers = new List<string>();

        if (await _expenseRepository.AnyAsync(x => x.ExpenseCategoryId == id))
        {
            blockers.Add("Expenses");
        }

        if (blockers.Count > 0)
        {
            throw new UserFriendlyException(
                $"This expense category cannot be deleted because it is used in: {string.Join(", ", blockers)}. " +
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

    public async Task<List<ExpenseCategoryLookupDto>> GetExpenseCategoryLookupAsync()
    {
        var queryable = await _repository.GetQueryableAsync();

        queryable = queryable.Where(x => x.IsActive == true);

        var items = await AsyncExecuter.ToListAsync(
            queryable.OrderBy(x => x.Name)
        );

        return items.Select(x => new ExpenseCategoryLookupDto
        {
            Id = x.Id,
            Name = x.Name
        }).ToList();
    }

    private async Task EnsureNameNotExistsAsync(string name, Guid? excludeId = null)
    {
        var normalized = name?.Trim();
        if (normalized.IsNullOrWhiteSpace())
        {
            return;
        }

        var queryable = await _repository.GetQueryableAsync();

        queryable = queryable.Where(x => x.TenantId == CurrentTenant.Id);
        queryable = queryable.Where(x => x.Name == normalized);

        if (excludeId.HasValue)
        {
            queryable = queryable.Where(x => x.Id != excludeId.Value);
        }

        var exists = await AsyncExecuter.AnyAsync(queryable);
        if (exists)
        {
            throw new UserFriendlyException($"Expense category '{normalized}' already exists.");
        }
    }
}
