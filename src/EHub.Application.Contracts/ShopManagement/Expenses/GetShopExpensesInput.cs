using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Expenses;

public class GetShopExpensesInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? ExpenseCategoryId { get; set; }
    public ShopExpenseStatus? Status { get; set; }
    public ShopExpensePaymentMethod? PaymentMethod { get; set; }
    public DateTime? ExpenseDateFrom { get; set; }
    public DateTime? ExpenseDateTo { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
}
