import type { ShopPrintPaperSize } from './shop-print-paper-size.enum';
import type { ShopThermalFontSize } from './shop-thermal-font-size.enum';
import type { ShopThermalPrintDensity } from './shop-thermal-print-density.enum';
import type { FullAuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateShopPrintSettingsDto {
  defaultPrintPaperSize?: ShopPrintPaperSize;
  printHeaderLogo: boolean;
  printShopName: boolean;
  printShopAddress: boolean;
  printShopPhone: boolean;
  printShopEmail: boolean;
  printTaxNumber: boolean;
  printFooterMessage?: string;
  printTermsAndConditions?: string;
  printQrCode: boolean;
  printBarcode: boolean;
  printCustomerCopyLabel: string;
  printDuplicateCopyLabel: string;
  printItemCode: boolean;
  printUnit: boolean;
  printBatchNumber: boolean;
  printExpiryDate: boolean;
  printDiscount: boolean;
  printTax: boolean;
  printPaymentDetails: boolean;
  printCashierName: boolean;
  printDateTime: boolean;
  printPageNumberForA4: boolean;
  thermalFontSize?: ShopThermalFontSize;
  thermalPrintDensity?: ShopThermalPrintDensity;
  thermalAutoCut: boolean;
  thermalOpenCashDrawer: boolean;
}

export interface ShopPrintSettingsDto extends FullAuditedEntityDto<string> {
  defaultPrintPaperSize?: ShopPrintPaperSize;
  printHeaderLogo: boolean;
  printShopName: boolean;
  printShopAddress: boolean;
  printShopPhone: boolean;
  printShopEmail: boolean;
  printTaxNumber: boolean;
  printFooterMessage?: string;
  printTermsAndConditions?: string;
  printQrCode: boolean;
  printBarcode: boolean;
  printCustomerCopyLabel?: string;
  printDuplicateCopyLabel?: string;
  printItemCode: boolean;
  printUnit: boolean;
  printBatchNumber: boolean;
  printExpiryDate: boolean;
  printDiscount: boolean;
  printTax: boolean;
  printPaymentDetails: boolean;
  printCashierName: boolean;
  printDateTime: boolean;
  printPageNumberForA4: boolean;
  thermalFontSize?: ShopThermalFontSize;
  thermalPrintDensity?: ShopThermalPrintDensity;
  thermalAutoCut: boolean;
  thermalOpenCashDrawer: boolean;
}
