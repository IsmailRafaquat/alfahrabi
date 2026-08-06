import type { CreateShopProductDto, GetShopProductsInput, ShopProductDto, ShopProductLookupDto, UpdateShopProductDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopProductService {
  apiName = 'Default';
  

  create = (input: CreateShopProductDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopProductDto>({
      method: 'POST',
      url: '/api/app/shop-product',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-product/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopProductDto>({
      method: 'GET',
      url: `/api/app/shop-product/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopProductsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopProductDto>>({
      method: 'GET',
      url: '/api/app/shop-product',
      params: { filter: input.filter, categoryId: input.categoryId, unitId: input.unitId, isActive: input.isActive, isTaxable: input.isTaxable, trackBatch: input.trackBatch, trackExpiry: input.trackExpiry, lowStockOnly: input.lowStockOnly, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getLookup = (filter?: string, categoryId?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopProductLookupDto>>({
      method: 'GET',
      url: '/api/app/shop-product/lookup',
      params: { filter, categoryId },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateShopProductDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopProductDto>({
      method: 'PUT',
      url: `/api/app/shop-product/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
