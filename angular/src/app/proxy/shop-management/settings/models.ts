import type { FullAuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateShopSettingDto {
  shopDisplayName: string;
  logoFileId?: string;
  phone?: string;
  alternatePhone?: string;
  email?: string;
  website?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  stateOrProvince?: string;
  postalCode?: string;
  country?: string;
  currencyCode: string;
  currencySymbol: string;
  taxNumber?: string;
  defaultTaxPercentage: number;
  invoicePrefix: string;
  purchaseOrderPrefix: string;
  receiptFooter?: string;
  returnPolicy?: string;
  allowNegativeStock: boolean;
  autoGenerateProductBarcode: boolean;
  autoGenerateInvoiceQrCode: boolean;
  defaultLowStockLevel: number;
  decimalPlaces: number;
}

export interface ShopSettingDto extends FullAuditedEntityDto<string> {
  shopDisplayName?: string;
  logoFileId?: string;
  phone?: string;
  alternatePhone?: string;
  email?: string;
  website?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  stateOrProvince?: string;
  postalCode?: string;
  country?: string;
  currencyCode?: string;
  currencySymbol?: string;
  taxNumber?: string;
  defaultTaxPercentage: number;
  invoicePrefix?: string;
  purchaseOrderPrefix?: string;
  receiptFooter?: string;
  returnPolicy?: string;
  allowNegativeStock: boolean;
  autoGenerateProductBarcode: boolean;
  autoGenerateInvoiceQrCode: boolean;
  defaultLowStockLevel: number;
  decimalPlaces: number;
  isConfigured: boolean;
}

export interface ShopSettingSetupStatusDto {
  isConfigured: boolean;
  shopDisplayName?: string;
}
