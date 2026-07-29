using System;
using System.Collections.Generic;
using EHub.ShopManagement.CustomerLedger;

namespace EHub.ShopManagement.Reports;

public class GetShopCustomerTransactionReportInput : ShopReportInputBase
{
    public Guid CustomerId { get; set; }
    public ShopCustomerLedgerReferenceType? TransactionType { get; set; }
    public string? ReferenceNumber { get; set; }
}

public class ShopCustomerTransactionReportItemDto
{
    public Guid CustomerLedgerId { get; set; }
    public DateTime TransactionDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public ShopCustomerLedgerReferenceType TransactionType { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }
}

public class ShopCustomerTransactionReportTotalsDto
{
    public decimal OpeningBalance { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal ClosingBalance { get; set; }
}

public class ShopCustomerTransactionReportResultDto
{
    public List<ShopCustomerTransactionReportItemDto> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public ShopCustomerTransactionReportTotalsDto Totals { get; set; } = new();
}
