using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.PrintSettings;

// Single-row-per-tenant settings aggregate, same shape as ShopSetting (EHub.ShopManagement.Settings) -
// located by TenantId (unique index), not a fixed id. Kept as its own table rather than adding ~25
// columns to ShopSetting, which has never had a column added since it was created.
//
// Business info shown on a receipt (shop name/address/phone/email/tax number/currency/logo/receipt
// footer) is NOT duplicated here - it is read from ShopSetting at print time. Every "Print*" bool
// below only controls whether that already-existing piece of business info is shown on a receipt.
public class ShopPrintSetting : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }

    public ShopPrintPaperSize DefaultPrintPaperSize { get; private set; } = ShopPrintPaperSize.Thermal80Mm;

    public bool PrintHeaderLogo { get; private set; }
    public bool PrintShopName { get; private set; } = true;
    public bool PrintShopAddress { get; private set; } = true;
    public bool PrintShopPhone { get; private set; } = true;
    public bool PrintShopEmail { get; private set; }
    public bool PrintTaxNumber { get; private set; }

    public string? PrintFooterMessage { get; private set; }
    public string? PrintTermsAndConditions { get; private set; }

    public bool PrintQrCode { get; private set; }
    public bool PrintBarcode { get; private set; }

    public string PrintCustomerCopyLabel { get; private set; } = "Customer Copy";
    public string PrintDuplicateCopyLabel { get; private set; } = "Duplicate Copy";

    public bool PrintItemCode { get; private set; }
    public bool PrintUnit { get; private set; } = true;
    public bool PrintBatchNumber { get; private set; } = true;
    public bool PrintExpiryDate { get; private set; } = true;
    public bool PrintDiscount { get; private set; } = true;
    public bool PrintTax { get; private set; } = true;
    public bool PrintPaymentDetails { get; private set; } = true;
    public bool PrintCashierName { get; private set; } = true;
    public bool PrintDateTime { get; private set; } = true;
    public bool PrintPageNumberForA4 { get; private set; } = true;

    public ShopThermalFontSize ThermalFontSize { get; private set; } = ShopThermalFontSize.Medium;
    public ShopThermalPrintDensity ThermalPrintDensity { get; private set; } = ShopThermalPrintDensity.Normal;

    // Inert metadata only - browser printing cannot drive real printer hardware. Kept for a future
    // optional local print-bridge integration; never wired to any hardware call from this codebase.
    public bool ThermalAutoCut { get; private set; }
    public bool ThermalOpenCashDrawer { get; private set; }

    private ShopPrintSetting() { }

    public ShopPrintSetting(Guid id, Guid tenantId) : base(id)
    {
        TenantId = tenantId;
    }

    public void Update(
        ShopPrintPaperSize defaultPrintPaperSize,
        bool printHeaderLogo, bool printShopName, bool printShopAddress, bool printShopPhone,
        bool printShopEmail, bool printTaxNumber,
        string? printFooterMessage, string? printTermsAndConditions,
        bool printQrCode, bool printBarcode,
        string printCustomerCopyLabel, string printDuplicateCopyLabel,
        bool printItemCode, bool printUnit, bool printBatchNumber, bool printExpiryDate,
        bool printDiscount, bool printTax, bool printPaymentDetails, bool printCashierName,
        bool printDateTime, bool printPageNumberForA4,
        ShopThermalFontSize thermalFontSize, ShopThermalPrintDensity thermalPrintDensity,
        bool thermalAutoCut, bool thermalOpenCashDrawer)
    {
        DefaultPrintPaperSize = defaultPrintPaperSize;
        PrintHeaderLogo = printHeaderLogo;
        PrintShopName = printShopName;
        PrintShopAddress = printShopAddress;
        PrintShopPhone = printShopPhone;
        PrintShopEmail = printShopEmail;
        PrintTaxNumber = printTaxNumber;
        PrintFooterMessage = printFooterMessage?.Trim();
        PrintTermsAndConditions = printTermsAndConditions?.Trim();
        PrintQrCode = printQrCode;
        PrintBarcode = printBarcode;
        PrintCustomerCopyLabel = string.IsNullOrWhiteSpace(printCustomerCopyLabel) ? "Customer Copy" : printCustomerCopyLabel.Trim();
        PrintDuplicateCopyLabel = string.IsNullOrWhiteSpace(printDuplicateCopyLabel) ? "Duplicate Copy" : printDuplicateCopyLabel.Trim();
        PrintItemCode = printItemCode;
        PrintUnit = printUnit;
        PrintBatchNumber = printBatchNumber;
        PrintExpiryDate = printExpiryDate;
        PrintDiscount = printDiscount;
        PrintTax = printTax;
        PrintPaymentDetails = printPaymentDetails;
        PrintCashierName = printCashierName;
        PrintDateTime = printDateTime;
        PrintPageNumberForA4 = printPageNumberForA4;
        ThermalFontSize = thermalFontSize;
        ThermalPrintDensity = thermalPrintDensity;
        ThermalAutoCut = thermalAutoCut;
        ThermalOpenCashDrawer = thermalOpenCashDrawer;
    }
}
