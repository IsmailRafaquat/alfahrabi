using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.BankAccounts;

public class GetShopBankTransactionsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? BankAccountId { get; set; }
    public ShopBankTransactionType? TransactionType { get; set; }
    public ShopBankDirection? Direction { get; set; }
    public ShopBankReferenceType? ReferenceType { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
}
