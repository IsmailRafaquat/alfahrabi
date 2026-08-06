import type { CreateUpdateShopProductCategoryDto, GetShopProductCategoriesInput, ShopProductCategoryDto, ShopProductCategoryLookupDto, ShopProductCategoryTreeDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopProductCategoryService {
  apiName = 'Default';
  

  create = (input: CreateUpdateShopProductCategoryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopProductCategoryDto>({
      method: 'POST',
      url: '/api/app/shop-product-category',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-product-category/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopProductCategoryDto>({
      method: 'GET',
      url: `/api/app/shop-product-category/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopProductCategoriesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopProductCategoryDto>>({
      method: 'GET',
      url: '/api/app/shop-product-category',
      params: { filter: input.filter, parentCategoryId: input.parentCategoryId, rootCategoriesOnly: input.rootCategoriesOnly, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getLookup = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopProductCategoryLookupDto>>({
      method: 'GET',
      url: '/api/app/shop-product-category/lookup',
    },
    { apiName: this.apiName,...config });
  

  getTree = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopProductCategoryTreeDto[]>({
      method: 'GET',
      url: '/api/app/shop-product-category/tree',
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateShopProductCategoryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopProductCategoryDto>({
      method: 'PUT',
      url: `/api/app/shop-product-category/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
