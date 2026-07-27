using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.BankAccounts;

/// <summary>
/// An immutable bank-ledger entry. No update or delete is ever exposed for this entity.
/// </summary>
public class ShopBankTransaction : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid BankAccountId { get; protected set; }
    public ShopBankAccount? BankAccount { get; protected set; }

    public DateTime TransactionDate { get; protected set; }
    public ShopBankTransactionType TransactionType { get; protected set; }
    public ShopBankDirection Direction { get; protected set; }
    public decimal Amount { get; protected set; }

    public ShopBankReferenceType ReferenceType { get; protected set; }
    public Guid ReferenceId { get; protected set; }
    public string ReferenceNumber { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }

    public decimal BalanceAfterTransaction { get; protected set; }

    /// <summary>
    /// Combined with a unique index on TenantId + ReferenceType + ReferenceId + TransactionType + IsReversal,
    /// this guarantees a given source event (an original posting, or its opposite-direction reversal) can
    /// only ever create one bank transaction (idempotency).
    /// </summary>
    public Guid? SourceItemId { get; protected set; }

    public Guid? ReversalOfTransactionId { get; protected set; }
    public bool IsReversal { get; protected set; }

    public Guid? CreatedByUserId { get; protected set; }
    public DateTime CreationTime { get; protected set; }

    protected ShopBankTransaction() { }

    internal ShopBankTransaction(
        Guid id,
        Guid tenantId,
        Guid bankAccountId,
        DateTime transactionDate,
        ShopBankTransactionType transactionType,
        ShopBankDirection direction,
        decimal amount,
        ShopBankReferenceType referenceType,
        Guid referenceId,
        string? referenceNumber,
        string? description,
        decimal balanceAfterTransaction,
        Guid? sourceItemId,
        Guid? reversalOfTransactionId,
        bool isReversal,
        Guid? createdByUserId,
        DateTime creationTime) : base(id)
    {
        if (amount <= 0) throw new BusinessException("ShopManagement:BankTransactionInvalidAmount");

        TenantId = tenantId;
        BankAccountId = bankAccountId;
        TransactionDate = transactionDate;
        TransactionType = transactionType;
        Direction = direction;
        Amount = amount;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        ReferenceNumber = Check.Length(referenceNumber?.Trim(), nameof(referenceNumber), ShopBankAccountConsts.ReferenceNumberMaxLength) ?? string.Empty;
        Description = Check.Length(description?.Trim(), nameof(description), ShopBankAccountConsts.DescriptionMaxLength);
        BalanceAfterTransaction = balanceAfterTransaction;
        SourceItemId = sourceItemId;
        ReversalOfTransactionId = reversalOfTransactionId;
        IsReversal = isReversal;
        CreatedByUserId = createdByUserId;
        CreationTime = creationTime;
    }
}
