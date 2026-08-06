using System;
using System.Collections.Generic;

namespace EHub.ShopManagement.Reports;

public class GetShopStockReportInput : ShopReportInputBase
{
    public Guid? ProductId { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public Guid? UnitId { get; set; }
    public Guid? SupplierId { get; set; }
    public ShopStockReportStatus StockStatus { get; set; } = ShopStockReportStatus.All;
    public bool? BatchTracked { get; set; }
    public bool? ExpiryTracked { get; set; }
    public bool LowStockOnly { get; set; }
    public bool OutOfStockOnly { get; set; }
    public bool InStockOnly { get; set; }
    public bool IncludeInactiveProducts { get; set; }
    public decimal? MinimumStock { get; set; }
    public decimal? MaximumStock { get; set; }
}

public class ShopStockReportItemDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string UnitShortName { get; set; } = string.Empty;

    public decimal CurrentStock { get; set; }
    public decimal ReorderLevel { get; set; }
    public ShopStockReportStatus StockStatus { get; set; }

    public decimal? PurchasePrice { get; set; }
    public decimal? AverageCost { get; set; }
    public decimal SalePrice { get; set; }
    public decimal? StockValue { get; set; }
    public decimal PotentialSaleValue { get; set; }
    public decimal? PotentialMargin { get; set; }

    public bool TrackBatch { get; set; }
    public bool TrackExpiry { get; set; }
    public int ActiveBatchCount { get; set; }
    public decimal NearExpiryQuantity { get; set; }
    public decimal ExpiredQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class ShopStockReportTotalsDto
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int InStockProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public int NegativeStockProducts { get; set; }
    public decimal TotalStockQuantity { get; set; }
    public decimal? TotalStockValue { get; set; }
    public decimal TotalPotentialSaleValue { get; set; }
    public decimal? TotalPotentialMargin { get; set; }
}

public class ShopStockReportResultDto
{
    public List<ShopStockReportItemDto> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public ShopStockReportTotalsDto Totals { get; set; } = new();
}
