import type { CancelShopCustomerPaymentDto, CreateUpdateShopCustomerPaymentDto, GetShopCustomerPaymentsInput, ShopCustomerOutstandingSaleDto, ShopCustomerPaymentDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopCustomerPaymentService {
  apiName = 'Default';
  

  cancel = (id: string, input: CancelShopCustomerPaymentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCustomerPaymentDto>({
      method: 'POST',
      url: `/api/app/shop-customer-payment/${id}/cancel`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateShopCustomerPaymentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCustomerPaymentDto>({
      method: 'POST',
      url: '/api/app/shop-customer-payment',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-customer-payment/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCustomerPaymentDto>({
      method: 'GET',
      url: `/api/app/shop-customer-payment/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopCustomerPaymentsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopCustomerPaymentDto>>({
      method: 'GET',
      url: '/api/app/shop-customer-payment',
      params: { filter: input.filter, customerId: input.customerId, paymentType: input.paymentType, paymentMethod: input.paymentMethod, status: input.status, paymentDateFrom: input.paymentDateFrom, paymentDateTo: input.paymentDateTo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getOutstandingSales = (customerId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopCustomerOutstandingSaleDto>>({
      method: 'GET',
      url: `/api/app/shop-customer-payment/outstanding-sales/${customerId}`,
    },
    { apiName: this.apiName,...config });
  

  post = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCustomerPaymentDto>({
      method: 'POST',
      url: `/api/app/shop-customer-payment/${id}`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateShopCustomerPaymentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCustomerPaymentDto>({
      method: 'PUT',
      url: `/api/app/shop-customer-payment/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
