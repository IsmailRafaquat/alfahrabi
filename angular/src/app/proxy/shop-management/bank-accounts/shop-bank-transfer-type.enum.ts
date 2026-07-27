import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopBankTransferType {
  CashToBank = 0,
  BankToCash = 1,
  BankToBank = 2,
}

export const shopBankTransferTypeOptions = mapEnumToOptions(ShopBankTransferType);
