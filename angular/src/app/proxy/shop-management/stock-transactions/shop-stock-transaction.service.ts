import type { GetShopStockTransactionsInput, ShopStockTransactionDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopStockTransactionService {
  apiName = 'Default';
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockTransactionDto>({
      method: 'GET',
      url: `/api/app/shop-stock-transaction/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopStockTransactionsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopStockTransactionDto>>({
      method: 'GET',
      url: '/api/app/shop-stock-transaction',
      params: { filter: input.filter, productId: input.productId, transactionType: input.transactionType, referenceType: input.referenceType, dateFrom: input.dateFrom, dateTo: input.dateTo, referenceNumber: input.referenceNumber, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
