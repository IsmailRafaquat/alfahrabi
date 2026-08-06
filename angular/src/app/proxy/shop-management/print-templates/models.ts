import type { ShopPrintPaperSize } from '../print-settings/shop-print-paper-size.enum';

export interface ShopPrintBusinessDto {
  shopName?: string;
  addressLine?: string;
  phone?: string;
  email?: string;
  taxNumber?: string;
  currencyCode?: string;
  currencySymbol?: string;
  decimalPlaces: number;
  showLogo: boolean;
  showShopName: boolean;
  showAddress: boolean;
  showPhone: boolean;
  showEmail: boolean;
  showTaxNumber: boolean;
}

export interface ShopPrintDocumentDto {
  documentType?: string;
  documentNumber?: string;
  documentDate?: string;
  business: ShopPrintBusinessDto;
  party: ShopPrintPartyDto;
  lines: ShopPrintLineDto[];
  totals: ShopPrintTotalsDto;
  payment: ShopPrintPaymentDto;
  notes?: string;
  footerMessage?: string;
  qrCodeValue?: string;
  barcodeValue?: string;
  paperSize?: ShopPrintPaperSize;
}

export interface ShopPrintLineDto {
  productName?: string;
  itemCode?: string;
  unit?: string;
  quantity: number;
  unitPrice: number;
  discountAmount: number;
  taxAmount: number;
  lineTotal: number;
  batchNumber?: string;
  expiryDate?: string;
  manufacturingDate?: string;
  genericName?: string;
  dosageStrength?: string;
}

export interface ShopPrintPartyDto {
  label?: string;
  name?: string;
  phone?: string;
  address?: string;
  referenceNumber?: string;
}

export interface ShopPrintPaymentDto {
  method?: string;
  receivedAmount?: number;
  changeAmount?: number;
  referenceNumber?: string;
  splits: ShopPrintPaymentSplitDto[];
}

export interface ShopPrintPaymentSplitDto {
  method?: string;
  amount: number;
}

export interface ShopPrintTotalsDto {
  subTotal?: number;
  discountAmount?: number;
  taxAmount?: number;
  otherChargesAmount?: number;
  adjustmentAmount?: number;
  netAmount: number;
  paidAmount?: number;
  pendingAmount?: number;
  changeAmount?: number;
}
