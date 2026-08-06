using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.StockAdjustments;

public class GetShopStockAdjustmentsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public ShopStockAdjustmentStatus? Status { get; set; }
    public ShopStockAdjustmentReason? Reason { get; set; }
    public DateTime? AdjustmentDateFrom { get; set; }
    public DateTime? AdjustmentDateTo { get; set; }
    public Guid? ProductId { get; set; }
    public ShopStockAdjustmentType? AdjustmentType { get; set; }
}
