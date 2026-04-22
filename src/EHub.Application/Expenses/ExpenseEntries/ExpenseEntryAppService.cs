using EHub.Expenses.ExpenseCategories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.Expenses.ExpenseEntries;

[RemoteService(isEnabled: false)]
public class ExpenseEntryAppService : ApplicationService, IExpenseEntryAppService
{
    private readonly IRepository<ExpenseEntry, Guid> _expenseRepo;
    private readonly IRepository<ExpenseCategory, Guid> _categoryRepo;

    public ExpenseEntryAppService(
        IRepository<ExpenseEntry, Guid> expenseRepo,
        IRepository<ExpenseCategory, Guid> categoryRepo)
    {
        _expenseRepo = expenseRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<ExpenseEntryDto> GetAsync(Guid id)
    {
        var queryable = await _expenseRepo.GetQueryableAsync();
        var categories = await _categoryRepo.GetQueryableAsync();

        var dto = (from e in queryable
                   join c in categories on e.ExpenseCategoryId equals c.Id
                   where e.Id == id
                   select new ExpenseEntryDto
                   {
                       Id = e.Id,
                       TenantId = e.TenantId,
                       CreationTime = e.CreationTime,
                       CreatorId = e.CreatorId,
                       LastModificationTime = e.LastModificationTime,
                       LastModifierId = e.LastModifierId,

                       ExpenseDate = e.ExpenseDate,
                       ExpenseCategoryId = e.ExpenseCategoryId,
                       ExpenseCategoryName = c.Name,

                       Title = e.Title,
                       Amount = e.Amount,
                       PaidTo = e.PaidTo,
                       Remarks = e.Remarks
                   }).FirstOrDefault();

        return dto;
    }

    public async Task<PagedResultDto<ExpenseEntryDto>> GetListAsync(GetExpenseEntryListInput input)
    {
        var queryable = await _expenseRepo.GetQueryableAsync();
        var categories = await _categoryRepo.GetQueryableAsync();

        queryable = queryable
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(),
                x => x.Title.Contains(input.Filter!) ||
                     x.PaidTo != null && x.PaidTo.Contains(input.Filter!))
            .WhereIf(input.ExpenseCategoryId.HasValue, x => x.ExpenseCategoryId == input.ExpenseCategoryId!.Value)
            .WhereIf(input.FromDate.HasValue, x => x.ExpenseDate >= input.FromDate!.Value.Date)
            .WhereIf(input.ToDate.HasValue, x => x.ExpenseDate <= input.ToDate!.Value.Date);

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Join to bring CategoryName for grid
        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "ExpenseDate desc" : input.Sorting;

        var items = (from e in queryable
                     join c in categories on e.ExpenseCategoryId equals c.Id
                     select new ExpenseEntryDto
                     {
                         Id = e.Id,
                         TenantId = e.TenantId,
                         CreationTime = e.CreationTime,
                         CreatorId = e.CreatorId,
                         LastModificationTime = e.LastModificationTime,
                         LastModifierId = e.LastModifierId,

                         ExpenseDate = e.ExpenseDate,
                         ExpenseCategoryId = e.ExpenseCategoryId,
                         ExpenseCategoryName = c.Name,

                         Title = e.Title,
                         Amount = e.Amount,
                         PaidTo = e.PaidTo,
                         Remarks = e.Remarks
                     })
                          .OrderBy(sorting)
                          .Skip(input.SkipCount)
                          .Take(input.MaxResultCount)
                          .ToList();

        return new PagedResultDto<ExpenseEntryDto>(totalCount, items);
    }

    public async Task<ExpenseEntryDto> CreateAsync(CreateUpdateExpenseEntryDto input)
    {
        // optional: validate category exists
        await _categoryRepo.GetAsync(input.ExpenseCategoryId);

        await EnsureMonthlyLimitAsync(
            input.ExpenseCategoryId,
            input.ExpenseDate);

        var entity = new ExpenseEntry(
            GuidGenerator.Create(),
            input.ExpenseDate,
            input.ExpenseCategoryId,
            input.Title,
            input.Amount,
            input.PaidTo,
            input.Remarks
        );

        await _expenseRepo.InsertAsync(entity, autoSave: true);

        return await GetAsync(entity.Id);
    }

    public async Task<ExpenseEntryDto> UpdateAsync(Guid id, CreateUpdateExpenseEntryDto input)
    {
        await _categoryRepo.GetAsync(input.ExpenseCategoryId);

        await EnsureMonthlyLimitAsync(
        input.ExpenseCategoryId,
        input.ExpenseDate,
        id
    );

        var entity = await _expenseRepo.GetAsync(id);

        entity.Update(
            input.ExpenseDate,
            input.ExpenseCategoryId,
            input.Title,
            input.Amount,
            input.PaidTo,
            input.Remarks
        );

        await _expenseRepo.UpdateAsync(entity, autoSave: true);

        return await GetAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _expenseRepo.DeleteAsync(id);
    }

    private async Task EnsureMonthlyLimitAsync(
        Guid expenseCategoryId,
        DateTime expenseDate,
        Guid? excludeId = null)
    {
        var category = await _categoryRepo.GetAsync(expenseCategoryId);

        if (category.EntryLimitType != ExpenseEntryLimitType.OneTimePerMonth)
        {
            return;
        }

        var monthStart = new DateTime(expenseDate.Year, expenseDate.Month, 1);
        var nextMonth = monthStart.AddMonths(1);

        var queryable = await _expenseRepo.GetQueryableAsync();

        queryable = queryable
            .Where(x => x.TenantId == CurrentTenant.Id)
            .Where(x => x.ExpenseCategoryId == expenseCategoryId)
            .Where(x => x.ExpenseDate >= monthStart && x.ExpenseDate < nextMonth);

        if (excludeId.HasValue)
        {
            queryable = queryable.Where(x => x.Id != excludeId.Value);
        }

        var exists = await AsyncExecuter.AnyAsync(queryable);

        if (exists)
        {
            throw new UserFriendlyException(
                $"Only one expense entry is allowed in a month for category '{category.Name}'."
            );
        }
    }
}
