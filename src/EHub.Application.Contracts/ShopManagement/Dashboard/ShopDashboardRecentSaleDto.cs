using System;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.Sales;

namespace EHub.ShopManagement.Dashboard;

public class ShopDashboardRecentSaleDto
{
    public Guid SaleId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    /// <summary>Reuses the existing Unpaid/PartiallyPaid/Paid enum defined for Goods Receipts.</summary>
    public ShopGoodsReceiptPaymentStatus PaymentStatus { get; set; }
    public ShopSaleStatus SaleStatus { get; set; }
}
