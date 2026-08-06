using System;
using EHub.ShopManagement.GoodsReceipts;

namespace EHub.ShopManagement.Dashboard;

public class ShopDashboardRecentPurchaseDto
{
    public Guid PurchaseId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime DocumentDate { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public ShopGoodsReceiptPaymentStatus Status { get; set; }
}
