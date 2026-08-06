using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Products;

public class GetShopProductsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? UnitId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsTaxable { get; set; }
    public bool? TrackBatch { get; set; }
    public bool? TrackExpiry { get; set; }
    public bool LowStockOnly { get; set; }
}
