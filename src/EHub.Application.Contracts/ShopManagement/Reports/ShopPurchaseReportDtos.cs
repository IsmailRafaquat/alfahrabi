using System;
using System.Collections.Generic;
using EHub.ShopManagement.GoodsReceipts;

namespace EHub.ShopManagement.Reports;

public class GetShopPurchaseReportInput : ShopReportInputBase
{
    public Guid? SupplierId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public ShopGoodsReceiptStatus? GoodsReceiptStatus { get; set; }
    public ShopGoodsReceiptPaymentStatus? PaymentStatus { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public bool? HasPendingAmount { get; set; }
}

public class ShopPurchaseReportItemDto
{
    public Guid GoodsReceiptId { get; set; }
    public string GoodsReceiptNumber { get; set; } = string.Empty;
    public DateTime GoodsReceiptDate { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;

    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal PurchaseReturnAmount { get; set; }
    public decimal FinalPurchaseAmount { get; set; }

    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }
    public ShopGoodsReceiptStatus Status { get; set; }
    public ShopGoodsReceiptPaymentStatus PaymentStatus { get; set; }
    public DateTime CreationTime { get; set; }
}

public class ShopPurchaseReportTotalsDto
{
    public int PurchaseCount { get; set; }
    public decimal GrossPurchases { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal NetPurchasesBeforeReturns { get; set; }
    public decimal PurchaseReturnAmount { get; set; }
    public decimal FinalNetPurchases { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal TotalQuantityPurchased { get; set; }
    public decimal AveragePurchaseValue { get; set; }
}

public class ShopPurchaseReportResultDto
{
    public List<ShopPurchaseReportItemDto> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public ShopPurchaseReportTotalsDto Totals { get; set; } = new();
}
