using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.BankAccounts;

public class ShopBankAccount : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string Code { get; protected set; } = string.Empty;
    public string AccountName { get; protected set; } = string.Empty;
    public string BankName { get; protected set; } = string.Empty;
    public string? AccountNumber { get; protected set; }
    public string? IBAN { get; protected set; }
    public string? BranchName { get; protected set; }

    public decimal OpeningBalance { get; protected set; }
    public decimal CurrentBalance { get; protected set; }

    public bool IsDefault { get; protected set; }
    public bool IsActive { get; protected set; } = true;
    public string? Notes { get; protected set; }

    protected ShopBankAccount() { }

    internal ShopBankAccount(
        Guid id,
        Guid tenantId,
        string code,
        string accountName,
        string bankName,
        string? accountNumber,
        string? iban,
        string? branchName,
        decimal openingBalance,
        bool isDefault,
        bool isActive,
        string? notes) : base(id)
    {
        TenantId = tenantId;
        SetValues(code, accountName, bankName, accountNumber, iban, branchName, isDefault, isActive, notes);

        if (openingBalance < 0) throw new BusinessException("ShopManagement:BankAccountOpeningBalanceCannotBeNegative");
        OpeningBalance = openingBalance;
        CurrentBalance = openingBalance;
    }

    internal void Update(
        string code,
        string accountName,
        string bankName,
        string? accountNumber,
        string? iban,
        string? branchName,
        bool isDefault,
        bool isActive,
        string? notes) =>
        SetValues(code, accountName, bankName, accountNumber, iban, branchName, isDefault, isActive, notes);

    internal void SetAsNotDefault()
    {
        IsDefault = false;
    }

    /// <summary>
    /// The current balance may only ever move through <see cref="ShopBankAccountManager"/>, which validates
    /// sufficiency for outgoing amounts and records the matching immutable <see cref="ShopBankTransaction"/>
    /// in the same unit of work.
    /// </summary>
    internal void ApplyBalanceChange(decimal newBalance)
    {
        if (newBalance < 0) throw new BusinessException("ShopManagement:BankAccountBalanceCannotBeNegative");
        CurrentBalance = newBalance;
    }

    private void SetValues(
        string code,
        string accountName,
        string bankName,
        string? accountNumber,
        string? iban,
        string? branchName,
        bool isDefault,
        bool isActive,
        string? notes)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ShopBankAccountConsts.CodeMaxLength).Trim();
        AccountName = Check.NotNullOrWhiteSpace(accountName, nameof(accountName), ShopBankAccountConsts.AccountNameMaxLength).Trim();
        BankName = Check.NotNullOrWhiteSpace(bankName, nameof(bankName), ShopBankAccountConsts.BankNameMaxLength).Trim();
        AccountNumber = Check.Length(accountNumber?.Trim(), nameof(accountNumber), ShopBankAccountConsts.AccountNumberMaxLength);
        IBAN = Check.Length(iban?.Trim(), nameof(iban), ShopBankAccountConsts.IbanMaxLength);
        BranchName = Check.Length(branchName?.Trim(), nameof(branchName), ShopBankAccountConsts.BranchNameMaxLength);
        IsDefault = isDefault;
        IsActive = isActive;
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopBankAccountConsts.NotesMaxLength);
    }
}
