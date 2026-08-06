using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.ExpenseCategories;

namespace EHub.ShopManagement.Expenses;

public class ShopExpense : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string ExpenseNumber { get; protected set; } = string.Empty;

    public Guid ExpenseCategoryId { get; protected set; }
    public ShopExpenseCategory? ExpenseCategory { get; protected set; }

    public DateTime ExpenseDate { get; protected set; }
    public decimal Amount { get; protected set; }
    public ShopExpensePaymentMethod PaymentMethod { get; protected set; }
    public string? PaidTo { get; protected set; }
    public string? ReferenceNumber { get; protected set; }
    public string? ChequeNumber { get; protected set; }
    public string? BankName { get; protected set; }
    public Guid? BankAccountId { get; protected set; }
    public string? Description { get; protected set; }
    public string? Notes { get; protected set; }
    public ShopExpenseStatus Status { get; protected set; } = ShopExpenseStatus.Draft;

    public Guid? PostedByUserId { get; protected set; }
    public DateTime? PostedDate { get; protected set; }
    public Guid? CancelledByUserId { get; protected set; }
    public DateTime? CancelledDate { get; protected set; }
    public string? CancellationReason { get; protected set; }

    protected ShopExpense() { }

    internal ShopExpense(
        Guid id,
        Guid tenantId,
        string expenseNumber,
        Guid expenseCategoryId,
        DateTime expenseDate,
        decimal amount,
        ShopExpensePaymentMethod paymentMethod,
        string? paidTo,
        string? referenceNumber,
        string? chequeNumber,
        string? bankName,
        Guid? bankAccountId,
        string? description,
        string? notes) : base(id)
    {
        TenantId = tenantId;
        ExpenseNumber = Check.NotNullOrWhiteSpace(expenseNumber, nameof(expenseNumber), ShopExpenseConsts.ExpenseNumberMaxLength);
        ExpenseCategoryId = expenseCategoryId;
        Status = ShopExpenseStatus.Draft;
        SetValues(expenseDate, amount, paymentMethod, paidTo, referenceNumber, chequeNumber, bankName, bankAccountId, description, notes);
    }

    internal void Update(
        Guid expenseCategoryId,
        DateTime expenseDate,
        decimal amount,
        ShopExpensePaymentMethod paymentMethod,
        string? paidTo,
        string? referenceNumber,
        string? chequeNumber,
        string? bankName,
        Guid? bankAccountId,
        string? description,
        string? notes)
    {
        EnsureEditable();
        ExpenseCategoryId = expenseCategoryId;
        SetValues(expenseDate, amount, paymentMethod, paidTo, referenceNumber, chequeNumber, bankName, bankAccountId, description, notes);
    }

    internal void MarkAsPosted(Guid postedByUserId, DateTime postedDate)
    {
        EnsurePostable();
        Status = ShopExpenseStatus.Posted;
        PostedByUserId = postedByUserId;
        PostedDate = postedDate;
    }

    internal void MarkAsCancelled(Guid cancelledByUserId, DateTime cancelledDate, string cancellationReason)
    {
        if (Status != ShopExpenseStatus.Posted) throw new BusinessException("ShopManagement:ExpenseCannotBeCancelled");
        Status = ShopExpenseStatus.Cancelled;
        CancelledByUserId = cancelledByUserId;
        CancelledDate = cancelledDate;
        CancellationReason = Check.NotNullOrWhiteSpace(cancellationReason, nameof(cancellationReason), ShopExpenseConsts.CancellationReasonMaxLength).Trim();
    }

    internal void EnsureEditable()
    {
        if (Status != ShopExpenseStatus.Draft) throw new BusinessException("ShopManagement:ExpenseCannotBeEdited");
    }

    internal void EnsureDeletable()
    {
        if (Status != ShopExpenseStatus.Draft) throw new BusinessException("ShopManagement:ExpenseCannotBeDeleted");
    }

    internal void EnsurePostable()
    {
        if (Status != ShopExpenseStatus.Draft) throw new BusinessException("ShopManagement:ExpenseCannotBePosted");
    }

    private void SetValues(
        DateTime expenseDate,
        decimal amount,
        ShopExpensePaymentMethod paymentMethod,
        string? paidTo,
        string? referenceNumber,
        string? chequeNumber,
        string? bankName,
        Guid? bankAccountId,
        string? description,
        string? notes)
    {
        if (amount <= 0) throw new BusinessException("ShopManagement:ExpenseAmountMustBeGreaterThanZero");

        var trimmedChequeNumber = Check.Length(chequeNumber?.Trim(), nameof(chequeNumber), ShopExpenseConsts.ChequeNumberMaxLength);
        var trimmedBankName = Check.Length(bankName?.Trim(), nameof(bankName), ShopExpenseConsts.BankNameMaxLength);

        if (paymentMethod == ShopExpensePaymentMethod.Cheque && string.IsNullOrWhiteSpace(trimmedChequeNumber))
            throw new BusinessException("ShopManagement:ExpenseChequeNumberRequired");
        if ((paymentMethod == ShopExpensePaymentMethod.Cheque || paymentMethod == ShopExpensePaymentMethod.BankTransfer) && string.IsNullOrWhiteSpace(trimmedBankName))
            throw new BusinessException("ShopManagement:ExpenseBankNameRequired");
        if (RequiresBankAccount(paymentMethod) && !bankAccountId.HasValue)
            throw new BusinessException("ShopManagement:BankAccountRequired");

        ExpenseDate = expenseDate;
        Amount = amount;
        PaymentMethod = paymentMethod;
        PaidTo = Check.Length(paidTo?.Trim(), nameof(paidTo), ShopExpenseConsts.PaidToMaxLength);
        ReferenceNumber = Check.Length(referenceNumber?.Trim(), nameof(referenceNumber), ShopExpenseConsts.ReferenceNumberMaxLength);
        ChequeNumber = trimmedChequeNumber;
        BankName = trimmedBankName;
        BankAccountId = RequiresBankAccount(paymentMethod) ? bankAccountId : null;
        Description = Check.Length(description?.Trim(), nameof(description), ShopExpenseConsts.DescriptionMaxLength);
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopExpenseConsts.NotesMaxLength);
    }

    private static bool RequiresBankAccount(ShopExpensePaymentMethod paymentMethod) =>
        paymentMethod is ShopExpensePaymentMethod.BankTransfer or ShopExpensePaymentMethod.Card or ShopExpensePaymentMethod.Cheque;
}
