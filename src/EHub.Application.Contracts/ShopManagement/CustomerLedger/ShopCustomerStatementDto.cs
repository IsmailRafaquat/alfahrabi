using System;
using System.Collections.Generic;

namespace EHub.ShopManagement.CustomerLedger;

public class ShopCustomerStatementDto
{
    public string ShopName { get; set; } = string.Empty;
    public string? ShopAddress { get; set; }
    public string? ShopPhone { get; set; }
    public string? ShopEmail { get; set; }

    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerAddress { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public DateTime GeneratedDate { get; set; }

    public decimal? OpeningBalance { get; set; }
    public decimal? TotalDebit { get; set; }
    public decimal? TotalCredit { get; set; }
    public decimal? ClosingBalance { get; set; }
    public decimal? ReceivableAmount { get; set; }
    public decimal? AdvanceAmount { get; set; }

    public List<ShopCustomerLedgerEntryDto> Entries { get; set; } = new();
}
