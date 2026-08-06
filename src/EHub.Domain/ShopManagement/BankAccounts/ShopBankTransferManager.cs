using System;
using System.Threading.Tasks;
using EHub.ShopManagement.CashRegisters;
using EHub.ShopManagement.PurchaseOrders;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace EHub.ShopManagement.BankAccounts;

public class ShopBankTransferManager : DomainService
{
    private const string DocumentType = "BankTransfer";
    private const string NumberPrefix = "BT-";

    private readonly IRepository<ShopBankTransfer, Guid> _repository;
    private readonly ShopBankAccountManager _bankAccountManager;
    private readonly ShopCashRegisterManager _cashRegisterManager;
    private readonly ShopDocumentNumberGenerator _numberGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public ShopBankTransferManager(
        IRepository<ShopBankTransfer, Guid> repository,
        ShopBankAccountManager bankAccountManager,
        ShopCashRegisterManager cashRegisterManager,
        ShopDocumentNumberGenerator numberGenerator,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _bankAccountManager = bankAccountManager;
        _cashRegisterManager = cashRegisterManager;
        _numberGenerator = numberGenerator;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<ShopBankTransfer> CreateAsync(
        DateTime transferDate, ShopBankTransferType transferType, Guid? fromBankAccountId, Guid? toBankAccountId,
        Guid? cashRegisterId, decimal amount, string? referenceNumber, string? notes)
    {
        var tenantId = RequireTenant();
        await ValidateAccountsAsync(tenantId, transferType, fromBankAccountId, toBankAccountId);

        var transferNumber = await _numberGenerator.GetNextNumberAsync(tenantId, DocumentType, NumberPrefix);

        return new ShopBankTransfer(GuidGenerator.Create(), tenantId, transferNumber, transferDate, transferType,
            fromBankAccountId, toBankAccountId, cashRegisterId, amount, referenceNumber, notes);
    }

    public async Task UpdateAsync(
        ShopBankTransfer transfer, DateTime transferDate, ShopBankTransferType transferType, Guid? fromBankAccountId,
        Guid? toBankAccountId, Guid? cashRegisterId, decimal amount, string? referenceNumber, string? notes)
    {
        var tenantId = RequireTenantOwnership(transfer);
        await ValidateAccountsAsync(tenantId, transferType, fromBankAccountId, toBankAccountId);

        transfer.Update(transferDate, transferType, fromBankAccountId, toBankAccountId, cashRegisterId, amount, referenceNumber, notes);
    }

    public Task ValidateDeleteAsync(ShopBankTransfer transfer)
    {
        RequireTenantOwnership(transfer);
        transfer.EnsureDeletable();
        return Task.CompletedTask;
    }

    public async Task PostAsync(ShopBankTransfer transfer)
    {
        var tenantId = RequireTenantOwnership(transfer);
        transfer.EnsurePostable();
        await ValidateAccountsAsync(tenantId, transfer.TransferType, transfer.FromBankAccountId, transfer.ToBankAccountId, requireActive: true);

        var description = $"Transfer {transfer.TransferNumber}";

        switch (transfer.TransferType)
        {
            case ShopBankTransferType.CashToBank:
                await _cashRegisterManager.RecordBankTransferCashTransactionAsync(
                    transfer.CashRegisterId!.Value, transfer.TransferDate, ShopCashDirection.Out, transfer.Amount,
                    transfer.Id, transfer.TransferNumber, description);
                await _bankAccountManager.RecordTransactionAsync(
                    tenantId, transfer.ToBankAccountId!.Value, ShopBankTransactionType.CashDeposit, ShopBankDirection.In,
                    transfer.Amount, ShopBankReferenceType.CashRegisterTransfer, transfer.Id, transfer.TransferNumber, description, transfer.TransferDate);
                break;

            case ShopBankTransferType.BankToCash:
                await _bankAccountManager.RecordTransactionAsync(
                    tenantId, transfer.FromBankAccountId!.Value, ShopBankTransactionType.CashWithdrawal, ShopBankDirection.Out,
                    transfer.Amount, ShopBankReferenceType.CashRegisterTransfer, transfer.Id, transfer.TransferNumber, description, transfer.TransferDate);
                await _cashRegisterManager.RecordBankTransferCashTransactionAsync(
                    transfer.CashRegisterId!.Value, transfer.TransferDate, ShopCashDirection.In, transfer.Amount,
                    transfer.Id, transfer.TransferNumber, description);
                break;

            case ShopBankTransferType.BankToBank:
                await _bankAccountManager.RecordTransactionAsync(
                    tenantId, transfer.FromBankAccountId!.Value, ShopBankTransactionType.BankTransferOut, ShopBankDirection.Out,
                    transfer.Amount, ShopBankReferenceType.BankTransfer, transfer.Id, transfer.TransferNumber, description, transfer.TransferDate);
                await _bankAccountManager.RecordTransactionAsync(
                    tenantId, transfer.ToBankAccountId!.Value, ShopBankTransactionType.BankTransferIn, ShopBankDirection.In,
                    transfer.Amount, ShopBankReferenceType.BankTransfer, transfer.Id, transfer.TransferNumber, description, transfer.TransferDate);
                break;
        }

        transfer.MarkAsPosted(_currentUser.GetId(), Clock.Now);
    }

    public async Task CancelAsync(ShopBankTransfer transfer, string cancellationReason)
    {
        RequireTenantOwnership(transfer);
        transfer.MarkAsCancelled(_currentUser.GetId(), Clock.Now, cancellationReason);

        var description = $"Reversal - cancelled transfer {transfer.TransferNumber}";
        var tenantId = transfer.TenantId!.Value;

        switch (transfer.TransferType)
        {
            case ShopBankTransferType.CashToBank:
                await _cashRegisterManager.RecordReversalIfExistsAsync(
                    tenantId, ShopCashTransactionType.CashOut, ShopCashReferenceType.BankTransfer, transfer.Id, transfer.TransferNumber, description, Clock.Now);
                await _bankAccountManager.RecordReversalIfExistsAsync(
                    tenantId, ShopBankTransactionType.CashDeposit, ShopBankReferenceType.CashRegisterTransfer, transfer.Id, transfer.TransferNumber, description, Clock.Now);
                break;

            case ShopBankTransferType.BankToCash:
                await _bankAccountManager.RecordReversalIfExistsAsync(
                    tenantId, ShopBankTransactionType.CashWithdrawal, ShopBankReferenceType.CashRegisterTransfer, transfer.Id, transfer.TransferNumber, description, Clock.Now);
                await _cashRegisterManager.RecordReversalIfExistsAsync(
                    tenantId, ShopCashTransactionType.CashIn, ShopCashReferenceType.BankTransfer, transfer.Id, transfer.TransferNumber, description, Clock.Now);
                break;

            case ShopBankTransferType.BankToBank:
                await _bankAccountManager.RecordReversalIfExistsAsync(
                    tenantId, ShopBankTransactionType.BankTransferOut, ShopBankReferenceType.BankTransfer, transfer.Id, transfer.TransferNumber, description, Clock.Now);
                await _bankAccountManager.RecordReversalIfExistsAsync(
                    tenantId, ShopBankTransactionType.BankTransferIn, ShopBankReferenceType.BankTransfer, transfer.Id, transfer.TransferNumber, description, Clock.Now);
                break;
        }
    }

    private async Task ValidateAccountsAsync(Guid tenantId, ShopBankTransferType transferType, Guid? fromBankAccountId, Guid? toBankAccountId, bool requireActive = false)
    {
        if (fromBankAccountId.HasValue)
        {
            var from = await _bankAccountManager.GetAccountAsync(fromBankAccountId.Value, tenantId);
            if (requireActive && !from.IsActive) throw new BusinessException("ShopManagement:BankAccountInactive");
        }

        if (toBankAccountId.HasValue)
        {
            var to = await _bankAccountManager.GetAccountAsync(toBankAccountId.Value, tenantId);
            if (requireActive && !to.IsActive) throw new BusinessException("ShopManagement:BankAccountInactive");
        }

        if (transferType == ShopBankTransferType.BankToBank && fromBankAccountId == toBankAccountId)
            throw new BusinessException("ShopManagement:BankTransferSameAccountNotAllowed");
    }

    private Guid RequireTenantOwnership(ShopBankTransfer transfer)
    {
        var tenantId = RequireTenant();
        if (transfer.TenantId != tenantId) throw new BusinessException("ShopManagement:BankTransferNotFound");
        return tenantId;
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}
