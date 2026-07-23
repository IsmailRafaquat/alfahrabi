import type { CreateUpdateShopCustomerDto, GetShopCustomersInput, ShopCustomerDto, ShopCustomerLookupDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopCustomerService {
  apiName = 'Default';
  

  create = (input: CreateUpdateShopCustomerDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCustomerDto>({
      method: 'POST',
      url: '/api/app/shop-customer',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-customer/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCustomerDto>({
      method: 'GET',
      url: `/api/app/shop-customer/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopCustomersInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopCustomerDto>>({
      method: 'GET',
      url: '/api/app/shop-customer',
      params: { filter: input.filter, customerType: input.customerType, city: input.city, country: input.country, isActive: input.isActive, isWalkInCustomer: input.isWalkInCustomer, hasOpeningBalance: input.hasOpeningBalance, hasCreditLimit: input.hasCreditLimit, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getLookup = (filter?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopCustomerLookupDto>>({
      method: 'GET',
      url: '/api/app/shop-customer/lookup',
      params: { filter },
    },
    { apiName: this.apiName,...config });
  

  getWalkInCustomer = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCustomerLookupDto>({
      method: 'GET',
      url: '/api/app/shop-customer/walk-in-customer',
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateShopCustomerDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCustomerDto>({
      method: 'PUT',
      url: `/api/app/shop-customer/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
