import type { CancelShopPurchaseReturnDto, CreateShopPurchaseReturnDto, GetShopPurchaseReturnsInput, ShopGoodsReceiptReturnableDto, ShopPurchaseReturnDto, UpdateShopPurchaseReturnDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopPurchaseReturnService {
  apiName = 'Default';
  

  cancel = (id: string, i: CancelShopPurchaseReturnDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPurchaseReturnDto>({
      method: 'POST',
      url: `/api/app/shop-purchase-return/${id}/cancel`,
      body: i,
    },
    { apiName: this.apiName,...config });
  

  complete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPurchaseReturnDto>({
      method: 'POST',
      url: `/api/app/shop-purchase-return/${id}/complete`,
    },
    { apiName: this.apiName,...config });
  

  create = (i: CreateShopPurchaseReturnDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPurchaseReturnDto>({
      method: 'POST',
      url: '/api/app/shop-purchase-return',
      body: i,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-purchase-return/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPurchaseReturnDto>({
      method: 'GET',
      url: `/api/app/shop-purchase-return/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getGoodsReceiptForReturn = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopGoodsReceiptReturnableDto>({
      method: 'GET',
      url: `/api/app/shop-purchase-return/${id}/goods-receipt-for-return`,
    },
    { apiName: this.apiName,...config });
  

  getList = (i: GetShopPurchaseReturnsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopPurchaseReturnDto>>({
      method: 'GET',
      url: '/api/app/shop-purchase-return',
      params: { filter: i.filter, supplierId: i.supplierId, goodsReceiptId: i.goodsReceiptId, status: i.status, fromDate: i.fromDate, toDate: i.toDate, sorting: i.sorting, skipCount: i.skipCount, maxResultCount: i.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, i: UpdateShopPurchaseReturnDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPurchaseReturnDto>({
      method: 'PUT',
      url: `/api/app/shop-purchase-return/${id}`,
      body: i,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
