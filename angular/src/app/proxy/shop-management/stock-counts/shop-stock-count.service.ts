import type { CancelShopStockCountDto, CreateShopStockCountDto, GetShopStockCountsInput, ShopStockCountDto, ShopStockCountPostingPreviewDto, ShopStockCountProductLookupDto, UpdateShopStockCountDto, UpdateShopStockCountItemQuantityDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopStockCountService {
  apiName = 'Default';
  

  cancel = (id: string, input: CancelShopStockCountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockCountDto>({
      method: 'POST',
      url: `/api/app/shop-stock-count/${id}/cancel`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  completeCount = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockCountDto>({
      method: 'POST',
      url: `/api/app/shop-stock-count/${id}/complete-count`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateShopStockCountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockCountDto>({
      method: 'POST',
      url: '/api/app/shop-stock-count',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-stock-count/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockCountDto>({
      method: 'GET',
      url: `/api/app/shop-stock-count/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopStockCountsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopStockCountDto>>({
      method: 'GET',
      url: '/api/app/shop-stock-count',
      params: { filter: input.filter, status: input.status, scope: input.scope, productCategoryId: input.productCategoryId, productId: input.productId, countDateFrom: input.countDateFrom, countDateTo: input.countDateTo, hasDifferences: input.hasDifferences, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getPostingPreview = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockCountPostingPreviewDto>({
      method: 'GET',
      url: `/api/app/shop-stock-count/${id}/posting-preview`,
    },
    { apiName: this.apiName,...config });
  

  getProductLookup = (filter?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopStockCountProductLookupDto>>({
      method: 'GET',
      url: '/api/app/shop-stock-count/product-lookup',
      params: { filter },
    },
    { apiName: this.apiName,...config });
  

  post = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockCountDto>({
      method: 'POST',
      url: `/api/app/shop-stock-count/${id}`,
    },
    { apiName: this.apiName,...config });
  

  start = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockCountDto>({
      method: 'POST',
      url: `/api/app/shop-stock-count/${id}/start`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateShopStockCountDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockCountDto>({
      method: 'PUT',
      url: `/api/app/shop-stock-count/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateItemQuantity = (id: string, input: UpdateShopStockCountItemQuantityDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockCountDto>({
      method: 'PUT',
      url: `/api/app/shop-stock-count/${id}/item-quantity`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
