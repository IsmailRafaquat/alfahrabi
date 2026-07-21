using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Settings;

public class ShopSettingDto : FullAuditedEntityDto<Guid>
{
    public string ShopDisplayName { get; set; } = string.Empty;
    public Guid? LogoFileId { get; set; }
    public string? Phone { get; set; }
    public string? AlternatePhone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? StateOrProvince { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string CurrencyCode { get; set; } = "PKR";
    public string CurrencySymbol { get; set; } = "₨";
    public string? TaxNumber { get; set; }
    public decimal DefaultTaxPercentage { get; set; }
    public string InvoicePrefix { get; set; } = "INV";
    public string PurchaseOrderPrefix { get; set; } = "PO";
    public string? ReceiptFooter { get; set; }
    public string? ReturnPolicy { get; set; }
    public bool AllowNegativeStock { get; set; }
    public bool AutoGenerateProductBarcode { get; set; } = true;
    public bool AutoGenerateInvoiceQrCode { get; set; } = true;
    public decimal DefaultLowStockLevel { get; set; } = 5;
    public int DecimalPlaces { get; set; } = 2;
    public bool IsConfigured { get; set; }
}
