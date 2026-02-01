using System;
using Volo.Abp.Application.Dtos;

namespace EHub.Expenses.ExpenseEntries;

public class GetExpenseEntryListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }           // title / paidTo
    public Guid? ExpenseCategoryId { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
