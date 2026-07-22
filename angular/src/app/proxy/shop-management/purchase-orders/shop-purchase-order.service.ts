import type { CancelShopPurchaseOrderDto, CreateShopPurchaseOrderDto, GetShopPurchaseOrdersInput, RejectShopPurchaseOrderDto, ShopPurchaseOrderDto, UpdateShopPurchaseOrderDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopPurchaseOrderService {
  apiName = 'Default';
  

  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPurchaseOrderDto>({
      method: 'POST',
      url: `/api/app/shop-purchase-order/${id}/approve`,
    },
    { apiName: this.apiName,...config });
  

  cancel = (id: string, input: CancelShopPurchaseOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPurchaseOrderDto>({
      method: 'POST',
      url: `/api/app/shop-purchase-order/${id}/cancel`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateShopPurchaseOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPurchaseOrderDto>({
      method: 'POST',
      url: '/api/app/shop-purchase-order',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-purchase-order/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPurchaseOrderDto>({
      method: 'GET',
      url: `/api/app/shop-purchase-order/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopPurchaseOrdersInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopPurchaseOrderDto>>({
      method: 'GET',
      url: '/api/app/shop-purchase-order',
      params: { filter: input.filter, supplierId: input.supplierId, status: input.status, orderDateFrom: input.orderDateFrom, orderDateTo: input.orderDateTo, expectedDeliveryDateFrom: input.expectedDeliveryDateFrom, expectedDeliveryDateTo: input.expectedDeliveryDateTo, minimumGrandTotal: input.minimumGrandTotal, maximumGrandTotal: input.maximumGrandTotal, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  reject = (id: string, input: RejectShopPurchaseOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPurchaseOrderDto>({
      method: 'POST',
      url: `/api/app/shop-purchase-order/${id}/reject`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  submit = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPurchaseOrderDto>({
      method: 'POST',
      url: `/api/app/shop-purchase-order/${id}/submit`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateShopPurchaseOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPurchaseOrderDto>({
      method: 'PUT',
      url: `/api/app/shop-purchase-order/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
