using System;
using System.Collections.Generic;

namespace EHub.ShopManagement.SupplierLedger;

public class ShopSupplierLedgerDto
{
    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public decimal? OpeningBalance { get; set; }
    public decimal? TotalDebit { get; set; }
    public decimal? TotalCredit { get; set; }
    public decimal? ClosingBalance { get; set; }
    public decimal? PayableAmount { get; set; }
    public decimal? AdvanceAmount { get; set; }

    // Splits of TotalDebit/TotalCredit by source, for the ledger page's summary cards.
    // Unaffected by the ReferenceType/Filter display filters, same basis as TotalDebit/TotalCredit.
    public decimal? TotalPurchases { get; set; }
    public decimal? TotalPayments { get; set; }
    public decimal? TotalReturns { get; set; }

    public List<ShopSupplierLedgerEntryDto> Entries { get; set; } = new();
}
