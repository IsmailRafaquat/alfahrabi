import type { GetShopCustomerLedgerInput, ShopCustomerBalanceSummaryDto, ShopCustomerLedgerDto, ShopCustomerStatementDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopCustomerLedgerService {
  apiName = 'Default';
  

  getBalanceSummary = (customerId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCustomerBalanceSummaryDto>({
      method: 'GET',
      url: `/api/app/shop-customer-ledger/balance-summary/${customerId}`,
    },
    { apiName: this.apiName,...config });
  

  getLedger = (input: GetShopCustomerLedgerInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCustomerLedgerDto>({
      method: 'GET',
      url: '/api/app/shop-customer-ledger/ledger',
      params: { customerId: input.customerId, dateFrom: input.dateFrom, dateTo: input.dateTo, referenceType: input.referenceType, filter: input.filter },
    },
    { apiName: this.apiName,...config });
  

  getStatement = (input: GetShopCustomerLedgerInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCustomerStatementDto>({
      method: 'GET',
      url: '/api/app/shop-customer-ledger/statement',
      params: { customerId: input.customerId, dateFrom: input.dateFrom, dateTo: input.dateTo, referenceType: input.referenceType, filter: input.filter },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
