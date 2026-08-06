using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.CashRegisters;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.BankAccounts;

[Authorize(EHubPermissions.ShopBankTransfers.Default)]
public class ShopBankTransferAppService : ApplicationService, IShopBankTransferAppService
{
    private readonly IRepository<ShopBankTransfer, Guid> _repository;
    private readonly IRepository<ShopBankAccount, Guid> _accountRepository;
    private readonly IRepository<ShopCashRegister, Guid> _cashRegisterRepository;
    private readonly ShopBankTransferManager _manager;

    public ShopBankTransferAppService(
        IRepository<ShopBankTransfer, Guid> repository,
        IRepository<ShopBankAccount, Guid> accountRepository,
        IRepository<ShopCashRegister, Guid> cashRegisterRepository,
        ShopBankTransferManager manager)
    {
        _repository = repository;
        _accountRepository = accountRepository;
        _cashRegisterRepository = cashRegisterRepository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopBankTransferDto>> GetListAsync(GetShopBankTransfersInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopBankTransferDto>(0, new List<ShopBankTransferDto>());
        var tenantId = CurrentTenant.Id.Value;

        var query = await _repository.GetQueryableAsync();
        var filtered = query.Where(x => x.TenantId == tenantId)
            .WhereIf(input.TransferType.HasValue, x => x.TransferType == input.TransferType)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.FromBankAccountId.HasValue, x => x.FromBankAccountId == input.FromBankAccountId)
            .WhereIf(input.ToBankAccountId.HasValue, x => x.ToBankAccountId == input.ToBankAccountId)
            .WhereIf(input.DateFrom.HasValue, x => x.TransferDate >= input.DateFrom!.Value)
            .WhereIf(input.DateTo.HasValue, x => x.TransferDate <= input.DateTo!.Value)
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x =>
                x.TransferNumber.Contains(input.Filter!) || (x.ReferenceNumber != null && x.ReferenceNumber.Contains(input.Filter!)));

        var totalCount = await AsyncExecuter.CountAsync(filtered);
        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "TransferDate desc, CreationTime desc" : input.Sorting!;
        var rows = await AsyncExecuter.ToListAsync(filtered.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var items = await MapToDtosAsync(rows);
        await HideAmountIfNotAllowedAsync(items);
        return new PagedResultDto<ShopBankTransferDto>(totalCount, items);
    }

    public async Task<ShopBankTransferDto> GetAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        var dto = (await MapToDtosAsync(new List<ShopBankTransfer> { entity })).Single();
        await HideAmountIfNotAllowedAsync(new List<ShopBankTransferDto> { dto });
        return dto;
    }

    [Authorize(EHubPermissions.ShopBankTransfers.Create)]
    public async Task<ShopBankTransferDto> CreateAsync(CreateUpdateShopBankTransferDto input)
    {
        var entity = await _manager.CreateAsync(input.TransferDate, input.TransferType, input.FromBankAccountId,
            input.ToBankAccountId, input.CashRegisterId, input.Amount, input.ReferenceNumber, input.Notes);
        await _repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopBankTransfers.Edit)]
    public async Task<ShopBankTransferDto> UpdateAsync(Guid id, CreateUpdateShopBankTransferDto input)
    {
        var entity = await FindEntityAsync(id);
        await _manager.UpdateAsync(entity, input.TransferDate, input.TransferType, input.FromBankAccountId,
            input.ToBankAccountId, input.CashRegisterId, input.Amount, input.ReferenceNumber, input.Notes);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopBankTransfers.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        await _manager.ValidateDeleteAsync(entity);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    [Authorize(EHubPermissions.ShopBankTransfers.Post)]
    public async Task<ShopBankTransferDto> PostAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        await _manager.PostAsync(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopBankTransfers.Cancel)]
    public async Task<ShopBankTransferDto> CancelAsync(Guid id, CancelShopBankTransferDto input)
    {
        var entity = await FindEntityAsync(id);
        await _manager.CancelAsync(entity, input.CancellationReason);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    private async Task<List<ShopBankTransferDto>> MapToDtosAsync(List<ShopBankTransfer> entities)
    {
        var bankAccountIds = entities.SelectMany(x => new[] { x.FromBankAccountId, x.ToBankAccountId })
            .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        var accountQuery = await _accountRepository.GetQueryableAsync();
        var accounts = accountQuery.Where(x => bankAccountIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var cashRegisterIds = entities.Where(x => x.CashRegisterId.HasValue).Select(x => x.CashRegisterId!.Value).Distinct().ToList();
        var registerQuery = await _cashRegisterRepository.GetQueryableAsync();
        var registers = registerQuery.Where(x => cashRegisterIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        return entities.Select(entity =>
        {
            accounts.TryGetValue(entity.FromBankAccountId ?? Guid.Empty, out var fromAccount);
            accounts.TryGetValue(entity.ToBankAccountId ?? Guid.Empty, out var toAccount);
            registers.TryGetValue(entity.CashRegisterId ?? Guid.Empty, out var register);

            return new ShopBankTransferDto
            {
                Id = entity.Id,
                TransferNumber = entity.TransferNumber,
                TransferDate = entity.TransferDate,
                TransferType = entity.TransferType,
                FromBankAccountId = entity.FromBankAccountId,
                FromBankAccountName = fromAccount != null ? $"{fromAccount.Code} - {fromAccount.AccountName}" : null,
                ToBankAccountId = entity.ToBankAccountId,
                ToBankAccountName = toAccount != null ? $"{toAccount.Code} - {toAccount.AccountName}" : null,
                CashRegisterId = entity.CashRegisterId,
                CashRegisterName = register != null ? $"{register.Code} - {register.Name}" : null,
                Amount = entity.Amount,
                ReferenceNumber = entity.ReferenceNumber,
                Notes = entity.Notes,
                Status = entity.Status,
                PostedByUserId = entity.PostedByUserId,
                PostedDate = entity.PostedDate,
                CancelledByUserId = entity.CancelledByUserId,
                CancelledDate = entity.CancelledDate,
                CancellationReason = entity.CancellationReason,
                CreationTime = entity.CreationTime
            };
        }).ToList();
    }

    private async Task HideAmountIfNotAllowedAsync(List<ShopBankTransferDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopBankTransfers.ViewAmount)) return;
        foreach (var dto in items) dto.Amount = null;
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopBankTransfer> FindEntityAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId))
            ?? throw new BusinessException("ShopManagement:BankTransferNotFound");
    }
}
