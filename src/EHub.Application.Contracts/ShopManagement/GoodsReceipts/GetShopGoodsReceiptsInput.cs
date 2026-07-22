using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.GoodsReceipts;

public class GetShopGoodsReceiptsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid? SupplierId { get; set; }
    public ShopGoodsReceiptStatus? Status { get; set; }
    public DateTime? ReceiptDateFrom { get; set; }
    public DateTime? ReceiptDateTo { get; set; }
    public decimal? MinimumGrandTotal { get; set; }
    public decimal? MaximumGrandTotal { get; set; }
}
