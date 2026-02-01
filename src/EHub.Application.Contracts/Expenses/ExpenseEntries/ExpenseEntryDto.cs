using System;
using Volo.Abp.Application.Dtos;

namespace EHub.Expenses.ExpenseEntries;

public class ExpenseEntryDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public DateTime ExpenseDate { get; set; }

    public Guid ExpenseCategoryId { get; set; }
    public string ExpenseCategoryName { get; set; } = default!; // for grid display

    public string Title { get; set; } = default!;
    public decimal Amount { get; set; }

    public string? PaidTo { get; set; }
    public string? Remarks { get; set; }
}
