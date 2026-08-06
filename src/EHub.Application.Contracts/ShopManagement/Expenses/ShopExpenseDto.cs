using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Expenses;

public class ShopExpenseDto : EntityDto<Guid>
{
    public string ExpenseNumber { get; set; } = string.Empty;

    public Guid ExpenseCategoryId { get; set; }
    public string ExpenseCategoryCode { get; set; } = string.Empty;
    public string ExpenseCategoryName { get; set; } = string.Empty;

    public DateTime ExpenseDate { get; set; }
    public decimal? Amount { get; set; }
    public ShopExpensePaymentMethod PaymentMethod { get; set; }
    public string? PaidTo { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ChequeNumber { get; set; }
    public string? BankName { get; set; }
    public Guid? BankAccountId { get; set; }
    public string? BankAccountCode { get; set; }
    public string? BankAccountName { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public ShopExpenseStatus Status { get; set; }

    public DateTime? PostedDate { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreationTime { get; set; }
}
