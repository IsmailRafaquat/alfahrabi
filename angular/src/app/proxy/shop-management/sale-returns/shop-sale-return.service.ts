import type { CancelShopSaleReturnDto, CreateShopSaleReturnDto, GetShopSaleReturnsInput, ShopSaleReturnDto, ShopSaleReturnableDto, UpdateShopSaleReturnDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopSaleReturnService {
  apiName = 'Default';
  

  cancel = (id: string, input: CancelShopSaleReturnDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSaleReturnDto>({
      method: 'POST',
      url: `/api/app/shop-sale-return/${id}/cancel`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  complete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSaleReturnDto>({
      method: 'POST',
      url: `/api/app/shop-sale-return/${id}/complete`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateShopSaleReturnDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSaleReturnDto>({
      method: 'POST',
      url: '/api/app/shop-sale-return',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-sale-return/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSaleReturnDto>({
      method: 'GET',
      url: `/api/app/shop-sale-return/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopSaleReturnsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopSaleReturnDto>>({
      method: 'GET',
      url: '/api/app/shop-sale-return',
      params: { filter: input.filter, saleId: input.saleId, customerId: input.customerId, status: input.status, reason: input.reason, settlementType: input.settlementType, returnDateFrom: input.returnDateFrom, returnDateTo: input.returnDateTo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getSaleForReturn = (saleId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSaleReturnableDto>({
      method: 'GET',
      url: `/api/app/shop-sale-return/sale-for-return/${saleId}`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateShopSaleReturnDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSaleReturnDto>({
      method: 'PUT',
      url: `/api/app/shop-sale-return/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
