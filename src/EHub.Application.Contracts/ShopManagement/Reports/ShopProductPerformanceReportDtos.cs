using System;
using System.Collections.Generic;

namespace EHub.ShopManagement.Reports;

public class GetShopProductPerformanceReportInput : ShopReportInputBase
{
    public Guid? ProductId { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public Guid? SupplierId { get; set; }
    public bool IncludeInactiveProducts { get; set; }
    public bool SoldOnly { get; set; }
    public bool PurchasedOnly { get; set; }
}

public class ShopProductPerformanceReportItemDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;

    public decimal OpeningStock { get; set; }
    public decimal PurchasedQuantity { get; set; }
    public decimal PurchaseReturnQuantity { get; set; }
    public decimal SaleQuantity { get; set; }
    public decimal SaleReturnQuantity { get; set; }
    public decimal AdjustmentInQuantity { get; set; }
    public decimal AdjustmentOutQuantity { get; set; }
    public decimal ClosingStock { get; set; }

    public decimal GrossSalesAmount { get; set; }
    public decimal SaleReturnAmount { get; set; }
    public decimal NetSalesAmount { get; set; }
    public decimal? PurchaseAmount { get; set; }
    public decimal? PurchaseReturnAmount { get; set; }
    public decimal? NetPurchaseAmount { get; set; }

    public decimal CurrentStock { get; set; }
    public decimal? CurrentStockValue { get; set; }
    public decimal? AverageSalePrice { get; set; }
    public decimal? AveragePurchaseCost { get; set; }
}

public class ShopProductPerformanceReportTotalsDto
{
    public int ProductCount { get; set; }
    public decimal TotalPurchasedQuantity { get; set; }
    public decimal TotalSaleQuantity { get; set; }
    public decimal TotalNetSalesAmount { get; set; }
    public decimal? TotalNetPurchaseAmount { get; set; }
    public decimal TotalCurrentStock { get; set; }
    public decimal? TotalCurrentStockValue { get; set; }
}

public class ShopProductPerformanceReportResultDto
{
    public List<ShopProductPerformanceReportItemDto> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public ShopProductPerformanceReportTotalsDto Totals { get; set; } = new();
}
