using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Sales;

public class ShopSaleItemBatchAllocationDto
{
    public Guid ProductBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitCostSnapshot { get; set; }
}

public class ShopSaleItemDto : EntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string UnitShortName { get; set; } = string.Empty;
    public bool UnitAllowDecimal { get; set; }

    public decimal Quantity { get; set; }
    public decimal? UnitSalePrice { get; set; }
    public decimal? UnitCostSnapshot { get; set; }

    public decimal DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? LineSubTotal { get; set; }
    public decimal? LineTotal { get; set; }

    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public List<ShopSaleItemBatchAllocationDto> BatchAllocations { get; set; } = new();
}
