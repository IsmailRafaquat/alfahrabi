using Volo.Abp.Application.Dtos;

namespace EHub.Expenses.ExpenseCategories;

public class GetExpenseCategoryListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}
