using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.ExpenseCategories;
using EHub.ShopManagement.PurchaseOrders;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace EHub.ShopManagement.Expenses;

public class ShopExpenseManager : DomainService
{
    private const string DocumentType = "Expense";
    private const string NumberPrefix = "EXP-";

    private readonly IRepository<ShopExpense, Guid> _repository;
    private readonly IRepository<ShopExpenseCategory, Guid> _categoryRepository;
    private readonly ShopDocumentNumberGenerator _numberGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public ShopExpenseManager(
        IRepository<ShopExpense, Guid> repository,
        IRepository<ShopExpenseCategory, Guid> categoryRepository,
        ShopDocumentNumberGenerator numberGenerator,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _numberGenerator = numberGenerator;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<ShopExpense> CreateAsync(
        Guid expenseCategoryId,
        DateTime expenseDate,
        decimal amount,
        ShopExpensePaymentMethod paymentMethod,
        string? paidTo,
        string? referenceNumber,
        string? chequeNumber,
        string? bankName,
        string? description,
        string? notes)
    {
        var tenantId = RequireTenant();
        await ValidateCategoryAsync(expenseCategoryId, tenantId, requireActive: true);

        var expenseNumber = await _numberGenerator.GetNextNumberAsync(tenantId, DocumentType, NumberPrefix);

        return new ShopExpense(GuidGenerator.Create(), tenantId, expenseNumber, expenseCategoryId, expenseDate, amount,
            paymentMethod, paidTo, referenceNumber, chequeNumber, bankName, description, notes);
    }

    public async Task UpdateAsync(
        ShopExpense expense,
        Guid expenseCategoryId,
        DateTime expenseDate,
        decimal amount,
        ShopExpensePaymentMethod paymentMethod,
        string? paidTo,
        string? referenceNumber,
        string? chequeNumber,
        string? bankName,
        string? description,
        string? notes)
    {
        var tenantId = RequireTenantOwnership(expense);
        expense.EnsureEditable();
        await ValidateCategoryAsync(expenseCategoryId, tenantId, requireActive: false);

        expense.Update(expenseCategoryId, expenseDate, amount, paymentMethod, paidTo, referenceNumber, chequeNumber, bankName, description, notes);
    }

    public async Task PostAsync(ShopExpense expense)
    {
        var tenantId = RequireTenantOwnership(expense);
        await ValidateCategoryAsync(expense.ExpenseCategoryId, tenantId, requireActive: false);
        if (expense.Amount <= 0) throw new BusinessException("ShopManagement:ExpenseAmountMustBeGreaterThanZero");

        expense.MarkAsPosted(_currentUser.GetId(), Clock.Now);
    }

    public Task CancelAsync(ShopExpense expense, string cancellationReason)
    {
        RequireTenantOwnership(expense);
        expense.MarkAsCancelled(_currentUser.GetId(), Clock.Now, cancellationReason);
        return Task.CompletedTask;
    }

    public Task ValidateDeleteAsync(ShopExpense expense)
    {
        RequireTenantOwnership(expense);
        expense.EnsureDeletable();
        return Task.CompletedTask;
    }

    private async Task ValidateCategoryAsync(Guid categoryId, Guid tenantId, bool requireActive)
    {
        var query = await _categoryRepository.GetQueryableAsync();
        var category = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == categoryId && x.TenantId == tenantId));
        if (category == null) throw new BusinessException("ShopManagement:ExpenseCategoryNotFound");
        if (requireActive && !category.IsActive) throw new BusinessException("ShopManagement:ExpenseCategoryInactive").WithData("Category", category.Name);
    }

    private Guid RequireTenantOwnership(ShopExpense expense)
    {
        var tenantId = RequireTenant();
        if (expense.TenantId != tenantId) throw new BusinessException("ShopManagement:ExpenseNotFound");
        return tenantId;
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}
