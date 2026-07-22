using System;

namespace EHub.ShopManagement.SupplierLedger;

public class ShopSupplierLedgerEntryDto
{
    public DateTime TransactionDate { get; set; }
    public ShopSupplierLedgerReferenceType ReferenceType { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? DebitAmount { get; set; }
    public decimal? CreditAmount { get; set; }
    public decimal? RunningBalance { get; set; }
    public string TransactionStatus { get; set; } = string.Empty;
}
