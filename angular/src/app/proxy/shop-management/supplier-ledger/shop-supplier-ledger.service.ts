import type { GetShopSupplierLedgerInput, ShopSupplierBalanceSummaryDto, ShopSupplierLedgerDto, ShopSupplierStatementDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopSupplierLedgerService {
  apiName = 'Default';
  

  getBalanceSummary = (supplierId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSupplierBalanceSummaryDto>({
      method: 'GET',
      url: `/api/app/shop-supplier-ledger/balance-summary/${supplierId}`,
    },
    { apiName: this.apiName,...config });
  

  getLedger = (input: GetShopSupplierLedgerInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSupplierLedgerDto>({
      method: 'GET',
      url: '/api/app/shop-supplier-ledger/ledger',
      params: { supplierId: input.supplierId, dateFrom: input.dateFrom, dateTo: input.dateTo, referenceType: input.referenceType, filter: input.filter },
    },
    { apiName: this.apiName,...config });
  

  getStatement = (input: GetShopSupplierLedgerInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSupplierStatementDto>({
      method: 'GET',
      url: '/api/app/shop-supplier-ledger/statement',
      params: { supplierId: input.supplierId, dateFrom: input.dateFrom, dateTo: input.dateTo, referenceType: input.referenceType, filter: input.filter },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
