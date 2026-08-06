using System.Collections.Generic;

namespace EHub.ShopManagement.PrintTemplates;

public class ShopPrintPaymentDto
{
    public string Method { get; set; } = string.Empty;
    public decimal? ReceivedAmount { get; set; }
    public decimal? ChangeAmount { get; set; }
    public string? ReferenceNumber { get; set; }

    // Populated instead of Method/ReceivedAmount when a document was paid via more than one
    // method (e.g. part cash, part bank) - the Angular payment-summary component renders either
    // the single-method fields above or this split list, never both.
    public List<ShopPrintPaymentSplitDto>? Splits { get; set; }
}

public class ShopPrintPaymentSplitDto
{
    public string Method { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
