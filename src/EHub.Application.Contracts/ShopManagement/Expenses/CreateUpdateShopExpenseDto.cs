using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.Expenses;

public class CreateUpdateShopExpenseDto
{
    [Required]
    public Guid ExpenseCategoryId { get; set; }

    [Required]
    public DateTime ExpenseDate { get; set; }

    // Must be > 0; enforced by ShopExpense as a localized BusinessException.
    public decimal Amount { get; set; }

    [Required]
    public ShopExpensePaymentMethod PaymentMethod { get; set; }

    [StringLength(ShopExpenseConsts.PaidToMaxLength)]
    public string? PaidTo { get; set; }

    [StringLength(ShopExpenseConsts.ReferenceNumberMaxLength)]
    public string? ReferenceNumber { get; set; }

    [StringLength(ShopExpenseConsts.ChequeNumberMaxLength)]
    public string? ChequeNumber { get; set; }

    [StringLength(ShopExpenseConsts.BankNameMaxLength)]
    public string? BankName { get; set; }

    [StringLength(ShopExpenseConsts.DescriptionMaxLength)]
    public string? Description { get; set; }

    [StringLength(ShopExpenseConsts.NotesMaxLength)]
    public string? Notes { get; set; }
}
