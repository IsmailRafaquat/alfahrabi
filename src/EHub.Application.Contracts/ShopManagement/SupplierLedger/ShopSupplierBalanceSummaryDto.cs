using System;

namespace EHub.ShopManagement.SupplierLedger;

public class ShopSupplierBalanceSummaryDto
{
    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;

    public decimal? OpeningBalance { get; set; }
    public decimal? TotalCompletedPurchases { get; set; }
    public decimal? TotalCompletedReturns { get; set; }
    public decimal? TotalPostedPayments { get; set; }
    public decimal? CurrentBalance { get; set; }
    public decimal? PayableAmount { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}
