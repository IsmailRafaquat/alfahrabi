using System;

namespace EHub.Expenses.ExpenseCategories;

public class ExpenseCategoryLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
}
