import type { CreateManualBankMovementDto, GetShopBankTransactionsInput, ShopBankAccountSummaryDto, ShopBankTransactionDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopBankTransactionService {
  apiName = 'Default';
  

  createManualMovement = (input: CreateManualBankMovementDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopBankTransactionDto>({
      method: 'POST',
      url: '/api/app/shop-bank-transaction/manual-movement',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopBankTransactionDto>({
      method: 'GET',
      url: `/api/app/shop-bank-transaction/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopBankTransactionsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopBankTransactionDto>>({
      method: 'GET',
      url: '/api/app/shop-bank-transaction',
      params: { filter: input.filter, bankAccountId: input.bankAccountId, transactionType: input.transactionType, direction: input.direction, referenceType: input.referenceType, referenceNumber: input.referenceNumber, dateFrom: input.dateFrom, dateTo: input.dateTo, minimumAmount: input.minimumAmount, maximumAmount: input.maximumAmount, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getSummary = (bankAccountId: string, dateFrom?: string, dateTo?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopBankAccountSummaryDto>({
      method: 'GET',
      url: `/api/app/shop-bank-transaction/summary/${bankAccountId}`,
      params: { dateFrom, dateTo },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
