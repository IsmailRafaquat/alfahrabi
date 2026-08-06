using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.BankAccounts;
using EHub.ShopManagement.ExpenseCategories;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.Expenses;

[Authorize(EHubPermissions.ShopExpenses.Default)]
public class ShopExpenseAppService : ApplicationService, IShopExpenseAppService
{
    private readonly IRepository<ShopExpense, Guid> _repository;
    private readonly IRepository<ShopExpenseCategory, Guid> _categoryRepository;
    private readonly IRepository<ShopBankAccount, Guid> _bankAccountRepository;
    private readonly ShopExpenseManager _manager;

    public ShopExpenseAppService(
        IRepository<ShopExpense, Guid> repository,
        IRepository<ShopExpenseCategory, Guid> categoryRepository,
        IRepository<ShopBankAccount, Guid> bankAccountRepository,
        ShopExpenseManager manager)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _bankAccountRepository = bankAccountRepository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopExpenseDto>> GetListAsync(GetShopExpensesInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopExpenseDto>(0, new List<ShopExpenseDto>());
        var tenantId = CurrentTenant.Id.Value;

        var expenses = await _repository.GetQueryableAsync();
        var categories = await _categoryRepository.GetQueryableAsync();

        var filtered = ApplyFilters(expenses.Where(x => x.TenantId == tenantId), input);

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var matchingCategoryIds = await AsyncExecuter.ToListAsync(categories
                .Where(x => x.TenantId == tenantId && (x.Code.Contains(input.Filter!) || x.Name.Contains(input.Filter!)))
                .Select(x => x.Id));

            filtered = filtered.Where(x =>
                x.ExpenseNumber.Contains(input.Filter!) ||
                (x.PaidTo != null && x.PaidTo.Contains(input.Filter!)) ||
                (x.ReferenceNumber != null && x.ReferenceNumber.Contains(input.Filter!)) ||
                (x.Description != null && x.Description.Contains(input.Filter!)) ||
                (x.Notes != null && x.Notes.Contains(input.Filter!)) ||
                matchingCategoryIds.Contains(x.ExpenseCategoryId));
        }

