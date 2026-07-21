using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Products;

public class ShopProductDto : EntityDto<Guid>
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string UnitShortName { get; set; } = string.Empty;
    public bool UnitAllowDecimal { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }

    public decimal? PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? MinimumSalePrice { get; set; }
    public decimal TaxPercentage { get; set; }

    public decimal CurrentStock { get; set; }
    public decimal MinimumStockLevel { get; set; }
    public decimal? MaximumStockLevel { get; set; }
    public decimal ReorderLevel { get; set; }

    public bool TrackBatch { get; set; }
    public bool TrackExpiry { get; set; }
    public bool TrackSerialNumber { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsActive { get; set; }

    public DateTime CreationTime { get; set; }
}
