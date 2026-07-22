import type { CancelShopGoodsReceiptDto, CreateShopGoodsReceiptDto, GetShopGoodsReceiptsInput, ShopGoodsReceiptDto, ShopPurchaseOrderReceivingDto, UpdateShopGoodsReceiptDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopGoodsReceiptService {
  apiName = 'Default';
  

  cancel = (id: string, input: CancelShopGoodsReceiptDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopGoodsReceiptDto>({
      method: 'POST',
      url: `/api/app/shop-goods-receipt/${id}/cancel`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  complete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopGoodsReceiptDto>({
      method: 'POST',
      url: `/api/app/shop-goods-receipt/${id}/complete`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateShopGoodsReceiptDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopGoodsReceiptDto>({
      method: 'POST',
      url: '/api/app/shop-goods-receipt',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-goods-receipt/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopGoodsReceiptDto>({
      method: 'GET',
      url: `/api/app/shop-goods-receipt/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopGoodsReceiptsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopGoodsReceiptDto>>({
      method: 'GET',
      url: '/api/app/shop-goods-receipt',
      params: { filter: input.filter, purchaseOrderId: input.purchaseOrderId, supplierId: input.supplierId, status: input.status, receiptDateFrom: input.receiptDateFrom, receiptDateTo: input.receiptDateTo, minimumGrandTotal: input.minimumGrandTotal, maximumGrandTotal: input.maximumGrandTotal, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getPurchaseOrderForReceiving = (purchaseOrderId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPurchaseOrderReceivingDto>({
      method: 'GET',
      url: `/api/app/shop-goods-receipt/purchase-order-for-receiving/${purchaseOrderId}`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateShopGoodsReceiptDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopGoodsReceiptDto>({
      method: 'PUT',
      url: `/api/app/shop-goods-receipt/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
