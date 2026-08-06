import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopCustomerLedgerReferenceType {
  OpeningBalance = 0,
  Sale = 1,
  CustomerPayment = 2,
  SaleReturn = 3,
}

export const shopCustomerLedgerReferenceTypeOptions = mapEnumToOptions(ShopCustomerLedgerReferenceType);
