using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.StockCounts;

public class GetShopStockCountsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public ShopStockCountStatus? Status { get; set; }
    public ShopStockCountScope? Scope { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public Guid? ProductId { get; set; }
    public DateTime? CountDateFrom { get; set; }
    public DateTime? CountDateTo { get; set; }
    public bool? HasDifferences { get; set; }
}
