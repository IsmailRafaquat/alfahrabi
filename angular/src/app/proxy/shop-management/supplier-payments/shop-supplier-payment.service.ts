import type { CancelShopSupplierPaymentDto, CreateUpdateShopSupplierPaymentDto, GetShopSupplierPaymentsInput, ShopSupplierOutstandingReceiptDto, ShopSupplierPaymentDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopSupplierPaymentService {
  apiName = 'Default';
  

  cancel = (id: string, input: CancelShopSupplierPaymentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSupplierPaymentDto>({
      method: 'POST',
      url: `/api/app/shop-supplier-payment/${id}/cancel`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateShopSupplierPaymentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSupplierPaymentDto>({
      method: 'POST',
      url: '/api/app/shop-supplier-payment',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-supplier-payment/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSupplierPaymentDto>({
      method: 'GET',
      url: `/api/app/shop-supplier-payment/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopSupplierPaymentsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopSupplierPaymentDto>>({
      method: 'GET',
      url: '/api/app/shop-supplier-payment',
      params: { filter: input.filter, supplierId: input.supplierId, paymentType: input.paymentType, paymentMethod: input.paymentMethod, status: input.status, paymentDateFrom: input.paymentDateFrom, paymentDateTo: input.paymentDateTo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getOutstandingReceipts = (supplierId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSupplierOutstandingReceiptDto[]>({
      method: 'GET',
      url: `/api/app/shop-supplier-payment/outstanding-receipts/${supplierId}`,
    },
    { apiName: this.apiName,...config });
  

  post = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSupplierPaymentDto>({
      method: 'POST',
      url: `/api/app/shop-supplier-payment/${id}`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateShopSupplierPaymentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSupplierPaymentDto>({
      method: 'PUT',
      url: `/api/app/shop-supplier-payment/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
