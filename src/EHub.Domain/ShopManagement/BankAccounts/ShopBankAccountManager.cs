using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace EHub.ShopManagement.BankAccounts;

public class ShopBankAccountManager : DomainService
{
    private readonly IRepository<ShopBankAccount, Guid> _accountRepository;
    private readonly IRepository<ShopBankTransaction, Guid> _transactionRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public ShopBankAccountManager(
        IRepository<ShopBankAccount, Guid> accountRepository,
        IRepository<ShopBankTransaction, Guid> transactionRepository,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    // ---------------------------------------------------------------------
    // Account CRUD
    // ---------------------------------------------------------------------

    public async Task<ShopBankAccount> CreateAsync(
        string code, string accountName, string bankName, string? accountNumber, string? iban, string? branchName,
        decimal openingBalance, bool isDefault, bool isActive, string? notes)
    {
        var tenantId = RequireTenant();
        var normalizedCode = NormalizeCode(code);
        await ValidateUniqueCodeAsync(normalizedCode, tenantId, null);

        if (isDefault) await ClearExistingDefaultAsync(tenantId, null);

        var account = new ShopBankAccount(GuidGenerator.Create(), tenantId, normalizedCode, accountName, bankName,
            accountNumber, iban, branchName, openingBalance, isDefault, isActive, notes);
        await _accountRepository.InsertAsync(account, autoSave: true);

        if (openingBalance > 0)
        {
            var alreadyExists = await ExistsAsync(tenantId, ShopBankReferenceType.OpeningBalance, account.Id, ShopBankTransactionType.OpeningBalance, isReversal: false);
            if (!alreadyExists)
            {
                var transaction = new ShopBankTransaction(
                    GuidGenerator.Create(), tenantId, account.Id, Clock.Now, ShopBankTransactionType.OpeningBalance,
                    ShopBankDirection.In, openingBalance, ShopBankReferenceType.OpeningBalance, account.Id,
                    "OPEN-" + account.Id.ToString("N")[..8].ToUpperInvariant(), "Opening balance", openingBalance,
                    null, null, false, _currentUser.GetId(), Clock.Now);
                await _transactionRepository.InsertAsync(transaction, autoSave: true);
            }
        }

        return account;
    }

    public async Task UpdateAsync(
        ShopBankAccount account, string code, string accountName, string bankName, string? accountNumber, string? iban,
        string? branchName, bool isDefault, bool isActive, string? notes)
    {
        var tenantId = RequireTenantOwnership(account);
        var normalizedCode = NormalizeCode(code);
        await ValidateUniqueCodeAsync(normalizedCode, tenantId, account.Id);

        if (isDefault) await ClearExistingDefaultAsync(tenantId, account.Id);

        account.Update(normalizedCode, accountName, bankName, accountNumber, iban, branchName, isDefault, isActive, notes);
    }

    public async Task ValidateDeleteAsync(ShopBankAccount account)
    {
        RequireTenantOwnership(account);

        var query = await _transactionRepository.GetQueryableAsync();
        var hasTransactions = await AsyncExecuter.AnyAsync(query.Where(x => x.BankAccountId == account.Id));
        if (hasTransactions) throw new BusinessException("ShopManagement:BankAccountHasTransactions");
    }

    public async Task<ShopBankAccount> GetAccountAsync(Guid bankAccountId, Guid tenantId)
    {
        var query = await _accountRepository.GetQueryableAsync();
        var account = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == bankAccountId && x.TenantId == tenantId));
        return account ?? throw new BusinessException("ShopManagement:BankAccountNotFound");
    }

    private async Task ClearExistingDefaultAsync(Guid tenantId, Guid? excludeId)
    {
        var query = await _accountRepository.GetQueryableAsync();
        var others = await AsyncExecuter.ToListAsync(query.Where(x =>
            x.TenantId == tenantId && x.IsDefault && (!excludeId.HasValue || x.Id != excludeId.Value)));

        foreach (var other in others)
        {
            other.SetAsNotDefault();
            await _accountRepository.UpdateAsync(other, autoSave: true);
        }
    }

    private async Task ValidateUniqueCodeAsync(string code, Guid tenantId, Guid? excludedId)
    {
        var query = await _accountRepository.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(query.Where(x =>
            x.TenantId == tenantId && x.Code == code && (!excludedId.HasValue || x.Id != excludedId)));
        if (exists) throw new BusinessException("ShopManagement:BankAccountCodeAlreadyExists").WithData("Code", code);
    }

    // ---------------------------------------------------------------------
    // Automatic / integration transactions
    // ---------------------------------------------------------------------

    /// <summary>
    /// Records a bank transaction for an effective (posted/completed) source document against the given
    /// bank account, updating its <see cref="ShopBankAccount.CurrentBalance"/> in the same unit of work.
    /// No-ops silently if a transaction for this exact source has already been recorded (idempotency).
    /// </summary>
    public async Task<ShopBankTransaction?> RecordTransactionAsync(
        Guid tenantId,
        Guid bankAccountId,
        ShopBankTransactionType transactionType,
        ShopBankDirection direction,
        decimal amount,
        ShopBankReferenceType referenceType,
        Guid referenceId,
        string referenceNumber,
        string? description,
        DateTime transactionDate)
    {
        if (amount <= 0) return null;

        var alreadyExists = await ExistsAsync(tenantId, referenceType, referenceId, transactionType, isReversal: false);
        if (alreadyExists) return null;

        var account = await GetAccountAsync(bankAccountId, tenantId);
        if (!account.IsActive) throw new BusinessException("ShopManagement:BankAccountInactive");

        var newBalance = ApplyDirection(account.CurrentBalance, direction, amount);
        account.ApplyBalanceChange(newBalance);
        await _accountRepository.UpdateAsync(account, autoSave: true);

        var transaction = new ShopBankTransaction(
            GuidGenerator.Create(), tenantId, account.Id, transactionDate, transactionType, direction, amount,
            referenceType, referenceId, referenceNumber, description, newBalance, referenceId, null, false,
            _currentUser.GetId(), Clock.Now);
        await _transactionRepository.InsertAsync(transaction, autoSave: true);

        return transaction;
    }

    /// <summary>
    /// Reverses a previously-recorded bank transaction for a source document that has since been
    /// cancelled. Does nothing if no original bank transaction was ever recorded (the source wasn't bank
    /// settled) or if a reversal already exists for it.
    /// </summary>
    public async Task<ShopBankTransaction?> RecordReversalIfExistsAsync(
        Guid tenantId,
        ShopBankTransactionType transactionType,
        ShopBankReferenceType referenceType,
        Guid referenceId,
        string referenceNumber,
        string? description,
        DateTime transactionDate)
    {
        var query = await _transactionRepository.GetQueryableAsync();
        var original = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x =>
            x.TenantId == tenantId && x.ReferenceType == referenceType && x.ReferenceId == referenceId &&
            x.TransactionType == transactionType && !x.IsReversal));
        if (original == null) return null;

        var alreadyReversed = await AsyncExecuter.AnyAsync(query.Where(x =>
            x.TenantId == tenantId && x.ReversalOfTransactionId == original.Id && x.IsReversal));
        if (alreadyReversed) return null;

        var account = await GetAccountAsync(original.BankAccountId, tenantId);
        var reversalDirection = original.Direction == ShopBankDirection.In ? ShopBankDirection.Out : ShopBankDirection.In;

        var newBalance = ApplyDirection(account.CurrentBalance, reversalDirection, original.Amount);
        account.ApplyBalanceChange(newBalance);
        await _accountRepository.UpdateAsync(account, autoSave: true);

        var reversal = new ShopBankTransaction(
            GuidGenerator.Create(), tenantId, account.Id, transactionDate, ShopBankTransactionType.Reversal,
            reversalDirection, original.Amount, referenceType, referenceId, referenceNumber, description, newBalance,
            referenceId, original.Id, true, _currentUser.GetId(), Clock.Now);
        await _transactionRepository.InsertAsync(reversal, autoSave: true);

        return reversal;
    }

    // ---------------------------------------------------------------------
    // Manual movement
    // ---------------------------------------------------------------------

    public async Task<ShopBankTransaction> CreateManualMovementAsync(
        Guid bankAccountId, DateTime transactionDate, ShopBankDirection direction, decimal amount,
        string? referenceNumber, string? description)
    {
        var tenantId = RequireTenant();
        if (amount <= 0) throw new BusinessException("ShopManagement:BankMovementAmountMustBeGreaterThanZero");

        var account = await GetAccountAsync(bankAccountId, tenantId);
        if (!account.IsActive) throw new BusinessException("ShopManagement:BankAccountInactive");

        var newBalance = ApplyDirection(account.CurrentBalance, direction, amount);
        account.ApplyBalanceChange(newBalance);
        await _accountRepository.UpdateAsync(account, autoSave: true);

        var transactionType = direction == ShopBankDirection.In ? ShopBankTransactionType.ManualDeposit : ShopBankTransactionType.ManualWithdrawal;
        var transactionId = GuidGenerator.Create();

        var transaction = new ShopBankTransaction(
            transactionId, tenantId, account.Id, transactionDate, transactionType, direction, amount,
            ShopBankReferenceType.ManualBankMovement, transactionId, referenceNumber, description, newBalance,
            null, null, false, _currentUser.GetId(), Clock.Now);
        await _transactionRepository.InsertAsync(transaction, autoSave: true);

        return transaction;
    }

    // ---------------------------------------------------------------------
    // Summary
    // ---------------------------------------------------------------------

    public async Task<(decimal TotalMoneyIn, decimal TotalMoneyOut, decimal CustomerPayments, decimal SupplierPayments,
        decimal Expenses, decimal CustomerRefunds, decimal CashDeposits, decimal CashWithdrawals, decimal BankTransfersIn,
        decimal BankTransfersOut, decimal ManualDeposits, decimal ManualWithdrawals)> ComputeSummaryAsync(
        Guid bankAccountId, Guid tenantId, DateTime? dateFrom, DateTime? dateTo)
    {
        var query = await _transactionRepository.GetQueryableAsync();
        var filtered = query.Where(x => x.TenantId == tenantId && x.BankAccountId == bankAccountId)
            .WhereIf(dateFrom.HasValue, x => x.TransactionDate >= dateFrom!.Value)
            .WhereIf(dateTo.HasValue, x => x.TransactionDate <= dateTo!.Value);

        var all = await AsyncExecuter.ToListAsync(filtered);

        decimal SumType(ShopBankTransactionType type) => all.Where(x => x.TransactionType == type)
            .Sum(x => x.Direction == ShopBankDirection.In ? x.Amount : -x.Amount);

        var customerPayments = Math.Max(0, SumType(ShopBankTransactionType.CustomerPayment));
        var supplierPayments = Math.Max(0, -SumType(ShopBankTransactionType.SupplierPayment));
        var expenses = Math.Max(0, -SumType(ShopBankTransactionType.Expense));
        var customerRefunds = Math.Max(0, -SumType(ShopBankTransactionType.CustomerRefund));
        var cashDeposits = Math.Max(0, SumType(ShopBankTransactionType.CashDeposit));
        var cashWithdrawals = Math.Max(0, -SumType(ShopBankTransactionType.CashWithdrawal));
        var bankTransfersIn = Math.Max(0, SumType(ShopBankTransactionType.BankTransferIn));
        var bankTransfersOut = Math.Max(0, -SumType(ShopBankTransactionType.BankTransferOut));
        var manualDeposits = Math.Max(0, SumType(ShopBankTransactionType.ManualDeposit));
        var manualWithdrawals = Math.Max(0, -SumType(ShopBankTransactionType.ManualWithdrawal));

        var totalMoneyIn = Round(all.Where(x => x.Direction == ShopBankDirection.In).Sum(x => x.Amount));
        var totalMoneyOut = Round(all.Where(x => x.Direction == ShopBankDirection.Out).Sum(x => x.Amount));

        return (totalMoneyIn, totalMoneyOut, Round(customerPayments), Round(supplierPayments), Round(expenses),
            Round(customerRefunds), Round(cashDeposits), Round(cashWithdrawals), Round(bankTransfersIn),
            Round(bankTransfersOut), Round(manualDeposits), Round(manualWithdrawals));
    }

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------

    private async Task<bool> ExistsAsync(Guid tenantId, ShopBankReferenceType referenceType, Guid referenceId, ShopBankTransactionType transactionType, bool isReversal)
    {
        var query = await _transactionRepository.GetQueryableAsync();
        return await AsyncExecuter.AnyAsync(query.Where(x =>
            x.TenantId == tenantId && x.ReferenceType == referenceType && x.ReferenceId == referenceId &&
            x.TransactionType == transactionType && x.IsReversal == isReversal));
    }

    private static decimal ApplyDirection(decimal currentBalance, ShopBankDirection direction, decimal amount)
    {
        var newBalance = direction == ShopBankDirection.In ? currentBalance + amount : currentBalance - amount;
        if (newBalance < 0) throw new BusinessException("ShopManagement:InsufficientBankBalance");
        return Round(newBalance);
    }

    private Guid RequireTenantOwnership(ShopBankAccount account)
    {
        var tenantId = RequireTenant();
        if (account.TenantId != tenantId) throw new BusinessException("ShopManagement:BankAccountNotFound");
        return tenantId;
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private static string NormalizeCode(string value) =>
        Check.NotNullOrWhiteSpace(value, nameof(value), ShopBankAccountConsts.CodeMaxLength).Trim();

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
