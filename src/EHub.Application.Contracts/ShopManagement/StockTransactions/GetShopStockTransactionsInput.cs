using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.StockTransactions;

public class GetShopStockTransactionsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? ProductId { get; set; }
    public ShopStockTransactionType? TransactionType { get; set; }
    public ShopStockReferenceType? ReferenceType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? ReferenceNumber { get; set; }
}
