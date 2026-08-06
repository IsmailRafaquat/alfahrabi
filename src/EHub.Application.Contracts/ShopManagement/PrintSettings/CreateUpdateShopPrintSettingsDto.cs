using System.ComponentModel.DataAnnotations;
using EHub.ShopManagement.PrintSettings;

namespace EHub.ShopManagement.PrintSettings;

public class CreateUpdateShopPrintSettingsDto
{
    public ShopPrintPaperSize DefaultPrintPaperSize { get; set; } = ShopPrintPaperSize.Thermal80Mm;

    public bool PrintHeaderLogo { get; set; }
    public bool PrintShopName { get; set; } = true;
    public bool PrintShopAddress { get; set; } = true;
    public bool PrintShopPhone { get; set; } = true;
    public bool PrintShopEmail { get; set; }
    public bool PrintTaxNumber { get; set; }

    [StringLength(ShopPrintSettingConsts.FooterMessageMaxLength)]
    public string? PrintFooterMessage { get; set; }

    [StringLength(ShopPrintSettingConsts.TermsAndConditionsMaxLength)]
    public string? PrintTermsAndConditions { get; set; }

    public bool PrintQrCode { get; set; }
    public bool PrintBarcode { get; set; }

    [Required, StringLength(ShopPrintSettingConsts.LabelMaxLength)]
    public string PrintCustomerCopyLabel { get; set; } = "Customer Copy";

    [Required, StringLength(ShopPrintSettingConsts.LabelMaxLength)]
    public string PrintDuplicateCopyLabel { get; set; } = "Duplicate Copy";

    public bool PrintItemCode { get; set; }
    public bool PrintUnit { get; set; } = true;
    public bool PrintBatchNumber { get; set; } = true;
    public bool PrintExpiryDate { get; set; } = true;
    public bool PrintDiscount { get; set; } = true;
    public bool PrintTax { get; set; } = true;
    public bool PrintPaymentDetails { get; set; } = true;
    public bool PrintCashierName { get; set; } = true;
    public bool PrintDateTime { get; set; } = true;
    public bool PrintPageNumberForA4 { get; set; } = true;

    public ShopThermalFontSize ThermalFontSize { get; set; } = ShopThermalFontSize.Medium;
    public ShopThermalPrintDensity ThermalPrintDensity { get; set; } = ShopThermalPrintDensity.Normal;

    public bool ThermalAutoCut { get; set; }
    public bool ThermalOpenCashDrawer { get; set; }
}
