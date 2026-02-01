using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.Expenses.ExpenseEntries;

public class CreateUpdateExpenseEntryDto
{
    [Required]
    public DateTime ExpenseDate { get; set; }

    [Required]
    public Guid ExpenseCategoryId { get; set; }

    [Required]
    [StringLength(256)]
    public string Title { get; set; } = default!;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [StringLength(256)]
    public string? PaidTo { get; set; }

    [StringLength(1024)]
    public string? Remarks { get; set; }
}
