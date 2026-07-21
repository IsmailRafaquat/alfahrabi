import type { CreateUpdateShopSupplierDto, GetShopSuppliersInput, ShopSupplierDto, ShopSupplierLookupDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopSupplierService {
  apiName = 'Default';
  

  create = (input: CreateUpdateShopSupplierDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSupplierDto>({
      method: 'POST',
      url: '/api/app/shop-supplier',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-supplier/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSupplierDto>({
      method: 'GET',
      url: `/api/app/shop-supplier/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopSuppliersInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopSupplierDto>>({
      method: 'GET',
      url: '/api/app/shop-supplier',
      params: { filter: input.filter, city: input.city, country: input.country, isActive: input.isActive, hasOpeningBalance: input.hasOpeningBalance, hasCreditLimit: input.hasCreditLimit, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getLookup = (filter?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopSupplierLookupDto>>({
      method: 'GET',
      url: '/api/app/shop-supplier/lookup',
      params: { filter },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateShopSupplierDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSupplierDto>({
      method: 'PUT',
      url: `/api/app/shop-supplier/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
