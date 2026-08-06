using System;
using System.Collections.Generic;
using EHub.ShopManagement.PrintSettings;

namespace EHub.ShopManagement.PrintTemplates;

// Pure print view model - never persisted, never carries a TenantId (tenant is resolved from
// ICurrentTenant server-side and never trusted from the frontend). Every document type maps into
// this exact same shape so the shared Angular print components never need per-document branching.
public class ShopPrintDocumentDto
{
    public string DocumentType { get; set; } = default!;
    public string DocumentNumber { get; set; } = default!;
    public DateTime DocumentDate { get; set; }

    public ShopPrintBusinessDto Business { get; set; } = default!;
    public ShopPrintPartyDto? Party { get; set; }

    public List<ShopPrintLineDto> Lines { get; set; } = new();

    public ShopPrintTotalsDto Totals { get; set; } = default!;
    public ShopPrintPaymentDto? Payment { get; set; }

    public string? Notes { get; set; }
    public string? FooterMessage { get; set; }

    public string? QrCodeValue { get; set; }
    public string? BarcodeValue { get; set; }

    public ShopPrintPaperSize PaperSize { get; set; }
}
