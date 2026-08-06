using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.ProductBatches;

public class GetShopProductBatchesInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public Guid? SupplierId { get; set; }
    public ShopProductBatchStatus? Status { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryFrom { get; set; }
    public DateTime? ExpiryTo { get; set; }
    public bool? NearExpiryOnly { get; set; }
    public bool? ExpiredOnly { get; set; }
    public bool? HasAvailableStock { get; set; }
}
