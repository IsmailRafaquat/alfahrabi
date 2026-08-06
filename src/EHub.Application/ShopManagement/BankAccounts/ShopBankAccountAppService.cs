using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.BankAccounts;

[Authorize(EHubPermissions.ShopBankAccounts.Default)]
public class ShopBankAccountAppService : ApplicationService, IShopBankAccountAppService
{
    private readonly IRepository<ShopBankAccount, Guid> _repository;
    private readonly ShopBankAccountManager _manager;

    public ShopBankAccountAppService(
        IRepository<ShopBankAccount, Guid> repository,
        ShopBankAccountManager manager)
    {
        _repository = repository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopBankAccountDto>> GetListAsync(GetShopBankAccountsInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopBankAccountDto>(0, new List<ShopBankAccountDto>());
        var tenantId = CurrentTenant.Id.Value;

        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.TenantId == tenantId)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive)
            .WhereIf(input.IsDefault.HasValue, x => x.IsDefault == input.IsDefault)
            .WhereIf(!input.BankName.IsNullOrWhiteSpace(), x => x.BankName.Contains(input.BankName!))
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x =>
                x.Code.Contains(input.Filter!) || x.AccountName.Contains(input.Filter!) || x.BankName.Contains(input.Filter!) ||
                (x.AccountNumber != null && x.AccountNumber.Contains(input.Filter!)) ||
                (x.IBAN != null && x.IBAN.Contains(input.Filter!)) ||
                (x.BranchName != null && x.BranchName.Contains(input.Filter!)));

        var totalCount = await AsyncExecuter.CountAsync(query);
        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "AccountName asc" : input.Sorting!;
        var entities = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).PageBy(input));
        var items = entities.Select(MapToDto).ToList();
        await HideBalanceIfNotAllowedAsync(items);
        return new PagedResultDto<ShopBankAccountDto>(totalCount, items);
    }

    public async Task<ShopBankAccountDto> GetAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        var dto = MapToDto(entity);
        await HideBalanceIfNotAllowedAsync(new List<ShopBankAccountDto> { dto });
        return dto;
    }

    [Authorize(EHubPermissions.ShopBankAccounts.Create)]
    public async Task<ShopBankAccountDto> CreateAsync(CreateUpdateShopBankAccountDto input)
    {
        var entity = await _manager.CreateAsync(input.Code, input.AccountName, input.BankName, input.AccountNumber,
            input.IBAN, input.BranchName, input.OpeningBalance, input.IsDefault, input.IsActive, input.Notes);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopBankAccounts.Edit)]
    public async Task<ShopBankAccountDto> UpdateAsync(Guid id, CreateUpdateShopBankAccountDto input)
    {
        var entity = await FindEntityAsync(id);
        await _manager.UpdateAsync(entity, input.Code, input.AccountName, input.BankName, input.AccountNumber,
            input.IBAN, input.BranchName, input.IsDefault, input.IsActive, input.Notes);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopBankAccounts.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        await _manager.ValidateDeleteAsync(entity);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    public async Task<ListResultDto<ShopBankAccountLookupDto>> GetLookupAsync()
    {
        if (!CurrentTenant.Id.HasValue) return new ListResultDto<ShopBankAccountLookupDto>(new List<ShopBankAccountLookupDto>());
        var tenantId = CurrentTenant.Id.Value;

        var query = await _repository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(query.Where(x => x.TenantId == tenantId && x.IsActive).OrderBy(x => x.AccountName));
        var items = entities.Select(x => new ShopBankAccountLookupDto
        {
            Id = x.Id,
            Code = x.Code,
            AccountName = x.AccountName,
            BankName = x.BankName,
            IsDefault = x.IsDefault
        }).ToList();
        return new ListResultDto<ShopBankAccountLookupDto>(items);
    }

    private static ShopBankAccountDto MapToDto(ShopBankAccount entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        AccountName = entity.AccountName,
        BankName = entity.BankName,
        AccountNumber = entity.AccountNumber,
        IBAN = entity.IBAN,
        BranchName = entity.BranchName,
        OpeningBalance = entity.OpeningBalance,
        CurrentBalance = entity.CurrentBalance,
        IsDefault = entity.IsDefault,
        IsActive = entity.IsActive,
        Notes = entity.Notes,
        CreationTime = entity.CreationTime
    };

    private async Task HideBalanceIfNotAllowedAsync(List<ShopBankAccountDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopBankAccounts.ViewBalance)) return;
        foreach (var dto in items)
        {
            dto.OpeningBalance = null;
            dto.CurrentBalance = null;
        }
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopBankAccount> FindEntityAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId))
            ?? throw new BusinessException("ShopManagement:BankAccountNotFound");
    }
}
