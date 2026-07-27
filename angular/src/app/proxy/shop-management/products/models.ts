import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateShopProductDto extends ShopProductEditDtoBase {
}

export interface GetShopProductsInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  categoryId?: string;
  unitId?: string;
  isActive?: boolean;
  isTaxable?: boolean;
  trackBatch?: boolean;
  trackExpiry?: boolean;
  lowStockOnly: boolean;
}

export interface ShopProductDto extends EntityDto<string> {
  categoryId?: string;
  categoryName?: string;
  unitId?: string;
  unitName?: string;
  unitShortName?: string;
  unitAllowDecimal: boolean;
  name?: string;
  code?: string;
  sku?: string;
  barcode?: string;
  description?: string;
  brand?: string;
  model?: string;
  purchasePrice?: number;
  salePrice: number;
  wholesalePrice?: number;
  minimumSalePrice?: number;
  taxPercentage: number;
  currentStock: number;
  minimumStockLevel: number;
  maximumStockLevel?: number;
  reorderLevel: number;
  trackBatch: boolean;
  trackExpiry: boolean;
  expiryAlertDays?: number;
  blockExpiredSale: boolean;
  trackSerialNumber: boolean;
  isTaxable: boolean;
  isActive: boolean;
  creationTime?: string;
}

export interface ShopProductEditDtoBase {
  categoryId: string;
  unitId: string;
  name: string;
  code: string;
  sku?: string;
  barcode?: string;
  description?: string;
  brand?: string;
  model?: string;
  purchasePrice: number;
  salePrice: number;
  wholesalePrice?: number;
  minimumSalePrice?: number;
  taxPercentage: number;
  minimumStockLevel: number;
  maximumStockLevel?: number;
  reorderLevel: number;
  trackBatch: boolean;
  trackExpiry: boolean;
  expiryAlertDays?: number;
  blockExpiredSale: boolean;
  trackSerialNumber: boolean;
  isTaxable: boolean;
  isActive: boolean;
}

export interface ShopProductLookupDto extends EntityDto<string> {
  name?: string;
  code?: string;
  barcode?: string;
  categoryName?: string;
  unitName?: string;
  unitShortName?: string;
  unitAllowDecimal: boolean;
  salePrice: number;
  currentStock: number;
  isActive: boolean;
}

export interface UpdateShopProductDto extends ShopProductEditDtoBase {
}
