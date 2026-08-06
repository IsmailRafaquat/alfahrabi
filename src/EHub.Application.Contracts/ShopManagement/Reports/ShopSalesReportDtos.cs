using System;
using System.Collections.Generic;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.Sales;

namespace EHub.ShopManagement.Reports;

public class GetShopSalesReportInput : ShopReportInputBase
{
    public Guid? CustomerId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public ShopSaleStatus? SaleStatus { get; set; }
    public ShopGoodsReceiptPaymentStatus? PaymentStatus { get; set; }
    public ShopSalePaymentMethod? PaymentMethod { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public bool? HasPendingAmount { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public ShopSalesReportGroupBy GroupBy { get; set; } = ShopSalesReportGroupBy.None;
}

public class ShopSalesReportItemDto
{
    public Guid SaleId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }

    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal ReturnAmount { get; set; }
    public decimal FinalSalesAmount { get; set; }

    public ShopGoodsReceiptPaymentStatus PaymentStatus { get; set; }
    public ShopSaleStatus SaleStatus { get; set; }
    public ShopSalePaymentMethod PaymentMethod { get; set; }

    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreationTime { get; set; }
}

public class ShopSalesReportGroupItemDto
{
    public string GroupKey { get; set; } = string.Empty;
    public string GroupLabel { get; set; } = string.Empty;
    public int SaleCount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal ReturnAmount { get; set; }
    public decimal FinalSalesAmount { get; set; }
    public decimal TotalQuantity { get; set; }
}

public class ShopSalesReportTotalsDto
{
    public int SaleCount { get; set; }
    public decimal GrossSales { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal NetSalesBeforeReturns { get; set; }
    public decimal SaleReturnAmount { get; set; }
    public decimal FinalNetSales { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal TotalQuantitySold { get; set; }
    public decimal AverageSaleValue { get; set; }
}

public class ShopSalesReportResultDto
{
    public List<ShopSalesReportItemDto> Items { get; set; } = new();
    public List<ShopSalesReportGroupItemDto>? Groups { get; set; }
    public long TotalCount { get; set; }
    public ShopSalesReportTotalsDto Totals { get; set; } = new();
}
