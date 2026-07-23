using System;

namespace EHub.ShopManagement.CustomerLedger;

public class ShopCustomerBalanceSummaryDto
{
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public decimal? OpeningBalance { get; set; }
    public decimal? TotalCompletedSales { get; set; }
    public decimal? TotalInitialPaidAtSale { get; set; }
    public decimal? TotalPostedCustomerPayments { get; set; }
    public decimal? CurrentBalance { get; set; }
    public decimal? ReceivableAmount { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}
