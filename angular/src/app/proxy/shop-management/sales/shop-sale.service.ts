import type { CancelShopSaleDto, CompleteShopSaleDto, CreateShopSaleDto, GetShopSalesInput, ShopSaleDto, ShopSaleProductLookupDto, UpdateShopSaleDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopSaleService {
  apiName = 'Default';
  

  cancel = (id: string, input: CancelShopSaleDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSaleDto>({
      method: 'POST',
      url: `/api/app/shop-sale/${id}/cancel`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  complete = (id: string, input: CompleteShopSaleDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSaleDto>({
      method: 'POST',
      url: `/api/app/shop-sale/${id}/complete`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateShopSaleDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSaleDto>({
      method: 'POST',
      url: '/api/app/shop-sale',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-sale/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSaleDto>({
      method: 'GET',
      url: `/api/app/shop-sale/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopSalesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopSaleDto>>({
      method: 'GET',
      url: '/api/app/shop-sale',
      params: { filter: input.filter, customerId: input.customerId, status: input.status, saleType: input.saleType, saleDateFrom: input.saleDateFrom, saleDateTo: input.saleDateTo, minimumGrandTotal: input.minimumGrandTotal, maximumGrandTotal: input.maximumGrandTotal, hasPendingAmount: input.hasPendingAmount, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getSaleProductLookup = (filter?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopSaleProductLookupDto>>({
      method: 'GET',
      url: '/api/app/shop-sale/sale-product-lookup',
      params: { filter },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateShopSaleDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSaleDto>({
      method: 'PUT',
      url: `/api/app/shop-sale/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
