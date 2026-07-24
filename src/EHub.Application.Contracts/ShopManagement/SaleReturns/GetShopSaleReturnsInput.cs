using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.SaleReturns;

public class GetShopSaleReturnsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? SaleId { get; set; }
    public Guid? CustomerId { get; set; }
    public ShopSaleReturnStatus? Status { get; set; }
    public ShopSaleReturnReason? Reason { get; set; }
    public ShopSaleReturnSettlementType? SettlementType { get; set; }
    public DateTime? ReturnDateFrom { get; set; }
    public DateTime? ReturnDateTo { get; set; }
}
