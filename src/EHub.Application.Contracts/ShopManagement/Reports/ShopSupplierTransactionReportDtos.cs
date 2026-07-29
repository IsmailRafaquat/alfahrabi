using System;
using System.Collections.Generic;
using EHub.ShopManagement.SupplierLedger;

namespace EHub.ShopManagement.Reports;

public class GetShopSupplierTransactionReportInput : ShopReportInputBase
{
    public Guid SupplierId { get; set; }
    public ShopSupplierLedgerReferenceType? TransactionType { get; set; }
    public string? ReferenceNumber { get; set; }
}

public class ShopSupplierTransactionReportItemDto
{
    public Guid SupplierLedgerId { get; set; }
    public DateTime TransactionDate { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public ShopSupplierLedgerReferenceType TransactionType { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }
}

public class ShopSupplierTransactionReportTotalsDto
{
    public decimal OpeningBalance { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal ClosingBalance { get; set; }
}

public class ShopSupplierTransactionReportResultDto
{
    public List<ShopSupplierTransactionReportItemDto> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public ShopSupplierTransactionReportTotalsDto Totals { get; set; } = new();
}
