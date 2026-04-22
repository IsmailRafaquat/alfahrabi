using System.ComponentModel.DataAnnotations;

namespace EHub.Expenses.ExpenseCategories;

public class CreateUpdateExpenseCategoryDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = default!;

    public bool IsActive { get; set; } = true;

    public ExpenseEntryLimitType? EntryLimitType { get; set; }
}
