namespace EHub.ShopManagement.PrintTemplates;

// Business/letterhead info sourced from ShopSetting + ShopPrintSetting at print time. Each
// "Show*" flag mirrors a ShopPrintSetting toggle so the Angular header component never needs to
// separately fetch settings - the mapper has already applied the tenant's print preferences.
public class ShopPrintBusinessDto
{
    public string ShopName { get; set; } = string.Empty;
    public string? AddressLine { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxNumber { get; set; }
    public string CurrencyCode { get; set; } = "PKR";
    public string CurrencySymbol { get; set; } = "₨";
    public int DecimalPlaces { get; set; } = 2;

    public bool ShowLogo { get; set; }
    public bool ShowShopName { get; set; } = true;
    public bool ShowAddress { get; set; } = true;
    public bool ShowPhone { get; set; } = true;
    public bool ShowEmail { get; set; }
    public bool ShowTaxNumber { get; set; }
}