        var totalCount = await AsyncExecuter.CountAsync(filtered);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "ExpenseDate desc, CreationTime desc" : input.Sorting!;
        var pagedExpenses = filtered.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount);

        var projected = from expense in pagedExpenses
                        join category in categories on expense.ExpenseCategoryId equals category.Id
                        select new ShopExpenseDto
                        {
                            Id = expense.Id,
                            ExpenseNumber = expense.ExpenseNumber,
                            ExpenseCategoryId = expense.ExpenseCategoryId,
                            ExpenseCategoryCode = category.Code,
                            ExpenseCategoryName = category.Name,
                            ExpenseDate = expense.ExpenseDate,
                            Amount = expense.Amount,
                            PaymentMethod = expense.PaymentMethod,
                            PaidTo = expense.PaidTo,
                            ReferenceNumber = expense.ReferenceNumber,
                            ChequeNumber = expense.ChequeNumber,
                            BankName = expense.BankName,
                            BankAccountId = expense.BankAccountId,
                            Description = expense.Description,
                            Notes = expense.Notes,
                            Status = expense.Status,
                            PostedDate = expense.PostedDate,
                            CancelledDate = expense.CancelledDate,
                            CancellationReason = expense.CancellationReason,
                            CreationTime = expense.CreationTime
                        };

        var items = await AsyncExecuter.ToListAsync(projected);
        await PopulateBankAccountNamesAsync(items);
        await HideAmountIfNotAllowedAsync(items);
        return new PagedResultDto<ShopExpenseDto>(totalCount, items);
    }

    public async Task<ShopExpenseDto> GetAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        var dto = await MapToDtoAsync(entity);
        await HideAmountIfNotAllowedAsync(new List<ShopExpenseDto> { dto });
        return dto;
    }

    [Authorize(EHubPermissions.ShopExpenses.Create)]
    public async Task<ShopExpenseDto> CreateAsync(CreateUpdateShopExpenseDto input)
    {
        var entity = await _manager.CreateAsync(input.ExpenseCategoryId, input.ExpenseDate, input.Amount, input.PaymentMethod,
            input.PaidTo, input.ReferenceNumber, input.ChequeNumber, input.BankName, input.BankAccountId, input.Description, input.Notes);
        await _repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopExpenses.Edit)]
    public async Task<ShopExpenseDto> UpdateAsync(Guid id, CreateUpdateShopExpenseDto input)
    {
        var entity = await FindEntityAsync(id);
        await _manager.UpdateAsync(entity, input.ExpenseCategoryId, input.ExpenseDate, input.Amount, input.PaymentMethod,
            input.PaidTo, input.ReferenceNumber, input.ChequeNumber, input.BankName, input.BankAccountId, input.Description, input.Notes);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopExpenses.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        await _manager.ValidateDeleteAsync(entity);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    [Authorize(EHubPermissions.ShopExpenses.Post)]
    public async Task<ShopExpenseDto> PostAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        await _manager.PostAsync(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopExpenses.Cancel)]
    public async Task<ShopExpenseDto> CancelAsync(Guid id, CancelShopExpenseDto input)
    {
        var entity = await FindEntityAsync(id);
        await _manager.CancelAsync(entity, input.CancellationReason);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    public async Task<ShopExpenseSummaryDto> GetSummaryAsync(GetShopExpensesInput input)
    {
        var dto = new ShopExpenseSummaryDto();
        if (!CurrentTenant.Id.HasValue) return dto;
        var tenantId = CurrentTenant.Id.Value;

        var expenses = await _repository.GetQueryableAsync();
        var filtered = ApplyFilters(expenses.Where(x => x.TenantId == tenantId), input);

        var rows = await AsyncExecuter.ToListAsync(filtered.Select(x => new { x.Status, x.Amount }));

        dto.TotalPostedExpenses = rows.Where(x => x.Status == ShopExpenseStatus.Posted).Sum(x => x.Amount);
        dto.TotalDraftExpenses = rows.Where(x => x.Status == ShopExpenseStatus.Draft).Sum(x => x.Amount);
        dto.TotalCancelledExpenses = rows.Where(x => x.Status == ShopExpenseStatus.Cancelled).Sum(x => x.Amount);
        dto.PostedExpenseCount = rows.Count(x => x.Status == ShopExpenseStatus.Posted);
        dto.DraftExpenseCount = rows.Count(x => x.Status == ShopExpenseStatus.Draft);
        dto.CancelledExpenseCount = rows.Count(x => x.Status == ShopExpenseStatus.Cancelled);

        await HideSummaryAmountsIfNotAllowedAsync(dto);
        return dto;
    }

    private static IQueryable<ShopExpense> ApplyFilters(IQueryable<ShopExpense> query, GetShopExpensesInput input) =>
        query
            .WhereIf(input.ExpenseCategoryId.HasValue, x => x.ExpenseCategoryId == input.ExpenseCategoryId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.PaymentMethod.HasValue, x => x.PaymentMethod == input.PaymentMethod)
            .WhereIf(input.ExpenseDateFrom.HasValue, x => x.ExpenseDate >= input.ExpenseDateFrom!.Value)
            .WhereIf(input.ExpenseDateTo.HasValue, x => x.ExpenseDate <= input.ExpenseDateTo!.Value)
            .WhereIf(input.MinimumAmount.HasValue, x => x.Amount >= input.MinimumAmount!.Value)
            .WhereIf(input.MaximumAmount.HasValue, x => x.Amount <= input.MaximumAmount!.Value);

    private async Task<ShopExpenseDto> MapToDtoAsync(ShopExpense entity)
    {
        var category = await AsyncExecuter.FirstOrDefaultAsync(
            (await _categoryRepository.GetQueryableAsync()).Where(x => x.Id == entity.ExpenseCategoryId));

        var dto = new ShopExpenseDto
        {
            Id = entity.Id,
            ExpenseNumber = entity.ExpenseNumber,
            ExpenseCategoryId = entity.ExpenseCategoryId,
            ExpenseCategoryCode = category?.Code ?? string.Empty,
            ExpenseCategoryName = category?.Name ?? string.Empty,
            ExpenseDate = entity.ExpenseDate,
            Amount = entity.Amount,
            PaymentMethod = entity.PaymentMethod,
            PaidTo = entity.PaidTo,
            ReferenceNumber = entity.ReferenceNumber,
            ChequeNumber = entity.ChequeNumber,
            BankName = entity.BankName,
            BankAccountId = entity.BankAccountId,
            Description = entity.Description,
            Notes = entity.Notes,
            Status = entity.Status,
            PostedDate = entity.PostedDate,
            CancelledDate = entity.CancelledDate,
            CancellationReason = entity.CancellationReason,
            CreationTime = entity.CreationTime
        };
        await PopulateBankAccountNamesAsync(new List<ShopExpenseDto> { dto });
        return dto;
    }

    private async Task PopulateBankAccountNamesAsync(List<ShopExpenseDto> items)
    {
        var bankAccountIds = items.Where(x => x.BankAccountId.HasValue).Select(x => x.BankAccountId!.Value).Distinct().ToList();
        if (bankAccountIds.Count == 0) return;

        var query = await _bankAccountRepository.GetQueryableAsync();
        var accounts = query.Where(x => bankAccountIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        foreach (var item in items)
        {
            if (item.BankAccountId.HasValue && accounts.TryGetValue(item.BankAccountId.Value, out var account))
            {
                item.BankAccountCode = account.Code;
                item.BankAccountName = account.AccountName;
            }
        }
    }

    private async Task HideAmountIfNotAllowedAsync(List<ShopExpenseDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopExpenses.ViewAmount)) return;
        foreach (var dto in items) dto.Amount = null;
    }

    private async Task HideSummaryAmountsIfNotAllowedAsync(ShopExpenseSummaryDto dto)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopExpenses.ViewAmount)) return;
        dto.TotalPostedExpenses = null;
        dto.TotalDraftExpenses = null;
        dto.TotalCancelledExpenses = null;
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopExpense> FindEntityAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId))
            ?? throw new BusinessException("ShopManagement:ExpenseNotFound");
    }
}
