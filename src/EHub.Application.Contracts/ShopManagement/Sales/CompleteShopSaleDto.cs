using System;
using System.Collections.Generic;

namespace EHub.ShopManagement.Sales;

/// <summary>
/// Optional manual batch allocations to use when completing a Sale. Any batch-tracked sale item not
/// listed here is allocated automatically using FEFO/FIFO.
/// </summary>
public class CompleteShopSaleDto
{
    public List<CompleteShopSaleItemBatchAllocationDto> ItemBatchAllocations { get; set; } = new();
}

public class CompleteShopSaleItemBatchAllocationDto
{
    public Guid SaleItemId { get; set; }
    public List<ShopBatchAllocationLineDto> Allocations { get; set; } = new();
}

public class ShopBatchAllocationLineDto
{
    public Guid ProductBatchId { get; set; }
    public decimal Quantity { get; set; }
}
