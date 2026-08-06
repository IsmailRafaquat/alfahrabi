import type { CancelShopBankTransferDto, CreateUpdateShopBankTransferDto, GetShopBankTransfersInput, ShopBankTransferDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopBankTransferService {
  apiName = 'Default';
  

  cancel = (id: string, input: CancelShopBankTransferDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopBankTransferDto>({
      method: 'POST',
      url: `/api/app/shop-bank-transfer/${id}/cancel`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateShopBankTransferDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopBankTransferDto>({
      method: 'POST',
      url: '/api/app/shop-bank-transfer',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-bank-transfer/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopBankTransferDto>({
      method: 'GET',
      url: `/api/app/shop-bank-transfer/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopBankTransfersInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopBankTransferDto>>({
      method: 'GET',
      url: '/api/app/shop-bank-transfer',
      params: { filter: input.filter, transferType: input.transferType, status: input.status, fromBankAccountId: input.fromBankAccountId, toBankAccountId: input.toBankAccountId, dateFrom: input.dateFrom, dateTo: input.dateTo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  post = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopBankTransferDto>({
      method: 'POST',
      url: `/api/app/shop-bank-transfer/${id}`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateShopBankTransferDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopBankTransferDto>({
      method: 'PUT',
      url: `/api/app/shop-bank-transfer/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
