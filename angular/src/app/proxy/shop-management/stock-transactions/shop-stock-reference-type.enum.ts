import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopStockReferenceType {
  GoodsReceipt = 0,
  PurchaseReturn = 1,
  Sale = 2,
  SaleReturn = 3,
  StockAdjustment = 4,
  OpeningStock = 5,
}

export const shopStockReferenceTypeOptions = mapEnumToOptions(ShopStockReferenceType);
