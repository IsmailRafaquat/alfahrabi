using System;
using System.Collections.Generic;
using EHub.ShopManagement.ProductBatches;

namespace EHub.ShopManagement.Reports;

public class GetShopBatchExpiryReportInput : ShopReportInputBase
{
    public Guid? ProductId { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public Guid? SupplierId { get; set; }
    public ShopProductBatchStatus? BatchStatus { get; set; }
    public DateTime? ExpiryFrom { get; set; }
    public DateTime? ExpiryTo { get; set; }
    public bool NearExpiryOnly { get; set; }
    public bool ExpiredOnly { get; set; }
    public bool ActiveOnly { get; set; }
    public bool HasAvailableStock { get; set; }
    public bool IncludeBlocked { get; set; } = true;
}

public class ShopBatchExpiryReportItemDto
{
    public Guid ProductBatchId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? DaysToExpiry { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal IssuedQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? StockValue { get; set; }
    public ShopProductBatchStatus Status { get; set; }
    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public DateTime? FirstReceivedDate { get; set; }
    public DateTime? LastMovementDate { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
}

public class ShopBatchExpiryReportTotalsDto
{
    public int TotalBatches { get; set; }
    public int ActiveBatches { get; set; }
    public int NearExpiryBatches { get; set; }
    public int ExpiredBatches { get; set; }
    public int BlockedBatches { get; set; }
    public int ExhaustedBatches { get; set; }
    public decimal TotalAvailableQuantity { get; set; }
    public decimal NearExpiryQuantity { get; set; }
    public decimal ExpiredQuantity { get; set; }
    public decimal? TotalBatchStockValue { get; set; }
    public decimal? ExpiredStockValue { get; set; }
}

public class ShopBatchExpiryReportResultDto
{
    public List<ShopBatchExpiryReportItemDto> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public ShopBatchExpiryReportTotalsDto Totals { get; set; } = new();
}
