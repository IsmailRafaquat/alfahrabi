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

[Authorize(EHubPermissions.ShopBankTransactions.Default)]
public class ShopBankTransactionAppService : ApplicationService, IShopBankTransactionAppService
{
    private readonly IRepository<ShopBankTransaction, Guid> _transactionRepository;
    private readonly IRepository<ShopBankAccount, Guid> _accountRepository;
    private readonly ShopBankAccountManager _manager;

    public ShopBankTransactionAppService(
        IRepository<ShopBankTransaction, Guid> transactionRepository,
        IRepository<ShopBankAccount, Guid> accountRepository,
        ShopBankAccountManager manager)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopBankTransactionDto>> GetListAsync(GetShopBankTransactionsInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopBankTransactionDto>(0, new List<ShopBankTransactionDto>());
        var tenantId = CurrentTenant.Id.Value;

        var query = await _transactionRepository.GetQueryableAsync();
        var filtered = query.Where(x => x.TenantId == tenantId)
            .WhereIf(input.BankAccountId.HasValue, x => x.BankAccountId == input.BankAccountId)
            .WhereIf(input.TransactionType.HasValue, x => x.TransactionType == input.TransactionType)
            .WhereIf(input.Direction.HasValue, x => x.Direction == input.Direction)
            .WhereIf(input.ReferenceType.HasValue, x => x.ReferenceType == input.ReferenceType)
            .WhereIf(input.DateFrom.HasValue, x => x.TransactionDate >= input.DateFrom!.Value)
            .WhereIf(input.DateTo.HasValue, x => x.TransactionDate <= input.DateTo!.Value)
            .WhereIf(input.MinimumAmount.HasValue, x => x.Amount >= input.MinimumAmount!.Value)
            .WhereIf(input.MaximumAmount.HasValue, x => x.Amount <= input.MaximumAmount!.Value)
            .WhereIf(!input.ReferenceNumber.IsNullOrWhiteSpace(), x => x.ReferenceNumber.Contains(input.ReferenceNumber!))
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x =>
                x.ReferenceNumber.Contains(input.Filter!) || (x.Description != null && x.Description.Contains(input.Filter!)));

        var totalCount = await AsyncExecuter.CountAsync(filtered);
        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "TransactionDate desc, CreationTime desc" : input.Sorting!;
        var rows = await AsyncExecuter.ToListAsync(filtered.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var accountIds = rows.Select(x => x.BankAccountId).Distinct().ToList();
        var accountQuery = await _accountRepository.GetQueryableAsync();
        var accounts = accountQuery.Where(x => accountIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var items = rows.Select(x => MapToDto(x, accounts.GetValueOrDefault(x.BankAccountId))).ToList();
        await HideAmountIfNotAllowedAsync(items);
        return new PagedResultDto<ShopBankTransactionDto>(totalCount, items);
    }

    public async Task<ShopBankTransactionDto> GetAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _transactionRepository.GetQueryableAsync();
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId))
            ?? throw new BusinessException("ShopManagement:BankTransactionNotFound");

        var account = await _manager.GetAccountAsync(entity.BankAccountId, tenantId);
        var dto = MapToDto(entity, account);
        await HideAmountIfNotAllowedAsync(new List<ShopBankTransactionDto> { dto });
        return dto;
    }

    public async Task<ShopBankAccountSummaryDto> GetSummaryAsync(Guid bankAccountId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var tenantId = RequireTenant();
        var account = await _manager.GetAccountAsync(bankAccountId, tenantId);
        var summary = await _manager.ComputeSummaryAsync(bankAccountId, tenantId, dateFrom, dateTo);

        var dto = new ShopBankAccountSummaryDto
        {
            OpeningBalance = account.OpeningBalance,
            CurrentBalance = account.CurrentBalance,
            TotalMoneyIn = summary.TotalMoneyIn,
            TotalMoneyOut = summary.TotalMoneyOut,
            CustomerPayments = summary.CustomerPayments,
            SupplierPayments = summary.SupplierPayments,
            Expenses = summary.Expenses,
            CustomerRefunds = summary.CustomerRefunds,
            CashDeposits = summary.CashDeposits,
            CashWithdrawals = summary.CashWithdrawals,
            BankTransfersIn = summary.BankTransfersIn,
            BankTransfersOut = summary.BankTransfersOut,
            ManualDeposits = summary.ManualDeposits,
            ManualWithdrawals = summary.ManualWithdrawals
        };

        if (!await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopBankAccounts.ViewBalance))
        {
            dto.OpeningBalance = null;
            dto.CurrentBalance = null;
            dto.TotalMoneyIn = null;
            dto.TotalMoneyOut = null;
            dto.CustomerPayments = null;
            dto.SupplierPayments = null;
            dto.Expenses = null;
            dto.CustomerRefunds = null;
            dto.CashDeposits = null;
            dto.CashWithdrawals = null;
            dto.BankTransfersIn = null;
            dto.BankTransfersOut = null;
            dto.ManualDeposits = null;
            dto.ManualWithdrawals = null;
        }

        return dto;
    }

    [Authorize(EHubPermissions.ShopBankTransactions.ManualMovement)]
    public async Task<ShopBankTransactionDto> CreateManualMovementAsync(CreateManualBankMovementDto input)
    {
        var tenantId = RequireTenant();
        var transaction = await _manager.CreateManualMovementAsync(input.BankAccountId, input.TransactionDate, input.Direction,
            input.Amount, input.ReferenceNumber, input.Description);

        var account = await _manager.GetAccountAsync(transaction.BankAccountId, tenantId);
        var dto = MapToDto(transaction, account);
        await HideAmountIfNotAllowedAsync(new List<ShopBankTransactionDto> { dto });
        return dto;
    }

    private static ShopBankTransactionDto MapToDto(ShopBankTransaction entity, ShopBankAccount? account) => new()
    {
        Id = entity.Id,
        BankAccountId = entity.BankAccountId,
        BankAccountCode = account?.Code ?? string.Empty,
        BankAccountName = account?.AccountName ?? string.Empty,
        TransactionDate = entity.TransactionDate,
        TransactionType = entity.TransactionType,
        Direction = entity.Direction,
        Amount = entity.Amount,
        ReferenceType = entity.ReferenceType,
        ReferenceId = entity.ReferenceId,
        ReferenceNumber = entity.ReferenceNumber,
        Description = entity.Description,
        BalanceAfterTransaction = entity.BalanceAfterTransaction,
        IsReversal = entity.IsReversal,
        CreationTime = entity.CreationTime
    };

    private async Task HideAmountIfNotAllowedAsync(List<ShopBankTransactionDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopBankTransactions.ViewAmount)) return;
        foreach (var dto in items)
        {
            dto.Amount = null;
            dto.BalanceAfterTransaction = null;
        }
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}
