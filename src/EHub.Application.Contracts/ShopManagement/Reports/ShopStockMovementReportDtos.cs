using System;
using System.Collections.Generic;
using EHub.ShopManagement.StockTransactions;

namespace EHub.ShopManagement.Reports;

public class GetShopStockMovementReportInput : ShopReportInputBase
{
    public Guid? ProductId { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public Guid? ProductBatchId { get; set; }
    public ShopStockTransactionType? TransactionType { get; set; }
    public ShopStockReferenceType? ReferenceType { get; set; }
    public string? ReferenceNumber { get; set; }
    public ShopStockQuantityDirection QuantityDirection { get; set; } = ShopStockQuantityDirection.All;
    public Guid? CreatedByUserId { get; set; }
}

public class ShopStockMovementReportItemDto
{
    public Guid StockTransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public Guid? ProductBatchId { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public ShopStockTransactionType TransactionType { get; set; }
    public ShopStockReferenceType ReferenceType { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public decimal ProductBalanceQuantity { get; set; }
    public decimal? BatchBalanceQuantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
    public string? Notes { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreationTime { get; set; }
}

public class ShopStockMovementReportTotalsDto
{
    public int TransactionCount { get; set; }
    public decimal TotalQuantityIn { get; set; }
    public decimal TotalQuantityOut { get; set; }
    public decimal NetQuantityMovement { get; set; }
    public decimal? TotalStockInValue { get; set; }
    public decimal? TotalStockOutValue { get; set; }
}

public class ShopStockMovementReportResultDto
{
    public List<ShopStockMovementReportItemDto> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public ShopStockMovementReportTotalsDto Totals { get; set; } = new();
}
