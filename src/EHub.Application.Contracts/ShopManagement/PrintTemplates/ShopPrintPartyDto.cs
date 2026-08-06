namespace EHub.ShopManagement.PrintTemplates;

// Represents whichever counter-party the document concerns (customer, supplier, or a bank
// account/cash register for internal money-movement documents). Only populated fields render -
// the Angular header never prints an empty "Phone:" label. Bank account numbers are masked to
// the last 4 digits by the mapper before this DTO is built - never the full number.
public class ShopPrintPartyDto
{
    public string Label { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? ReferenceNumber { get; set; }
}
