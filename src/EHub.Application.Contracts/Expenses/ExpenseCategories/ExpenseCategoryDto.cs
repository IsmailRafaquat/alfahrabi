using System;
using Volo.Abp.Application.Dtos;

namespace EHub.Expenses.ExpenseCategories;

public class ExpenseCategoryDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; }
}
