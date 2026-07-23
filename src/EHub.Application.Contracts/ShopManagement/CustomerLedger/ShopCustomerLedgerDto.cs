using System;
using System.Collections.Generic;

namespace EHub.ShopManagement.CustomerLedger;

public class ShopCustomerLedgerDto
{
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public decimal? OpeningBalance { get; set; }
    public decimal? TotalSales { get; set; }
    public decimal? TotalInitialPaid { get; set; }
    public decimal? TotalAdditionalPayments { get; set; }
    public decimal? TotalDebit { get; set; }
    public decimal? TotalCredit { get; set; }
    public decimal? ClosingBalance { get; set; }
    public decimal? ReceivableAmount { get; set; }
    public decimal? AdvanceAmount { get; set; }

    public List<ShopCustomerLedgerEntryDto> Entries { get; set; } = new();
}
