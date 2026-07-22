using System;

namespace EHub.ShopManagement.SupplierPayments;

public class ShopSupplierOutstandingReceiptDto
{
    public Guid GoodsReceiptId { get; set; }
    public string GoodsReceiptNumber { get; set; } = string.Empty;
    public string? SupplierInvoiceNumber { get; set; }
    public DateTime ReceiptDate { get; set; }
    public decimal? GrandTotal { get; set; }
    public decimal? PaidAmount { get; set; }
    public decimal? PendingAmount { get; set; }
}
