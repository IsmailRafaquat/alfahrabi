import type { CreateUpdateShopExpenseCategoryDto, GetShopExpenseCategoriesInput, ShopExpenseCategoryDto, ShopExpenseCategoryLookupDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopExpenseCategoryService {
  apiName = 'Default';
  

  create = (input: CreateUpdateShopExpenseCategoryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopExpenseCategoryDto>({
      method: 'POST',
      url: '/api/app/shop-expense-category',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-expense-category/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopExpenseCategoryDto>({
      method: 'GET',
      url: `/api/app/shop-expense-category/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopExpenseCategoriesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopExpenseCategoryDto>>({
      method: 'GET',
      url: '/api/app/shop-expense-category',
      params: { filter: input.filter, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getLookup = (filter?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopExpenseCategoryLookupDto>>({
      method: 'GET',
      url: '/api/app/shop-expense-category/lookup',
      params: { filter },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateShopExpenseCategoryDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopExpenseCategoryDto>({
      method: 'PUT',
      url: `/api/app/shop-expense-category/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
