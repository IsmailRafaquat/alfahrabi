import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopBankTransferStatus {
  Draft = 0,
  Posted = 1,
  Cancelled = 2,
}

export const shopBankTransferStatusOptions = mapEnumToOptions(ShopBankTransferStatus);
