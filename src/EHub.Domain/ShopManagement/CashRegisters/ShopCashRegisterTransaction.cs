using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.CashRegisters;

/// <summary>
/// An immutable cash-drawer ledger entry. No update or delete is ever exposed for this entity.
/// </summary>
public class ShopCashRegisterTransaction : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid CashRegisterId { get; protected set; }
    public ShopCashRegister? CashRegister { get; protected set; }

    public Guid? CashClosingId { get; protected set; }
    public ShopCashClosing? CashClosing { get; protected set; }

    public DateTime TransactionDate { get; protected set; }
    public ShopCashTransactionType TransactionType { get; protected set; }
    public ShopCashDirection Direction { get; protected set; }
    public decimal Amount { get; protected set; }

    public ShopCashReferenceType ReferenceType { get; protected set; }
    public Guid ReferenceId { get; protected set; }
    public string ReferenceNumber { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }

    /// <summary>
    /// Combined with a unique index on TenantId + ReferenceType + ReferenceId + TransactionType + Direction,
    /// this guarantees a given source event (an original posting, or its opposite-direction reversal) can
    /// only ever create one cash transaction (idempotency).
    /// </summary>
    public Guid? SourceItemId { get; protected set; }

    public Guid? CreatedByUserId { get; protected set; }
    public DateTime CreationTime { get; protected set; }

    protected ShopCashRegisterTransaction() { }

    internal ShopCashRegisterTransaction(
        Guid id,
        Guid tenantId,
        Guid cashRegisterId,
        Guid? cashClosingId,
        DateTime transactionDate,
        ShopCashTransactionType transactionType,
        ShopCashDirection direction,
        decimal amount,
        ShopCashReferenceType referenceType,
        Guid referenceId,
        string? referenceNumber,
        string? description,
        Guid? sourceItemId,
        Guid? createdByUserId,
        DateTime creationTime) : base(id)
    {
        if (amount <= 0) throw new BusinessException("ShopManagement:CashTransactionInvalidAmount");

        TenantId = tenantId;
        CashRegisterId = cashRegisterId;
        CashClosingId = cashClosingId;
        TransactionDate = transactionDate;
        TransactionType = transactionType;
        Direction = direction;
        Amount = amount;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        ReferenceNumber = Check.Length(referenceNumber?.Trim(), nameof(referenceNumber), ShopCashRegisterConsts.ReferenceNumberMaxLength) ?? string.Empty;
        Description = Check.Length(description?.Trim(), nameof(description), ShopCashRegisterConsts.NotesMaxLength);
        SourceItemId = sourceItemId;
        CreatedByUserId = createdByUserId;
        CreationTime = creationTime;
    }

    internal void AssignToClosing(Guid cashClosingId)
    {
        CashClosingId = cashClosingId;
    }
}
