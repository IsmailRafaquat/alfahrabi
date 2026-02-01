using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.Expenses.ExpenseCategories;

public class ExpenseCategory : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string Name { get; private set; }

    public bool IsActive { get; private set; } = true;

    private ExpenseCategory()
    {
    }

    public ExpenseCategory(Guid id, string name, bool isActive = true) : base(id)
    {
        SetName(name);
        IsActive = isActive;
    }

    public ExpenseCategory ChangeName(string name)
    {
        SetName(name);
        return this;
    }

    public ExpenseCategory Activate()
    {
        IsActive = true;
        return this;
    }

    public ExpenseCategory Deactivate()
    {
        IsActive = false;
        return this;
    }

    private void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(Name), maxLength: 128);
    }
}
