using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace EHub.ShopManagement.CashRegisters;

public class ShopCashRegisterManager : DomainService
{
    private readonly IRepository<ShopCashRegister, Guid> _registerRepository;
    private readonly IRepository<ShopCashRegisterTransaction, Guid> _transactionRepository;
    private readonly IRepository<ShopCashClosing, Guid> _closingRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public ShopCashRegisterManager(
        IRepository<ShopCashRegister, Guid> registerRepository,
        IRepository<ShopCashRegisterTransaction, Guid> transactionRepository,
        IRepository<ShopCashClosing, Guid> closingRepository,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _registerRepository = registerRepository;
        _transactionRepository = transactionRepository;
        _closingRepository = closingRepository;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    // ---------------------------------------------------------------------
    // Register CRUD
    // ---------------------------------------------------------------------

    public async Task<ShopCashRegister> CreateAsync(string code, string name, string? description, bool isDefault, bool isActive)
    {
        var tenantId = RequireTenant();
        var normalizedCode = NormalizeCode(code);
        await ValidateUniqueCodeAsync(normalizedCode, tenantId, null);

        if (isDefault) await ClearExistingDefaultAsync(tenantId, null);

        return new ShopCashRegister(GuidGenerator.Create(), tenantId, normalizedCode, name, description, isDefault, isActive);
    }

    public async Task UpdateAsync(ShopCashRegister register, string code, string name, string? description, bool isDefault, bool isActive)
    {
        var tenantId = RequireTenantOwnership(register);
        var normalizedCode = NormalizeCode(code);
        await ValidateUniqueCodeAsync(normalizedCode, tenantId, register.Id);

        if (isDefault) await ClearExistingDefaultAsync(tenantId, register.Id);

        register.Update(normalizedCode, name, description, isDefault, isActive);
    }

    public async Task ValidateDeleteAsync(ShopCashRegister register)
    {
        RequireTenantOwnership(register);

        var closingQuery = await _closingRepository.GetQueryableAsync();
        var hasClosings = await AsyncExecuter.AnyAsync(closingQuery.Where(x => x.CashRegisterId == register.Id));
        if (hasClosings) throw new BusinessException("ShopManagement:CashRegisterHasClosingsCannotBeDeleted");
    }

    private async Task ClearExistingDefaultAsync(Guid tenantId, Guid? excludeId)
    {
        var query = await _registerRepository.GetQueryableAsync();
        var others = await AsyncExecuter.ToListAsync(query.Where(x =>
            x.TenantId == tenantId && x.IsDefault && (!excludeId.HasValue || x.Id != excludeId.Value)));

        foreach (var other in others)
        {
            other.SetAsNotDefault();
            await _registerRepository.UpdateAsync(other, autoSave: true);
        }
    }

    private async Task ValidateUniqueCodeAsync(string code, Guid tenantId, Guid? excludedId)
    {
        var query = await _registerRepository.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(query.Where(x =>
            x.TenantId == tenantId && x.Code == code && (!excludedId.HasValue || x.Id != excludedId)));
        if (exists) throw new BusinessException("ShopManagement:CashRegisterCodeAlreadyExists").WithData("Code", code);
    }

    // ---------------------------------------------------------------------
    // Open / Close / Cancel closing
    // ---------------------------------------------------------------------

    public async Task<ShopCashClosing> OpenAsync(Guid cashRegisterId, DateTime businessDate, decimal openingCash, string? notes)
    {
        var tenantId = RequireTenant();
        var register = await GetRegisterAsync(cashRegisterId, tenantId);
        if (!register.IsActive) throw new BusinessException("ShopManagement:CashRegisterInactive");

        var existingOpen = await FindOpenClosingAsync(register.Id, tenantId);
        if (existingOpen != null) throw new BusinessException("ShopManagement:CashRegisterAlreadyOpen");

        var openedByUserId = _currentUser.GetId();
        var openedDate = Clock.Now;

        var closing = new ShopCashClosing(GuidGenerator.Create(), tenantId, register.Id, businessDate, openingCash, notes, openedByUserId, openedDate);
        await _closingRepository.InsertAsync(closing, autoSave: true);

        var openingTransaction = new ShopCashRegisterTransaction(
            GuidGenerator.Create(), tenantId, register.Id, closing.Id, openedDate,
            ShopCashTransactionType.OpeningCash, ShopCashDirection.In, Math.Max(openingCash, 0.01m),
            ShopCashReferenceType.CashClosing, closing.Id, "OPEN-" + closing.Id.ToString("N")[..8].ToUpperInvariant(),
            "Opening cash", null, openedByUserId, openedDate);

        // An opening cash amount of exactly zero is valid business-wise but the transaction ledger
        // entity requires a positive amount; skip recording a ledger row in that edge case only.
        if (openingCash > 0) await _transactionRepository.InsertAsync(openingTransaction, autoSave: true);

        return closing;
    }

    public async Task<ShopCashClosing> CloseAsync(ShopCashClosing closing, decimal actualClosingCash, string? notes)
    {
        var tenantId = RequireTenantOwnership(closing);

        var summary = await ComputeSummaryAsync(closing, tenantId);

        closing.ApplyClosingTotals(
            summary.CashSales, summary.CustomerCashPayments, summary.SupplierCashPayments, summary.CashExpenses,
            summary.CustomerRefunds, summary.ManualCashIn, summary.ManualCashOut, summary.ExpectedClosingCash,
            actualClosingCash, notes, _currentUser.GetId(), Clock.Now);

        return closing;
    }

    public Task CancelClosingAsync(ShopCashClosing closing, string cancellationReason)
    {
        RequireTenantOwnership(closing);
        closing.MarkAsCancelled(cancellationReason);
        return Task.CompletedTask;
    }

    public async Task<ShopCashClosing?> FindOpenClosingAsync(Guid cashRegisterId, Guid tenantId)
    {
        var query = await _closingRepository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x =>
            x.TenantId == tenantId && x.CashRegisterId == cashRegisterId && x.Status == ShopCashClosingStatus.Open));
    }

    public async Task<(decimal CashSales, decimal CustomerCashPayments, decimal SupplierCashPayments, decimal CashExpenses,
        decimal CustomerRefunds, decimal ManualCashIn, decimal ManualCashOut, decimal ExpectedClosingCash)> ComputeSummaryAsync(
        ShopCashClosing closing, Guid tenantId)
    {
        // Sweep any transactions recorded against this register since it opened that were not yet
        // linked to a specific closing (e.g. recorded while no closing existed, now caught up here).
        var txQuery = await _transactionRepository.GetQueryableAsync();
        var unassigned = await AsyncExecuter.ToListAsync(txQuery.Where(x =>
            x.TenantId == tenantId && x.CashRegisterId == closing.CashRegisterId &&
            x.CashClosingId == null && x.TransactionDate >= closing.OpenedDate));

        foreach (var transaction in unassigned)
        {
            transaction.AssignToClosing(closing.Id);
            await _transactionRepository.UpdateAsync(transaction, autoSave: true);
        }

        var linkedQuery = await _transactionRepository.GetQueryableAsync();
        var linked = await AsyncExecuter.ToListAsync(linkedQuery.Where(x => x.CashClosingId == closing.Id));

        decimal NetIn(ShopCashTransactionType type) => Math.Max(0, Round(
            linked.Where(x => x.TransactionType == type && x.Direction == ShopCashDirection.In).Sum(x => x.Amount) -
            linked.Where(x => x.TransactionType == type && x.Direction == ShopCashDirection.Out).Sum(x => x.Amount)));

        decimal NetOut(ShopCashTransactionType type) => Math.Max(0, Round(
            linked.Where(x => x.TransactionType == type && x.Direction == ShopCashDirection.Out).Sum(x => x.Amount) -
            linked.Where(x => x.TransactionType == type && x.Direction == ShopCashDirection.In).Sum(x => x.Amount)));

        var cashSales = NetIn(ShopCashTransactionType.CashSale);
        var customerCashPayments = NetIn(ShopCashTransactionType.CustomerPayment);
        var supplierCashPayments = NetOut(ShopCashTransactionType.SupplierPayment);
        var cashExpenses = NetOut(ShopCashTransactionType.Expense);
        var customerRefunds = NetOut(ShopCashTransactionType.CustomerRefund);
        var manualCashIn = NetIn(ShopCashTransactionType.CashIn);
        var manualCashOut = NetOut(ShopCashTransactionType.CashOut);

        var expectedClosingCash = Round(closing.OpeningCash + cashSales + customerCashPayments + manualCashIn
            - supplierCashPayments - cashExpenses - customerRefunds - manualCashOut);

        return (cashSales, customerCashPayments, supplierCashPayments, cashExpenses, customerRefunds, manualCashIn, manualCashOut, expectedClosingCash);
    }

    // ---------------------------------------------------------------------
    // Manual cash movement
    // ---------------------------------------------------------------------

    public async Task<ShopCashRegisterTransaction> CreateManualMovementAsync(
        Guid cashRegisterId, DateTime transactionDate, ShopCashDirection direction, decimal amount, string? referenceNumber, string? description)
    {
        var tenantId = RequireTenant();
        if (amount <= 0) throw new BusinessException("ShopManagement:CashMovementAmountMustBeGreaterThanZero");

        var register = await GetRegisterAsync(cashRegisterId, tenantId);
        var openClosing = await FindOpenClosingAsync(register.Id, tenantId)
            ?? throw new BusinessException("ShopManagement:CashRegisterNotOpen");

        var transactionType = direction == ShopCashDirection.In ? ShopCashTransactionType.CashIn : ShopCashTransactionType.CashOut;
        var transactionId = GuidGenerator.Create();
        var createdByUserId = _currentUser.GetId();
        var createdDate = Clock.Now;

        var transaction = new ShopCashRegisterTransaction(
            transactionId, tenantId, register.Id, openClosing.Id, transactionDate, transactionType, direction, amount,
            ShopCashReferenceType.ManualCashMovement, transactionId, referenceNumber, description, null, createdByUserId, createdDate);

        await _transactionRepository.InsertAsync(transaction, autoSave: true);
        return transaction;
    }

    /// <summary>
    /// Records the cash-drawer side of a Bank Transfer (Cash-to-Bank / Bank-to-Cash) against an explicit
    /// register chosen on the transfer, rather than the tenant's implicit default register. Requires an
    /// open closing and, for an outgoing amount, validates sufficient available cash.
    /// </summary>
    public async Task<ShopCashRegisterTransaction> RecordBankTransferCashTransactionAsync(
        Guid cashRegisterId, DateTime transactionDate, ShopCashDirection direction, decimal amount, Guid transferId, string referenceNumber, string? description)
    {
        var tenantId = RequireTenant();
        if (amount <= 0) throw new BusinessException("ShopManagement:CashMovementAmountMustBeGreaterThanZero");

        var register = await GetRegisterAsync(cashRegisterId, tenantId);
        if (!register.IsActive) throw new BusinessException("ShopManagement:CashRegisterInactive");

        var openClosing = await FindOpenClosingAsync(register.Id, tenantId)
            ?? throw new BusinessException("ShopManagement:CashRegisterNotOpen");

        if (direction == ShopCashDirection.Out)
        {
            var summary = await ComputeSummaryAsync(openClosing, tenantId);
            if (amount > summary.ExpectedClosingCash) throw new BusinessException("ShopManagement:InsufficientCashBalance");
        }

        var transactionType = direction == ShopCashDirection.In ? ShopCashTransactionType.CashIn : ShopCashTransactionType.CashOut;

        var transaction = new ShopCashRegisterTransaction(
            GuidGenerator.Create(), tenantId, register.Id, openClosing.Id, transactionDate, transactionType, direction, amount,
            ShopCashReferenceType.BankTransfer, transferId, referenceNumber, description, transferId, _currentUser.GetId(), Clock.Now);

        await _transactionRepository.InsertAsync(transaction, autoSave: true);
        return transaction;
    }

    // ---------------------------------------------------------------------
    // Automatic recording (called by Sale / CustomerPayment / SupplierPayment / Expense / SaleReturn managers)
    // ---------------------------------------------------------------------

    /// <summary>
    /// Records a cash transaction for an effective (completed/posted) source document, attaching it to
    /// the tenant's default active cash register. No-ops silently (by design) if the tenant has not
    /// configured a default register yet, so existing Sale/Payment/Expense workflows keep working for
    /// tenants who haven't set up cash-register tracking.
    /// </summary>
    public async Task RecordAutomaticTransactionAsync(
        Guid tenantId,
        ShopCashTransactionType transactionType,
        ShopCashDirection direction,
        decimal amount,
        ShopCashReferenceType referenceType,
        Guid referenceId,
        string referenceNumber,
        string? description,
        DateTime transactionDate)
    {
        if (amount <= 0) return;

        var register = await GetDefaultRegisterAsync(tenantId);
        if (register == null) return;

        var alreadyExists = await ExistsAsync(tenantId, referenceType, referenceId, transactionType, direction);
        if (alreadyExists) return;

        var openClosing = await FindOpenClosingAsync(register.Id, tenantId);

        var transaction = new ShopCashRegisterTransaction(
            GuidGenerator.Create(), tenantId, register.Id, openClosing?.Id, transactionDate, transactionType, direction,
            amount, referenceType, referenceId, referenceNumber, description, referenceId, null, Clock.Now);

        await _transactionRepository.InsertAsync(transaction, autoSave: true);
    }

    /// <summary>
    /// Reverses a previously-recorded automatic cash transaction for a source document that has since
    /// been cancelled (e.g. a posted cash Customer/Supplier Payment or Expense). Does nothing if no
    /// original cash transaction was ever recorded (the source wasn't cash, or no register existed at
    /// the time) or if a reversal already exists.
    /// </summary>
    public async Task RecordReversalIfExistsAsync(
        Guid tenantId,
        ShopCashTransactionType transactionType,
        ShopCashReferenceType referenceType,
        Guid referenceId,
        string referenceNumber,
        string? description,
        DateTime transactionDate)
    {
        var query = await _transactionRepository.GetQueryableAsync();
        var original = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x =>
            x.TenantId == tenantId && x.ReferenceType == referenceType && x.ReferenceId == referenceId && x.TransactionType == transactionType));
        if (original == null) return;

        var reversalDirection = original.Direction == ShopCashDirection.In ? ShopCashDirection.Out : ShopCashDirection.In;
        var alreadyReversed = await ExistsAsync(tenantId, referenceType, referenceId, transactionType, reversalDirection);
        if (alreadyReversed) return;

        var openClosing = await FindOpenClosingAsync(original.CashRegisterId, tenantId);

        var reversal = new ShopCashRegisterTransaction(
            GuidGenerator.Create(), tenantId, original.CashRegisterId, openClosing?.Id, transactionDate, transactionType,
            reversalDirection, original.Amount, referenceType, referenceId, referenceNumber, description, referenceId, null, Clock.Now);

        await _transactionRepository.InsertAsync(reversal, autoSave: true);
    }

    private async Task<bool> ExistsAsync(Guid tenantId, ShopCashReferenceType referenceType, Guid referenceId, ShopCashTransactionType transactionType, ShopCashDirection direction)
    {
        var query = await _transactionRepository.GetQueryableAsync();
        return await AsyncExecuter.AnyAsync(query.Where(x =>
            x.TenantId == tenantId && x.ReferenceType == referenceType && x.ReferenceId == referenceId &&
            x.TransactionType == transactionType && x.Direction == direction));
    }

    private async Task<ShopCashRegister?> GetDefaultRegisterAsync(Guid tenantId)
    {
        var query = await _registerRepository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.TenantId == tenantId && x.IsDefault && x.IsActive));
    }

    public async Task<ShopCashRegister> GetRegisterAsync(Guid cashRegisterId, Guid tenantId)
    {
        var query = await _registerRepository.GetQueryableAsync();
        var register = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == cashRegisterId && x.TenantId == tenantId));
        return register ?? throw new BusinessException("ShopManagement:CashRegisterNotFound");
    }

    private Guid RequireTenantOwnership(ShopCashRegister register)
    {
        var tenantId = RequireTenant();
        if (register.TenantId != tenantId) throw new BusinessException("ShopManagement:CashRegisterNotFound");
        return tenantId;
    }

    private Guid RequireTenantOwnership(ShopCashClosing closing)
    {
        var tenantId = RequireTenant();
        if (closing.TenantId != tenantId) throw new BusinessException("ShopManagement:CashClosingNotFound");
        return tenantId;
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private static string NormalizeCode(string value) =>
        Check.NotNullOrWhiteSpace(value, nameof(value), ShopCashRegisterConsts.CodeMaxLength).Trim();

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
