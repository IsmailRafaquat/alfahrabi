import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopGoodsReceiptPaymentStatus {
  Unpaid = 0,
  PartiallyPaid = 1,
  Paid = 2,
}

export const shopGoodsReceiptPaymentStatusOptions = mapEnumToOptions(ShopGoodsReceiptPaymentStatus);
