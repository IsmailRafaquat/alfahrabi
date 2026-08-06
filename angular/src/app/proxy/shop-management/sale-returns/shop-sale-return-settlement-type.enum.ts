import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopSaleReturnSettlementType {
  CustomerCredit = 0,
  CashRefund = 1,
  Exchange = 2,
  BankRefund = 3,
}

export const shopSaleReturnSettlementTypeOptions = mapEnumToOptions(ShopSaleReturnSettlementType);
