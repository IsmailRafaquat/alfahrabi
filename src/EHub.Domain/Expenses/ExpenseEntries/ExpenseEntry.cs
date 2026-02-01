using EHub.Expenses.ExpenseCategories;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.Expenses.ExpenseEntries;

public class ExpenseEntry : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public DateTime ExpenseDate { get; private set; }

    public Guid ExpenseCategoryId { get; private set; }
    public ExpenseCategory ExpenseCategory { get; private set; }

    public string Title { get; private set; }

    public decimal Amount { get; private set; }

    public string? PaidTo { get; private set; }

    public string? Remarks { get; private set; }

    private ExpenseEntry()
    {
    }

    public ExpenseEntry(
        Guid id,
        DateTime expenseDate,
        Guid expenseCategoryId,
        string title,
        decimal amount,
        string? paidTo = null,
        string? remarks = null
    ) : base(id)
    {
        SetExpenseDate(expenseDate);
        SetCategory(expenseCategoryId);
        SetTitle(title);
        SetAmount(amount);
        PaidTo = paidTo;
        Remarks = remarks;
    }

    public ExpenseEntry Update(
        DateTime expenseDate,
        Guid expenseCategoryId,
        string title,
        decimal amount,
        string? paidTo,
        string? remarks)
    {
        SetExpenseDate(expenseDate);
        SetCategory(expenseCategoryId);
        SetTitle(title);
        SetAmount(amount);
        PaidTo = paidTo;
        Remarks = remarks;
        return this;
    }

    private void SetExpenseDate(DateTime date)
    {
        ExpenseDate = date;
    }

    private void SetCategory(Guid categoryId)
    {
        ExpenseCategoryId = Check.NotNull(categoryId, nameof(ExpenseCategoryId));
    }

    private void SetTitle(string title)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(Title), maxLength: 256);
    }

    private void SetAmount(decimal amount)
    {
        if (amount <= 0)
            throw new BusinessException("ExpenseAmountMustBeGreaterThanZero");

        Amount = amount;
    }
}
