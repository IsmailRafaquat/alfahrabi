using System;
using System.Collections.Generic;

namespace EHub.ShopManagement.SupplierLedger;

public class ShopSupplierStatementDto
{
    public string ShopName { get; set; } = string.Empty;
    public string? ShopAddress { get; set; }
    public string? ShopPhone { get; set; }
    public string? ShopEmail { get; set; }

    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string? SupplierAddress { get; set; }
    public string? SupplierPhone { get; set; }
    public string? SupplierEmail { get; set; }

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public DateTime GeneratedDate { get; set; }

    public decimal? OpeningBalance { get; set; }
    public decimal? TotalDebit { get; set; }
    public decimal? TotalCredit { get; set; }
    public decimal? ClosingBalance { get; set; }
    public decimal? PayableAmount { get; set; }
    public decimal? AdvanceAmount { get; set; }

    public List<ShopSupplierLedgerEntryDto> Entries { get; set; } = new();
}
