using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Sales;

public class GetShopSalesInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? CustomerId { get; set; }
    public ShopSaleStatus? Status { get; set; }
    public ShopSaleType? SaleType { get; set; }
    public DateTime? SaleDateFrom { get; set; }
    public DateTime? SaleDateTo { get; set; }
    public decimal? MinimumGrandTotal { get; set; }
    public decimal? MaximumGrandTotal { get; set; }
    public bool? HasPendingAmount { get; set; }
}
