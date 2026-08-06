using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.BankAccounts;

public class ShopBankTransfer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string TransferNumber { get; protected set; } = string.Empty;
    public DateTime TransferDate { get; protected set; }
    public ShopBankTransferType TransferType { get; protected set; }

    public Guid? FromBankAccountId { get; protected set; }
    public ShopBankAccount? FromBankAccount { get; protected set; }

    public Guid? ToBankAccountId { get; protected set; }
    public ShopBankAccount? ToBankAccount { get; protected set; }

    public Guid? CashRegisterId { get; protected set; }

    public decimal Amount { get; protected set; }
    public string? ReferenceNumber { get; protected set; }
    public string? Notes { get; protected set; }
    public ShopBankTransferStatus Status { get; protected set; } = ShopBankTransferStatus.Draft;

    public Guid? PostedByUserId { get; protected set; }
    public DateTime? PostedDate { get; protected set; }
    public Guid? CancelledByUserId { get; protected set; }
    public DateTime? CancelledDate { get; protected set; }
    public string? CancellationReason { get; protected set; }

    protected ShopBankTransfer() { }

    internal ShopBankTransfer(
        Guid id,
        Guid tenantId,
        string transferNumber,
        DateTime transferDate,
        ShopBankTransferType transferType,
        Guid? fromBankAccountId,
        Guid? toBankAccountId,
        Guid? cashRegisterId,
        decimal amount,
        string? referenceNumber,
        string? notes) : base(id)
    {
        TenantId = tenantId;
        TransferNumber = Check.NotNullOrWhiteSpace(transferNumber, nameof(transferNumber), ShopBankAccountConsts.TransferNumberMaxLength);
        Status = ShopBankTransferStatus.Draft;
        SetValues(transferDate, transferType, fromBankAccountId, toBankAccountId, cashRegisterId, amount, referenceNumber, notes);
    }

    internal void Update(
        DateTime transferDate,
        ShopBankTransferType transferType,
        Guid? fromBankAccountId,
        Guid? toBankAccountId,
        Guid? cashRegisterId,
        decimal amount,
        string? referenceNumber,
        string? notes)
    {
        EnsureEditable();
        SetValues(transferDate, transferType, fromBankAccountId, toBankAccountId, cashRegisterId, amount, referenceNumber, notes);
    }

    internal void MarkAsPosted(Guid postedByUserId, DateTime postedDate)
    {
        EnsurePostable();
        Status = ShopBankTransferStatus.Posted;
        PostedByUserId = postedByUserId;
        PostedDate = postedDate;
    }

    internal void MarkAsCancelled(Guid cancelledByUserId, DateTime cancelledDate, string cancellationReason)
    {
        if (Status != ShopBankTransferStatus.Posted) throw new BusinessException("ShopManagement:BankTransferCannotBeCancelled");
        Status = ShopBankTransferStatus.Cancelled;
        CancelledByUserId = cancelledByUserId;
        CancelledDate = cancelledDate;
        CancellationReason = Check.NotNullOrWhiteSpace(cancellationReason, nameof(cancellationReason), ShopBankAccountConsts.CancellationReasonMaxLength).Trim();
    }

    internal void EnsureEditable()
    {
        if (Status != ShopBankTransferStatus.Draft) throw new BusinessException("ShopManagement:BankTransferCannotBeEdited");
    }

    internal void EnsureDeletable()
    {
        if (Status != ShopBankTransferStatus.Draft) throw new BusinessException("ShopManagement:BankTransferCannotBeDeleted");
    }

    internal void EnsurePostable()
    {
        if (Status != ShopBankTransferStatus.Draft) throw new BusinessException("ShopManagement:BankTransferCannotBePosted");
    }

    private void SetValues(
        DateTime transferDate,
        ShopBankTransferType transferType,
        Guid? fromBankAccountId,
        Guid? toBankAccountId,
        Guid? cashRegisterId,
        decimal amount,
        string? referenceNumber,
        string? notes)
    {
        if (amount <= 0) throw new BusinessException("ShopManagement:BankTransferInvalidAmount");

        switch (transferType)
        {
            case ShopBankTransferType.CashToBank:
                if (!cashRegisterId.HasValue) throw new BusinessException("ShopManagement:BankTransferCashRegisterRequired");
                if (!toBankAccountId.HasValue) throw new BusinessException("ShopManagement:BankTransferToBankAccountRequired");
                fromBankAccountId = null;
                break;
            case ShopBankTransferType.BankToCash:
                if (!fromBankAccountId.HasValue) throw new BusinessException("ShopManagement:BankTransferFromBankAccountRequired");
                if (!cashRegisterId.HasValue) throw new BusinessException("ShopManagement:BankTransferCashRegisterRequired");
                toBankAccountId = null;
                break;
            case ShopBankTransferType.BankToBank:
                if (!fromBankAccountId.HasValue) throw new BusinessException("ShopManagement:BankTransferFromBankAccountRequired");
                if (!toBankAccountId.HasValue) throw new BusinessException("ShopManagement:BankTransferToBankAccountRequired");
                if (fromBankAccountId == toBankAccountId) throw new BusinessException("ShopManagement:BankTransferSameAccountNotAllowed");
                cashRegisterId = null;
                break;
        }

        TransferDate = transferDate;
        TransferType = transferType;
        FromBankAccountId = fromBankAccountId;
        ToBankAccountId = toBankAccountId;
        CashRegisterId = cashRegisterId;
        Amount = amount;
        ReferenceNumber = Check.Length(referenceNumber?.Trim(), nameof(referenceNumber), ShopBankAccountConsts.ReferenceNumberMaxLength);
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopBankAccountConsts.NotesMaxLength);
    }
}
