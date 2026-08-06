using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.PurchaseOrders;

public class GetShopPurchaseOrdersInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? SupplierId { get; set; }
    public ShopPurchaseOrderStatus? Status { get; set; }
    public DateTime? OrderDateFrom { get; set; }
    public DateTime? OrderDateTo { get; set; }
    public DateTime? ExpectedDeliveryDateFrom { get; set; }
    public DateTime? ExpectedDeliveryDateTo { get; set; }
    public decimal? MinimumGrandTotal { get; set; }
    public decimal? MaximumGrandTotal { get; set; }
}
