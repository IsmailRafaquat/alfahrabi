using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.CashRegisters;

public class GetShopCashTransactionsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? CashRegisterId { get; set; }
    public Guid? CashClosingId { get; set; }
    public ShopCashTransactionType? TransactionType { get; set; }
    public ShopCashDirection? Direction { get; set; }
    public DateTime? TransactionDateFrom { get; set; }
    public DateTime? TransactionDateTo { get; set; }
}
