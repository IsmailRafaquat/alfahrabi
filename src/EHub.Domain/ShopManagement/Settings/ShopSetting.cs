using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.Settings;

public class ShopSetting : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string ShopDisplayName { get; private set; } = string.Empty;
    public Guid? LogoFileId { get; private set; }
    public string? Phone { get; private set; }
    public string? AlternatePhone { get; private set; }
    public string? Email { get; private set; }
    public string? Website { get; private set; }
    public string? AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string? City { get; private set; }
    public string? StateOrProvince { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Country { get; private set; }
    public string CurrencyCode { get; private set; } = "PKR";
    public string CurrencySymbol { get; private set; } = "₨";
    public string? TaxNumber { get; private set; }
    public decimal DefaultTaxPercentage { get; private set; }
    public string InvoicePrefix { get; private set; } = "INV";
    public string PurchaseOrderPrefix { get; private set; } = "PO";
    public string? ReceiptFooter { get; private set; }
    public string? ReturnPolicy { get; private set; }
    public bool AllowNegativeStock { get; private set; }
    public bool AutoGenerateProductBarcode { get; private set; } = true;
    public bool AutoGenerateInvoiceQrCode { get; private set; } = true;
    public decimal DefaultLowStockLevel { get; private set; } = 5;
    public int DecimalPlaces { get; private set; } = 2;
    public bool IsConfigured { get; private set; }

    private ShopSetting() { }

    public ShopSetting(Guid id, Guid tenantId) : base(id)
    {
        TenantId = tenantId;
    }

    public void Update(string shopDisplayName, Guid? logoFileId, string? phone,
        string? alternatePhone, string? email, string? website, string? addressLine1,
        string? addressLine2, string? city, string? stateOrProvince, string? postalCode,
        string? country, string currencyCode, string currencySymbol, string? taxNumber,
        decimal defaultTaxPercentage, string invoicePrefix, string purchaseOrderPrefix,
        string? receiptFooter, string? returnPolicy, bool allowNegativeStock,
        bool autoGenerateProductBarcode, bool autoGenerateInvoiceQrCode,
        decimal defaultLowStockLevel, int decimalPlaces)
    {
        ShopDisplayName = Check.NotNullOrWhiteSpace(shopDisplayName, nameof(shopDisplayName), ShopSettingConsts.ShopDisplayNameMaxLength).Trim();
        CurrencyCode = Check.NotNullOrWhiteSpace(currencyCode, nameof(currencyCode), ShopSettingConsts.CurrencyCodeMaxLength).Trim().ToUpperInvariant();
        CurrencySymbol = Check.NotNullOrWhiteSpace(currencySymbol, nameof(currencySymbol), ShopSettingConsts.CurrencySymbolMaxLength).Trim();
        InvoicePrefix = Check.NotNullOrWhiteSpace(invoicePrefix, nameof(invoicePrefix), ShopSettingConsts.PrefixMaxLength).Trim();
        PurchaseOrderPrefix = Check.NotNullOrWhiteSpace(purchaseOrderPrefix, nameof(purchaseOrderPrefix), ShopSettingConsts.PrefixMaxLength).Trim();
        if (defaultTaxPercentage is < 0 or > 100 || defaultLowStockLevel < 0 || decimalPlaces is < 0 or > 4)
            throw new BusinessException("ShopManagement:InvalidSettings");

        LogoFileId = logoFileId;
        Phone = phone?.Trim(); AlternatePhone = alternatePhone?.Trim(); Email = email?.Trim(); Website = website?.Trim();
        AddressLine1 = addressLine1?.Trim(); AddressLine2 = addressLine2?.Trim(); City = city?.Trim();
        StateOrProvince = stateOrProvince?.Trim(); PostalCode = postalCode?.Trim(); Country = country?.Trim();
        TaxNumber = taxNumber?.Trim(); DefaultTaxPercentage = defaultTaxPercentage;
        ReceiptFooter = receiptFooter?.Trim(); ReturnPolicy = returnPolicy?.Trim();
        AllowNegativeStock = allowNegativeStock; AutoGenerateProductBarcode = autoGenerateProductBarcode;
        AutoGenerateInvoiceQrCode = autoGenerateInvoiceQrCode; DefaultLowStockLevel = defaultLowStockLevel;
        DecimalPlaces = decimalPlaces; IsConfigured = true;
    }
}
