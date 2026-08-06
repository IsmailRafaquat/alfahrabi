import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopPurchaseOrderStatus {
  Draft = 0,
  PendingApproval = 1,
  Approved = 2,
  Rejected = 3,
  PartiallyReceived = 4,
  FullyReceived = 5,
  Cancelled = 6,
  Closed = 7,
}

export const shopPurchaseOrderStatusOptions = mapEnumToOptions(ShopPurchaseOrderStatus);
