using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.Settings;

public class CreateUpdateShopSettingDto
{
    [Required, StringLength(ShopSettingConsts.ShopDisplayNameMaxLength)] public string ShopDisplayName { get; set; } = string.Empty;
    public Guid? LogoFileId { get; set; }
    [StringLength(ShopSettingConsts.PhoneMaxLength)] public string? Phone { get; set; }
    [StringLength(ShopSettingConsts.PhoneMaxLength)] public string? AlternatePhone { get; set; }
    [EmailAddress, StringLength(ShopSettingConsts.EmailMaxLength)] public string? Email { get; set; }
    [StringLength(ShopSettingConsts.WebsiteMaxLength)] public string? Website { get; set; }
    [StringLength(ShopSettingConsts.AddressMaxLength)] public string? AddressLine1 { get; set; }
    [StringLength(ShopSettingConsts.AddressMaxLength)] public string? AddressLine2 { get; set; }
    [StringLength(ShopSettingConsts.LocationMaxLength)] public string? City { get; set; }
    [StringLength(ShopSettingConsts.LocationMaxLength)] public string? StateOrProvince { get; set; }
    [StringLength(ShopSettingConsts.PostalCodeMaxLength)] public string? PostalCode { get; set; }
    [StringLength(ShopSettingConsts.LocationMaxLength)] public string? Country { get; set; }
    [Required, StringLength(ShopSettingConsts.CurrencyCodeMaxLength, MinimumLength = 3)] public string CurrencyCode { get; set; } = "PKR";
    [Required, StringLength(ShopSettingConsts.CurrencySymbolMaxLength)] public string CurrencySymbol { get; set; } = "₨";
    [StringLength(ShopSettingConsts.TaxNumberMaxLength)] public string? TaxNumber { get; set; }
    [Range(0, 100)] public decimal DefaultTaxPercentage { get; set; }
    [Required, StringLength(ShopSettingConsts.PrefixMaxLength)] public string InvoicePrefix { get; set; } = "INV";
    [Required, StringLength(ShopSettingConsts.PrefixMaxLength)] public string PurchaseOrderPrefix { get; set; } = "PO";
    [StringLength(ShopSettingConsts.LongTextMaxLength)] public string? ReceiptFooter { get; set; }
    [StringLength(ShopSettingConsts.LongTextMaxLength)] public string? ReturnPolicy { get; set; }
    public bool AllowNegativeStock { get; set; }
    public bool AutoGenerateProductBarcode { get; set; } = true;
    public bool AutoGenerateInvoiceQrCode { get; set; } = true;
    [Range(0, double.MaxValue)] public decimal DefaultLowStockLevel { get; set; } = 5;
    [Range(0, 4)] public int DecimalPlaces { get; set; } = 2;
}
