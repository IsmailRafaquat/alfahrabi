using System;
using System.Collections.Generic;

namespace EHub.ShopManagement.ProductBatches;

public class ShopBatchAvailabilityDto
{
    public Guid ProductId { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal TotalAvailableQuantity { get; set; }
    public bool IsSufficient { get; set; }
    public List<ShopProductBatchLookupDto> Batches { get; set; } = new();
}
