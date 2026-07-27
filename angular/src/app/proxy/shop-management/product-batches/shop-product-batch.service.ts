import type { BlockShopProductBatchDto, GetShopProductBatchesInput, ShopBatchAvailabilityDto, ShopBatchSummaryDto, ShopProductBatchDto, ShopProductBatchLookupDto, UpdateShopProductBatchDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { ShopStockTransactionDto } from '../stock-transactions/models';

@Injectable({
  providedIn: 'root',
})
export class ShopProductBatchService {
  apiName = 'Default';
  

  block = (id: string, input: BlockShopProductBatchDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopProductBatchDto>({
      method: 'POST',
      url: `/api/app/shop-product-batch/${id}/block`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  checkAvailability = (productId: string, requiredQuantity: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopBatchAvailabilityDto>({
      method: 'POST',
      url: `/api/app/shop-product-batch/check-availability/${productId}`,
      params: { requiredQuantity },
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopProductBatchDto>({
      method: 'GET',
      url: `/api/app/shop-product-batch/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getAvailableBatches = (productId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopProductBatchLookupDto>>({
      method: 'GET',
      url: `/api/app/shop-product-batch/available-batches/${productId}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopProductBatchesInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopProductBatchDto>>({
      method: 'GET',
      url: '/api/app/shop-product-batch',
      params: { filter: input.filter, productId: input.productId, productCategoryId: input.productCategoryId, supplierId: input.supplierId, status: input.status, batchNumber: input.batchNumber, expiryFrom: input.expiryFrom, expiryTo: input.expiryTo, nearExpiryOnly: input.nearExpiryOnly, expiredOnly: input.expiredOnly, hasAvailableStock: input.hasAvailableStock, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getSummary = (productId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopBatchSummaryDto>({
      method: 'GET',
      url: `/api/app/shop-product-batch/summary/${productId}`,
    },
    { apiName: this.apiName,...config });
  

  getTransactions = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopStockTransactionDto>>({
      method: 'GET',
      url: `/api/app/shop-product-batch/${id}/transactions`,
    },
    { apiName: this.apiName,...config });
  

  refreshStatuses = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/shop-product-batch/refresh-statuses',
    },
    { apiName: this.apiName,...config });
  

  unblock = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopProductBatchDto>({
      method: 'POST',
      url: `/api/app/shop-product-batch/${id}/unblock`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateShopProductBatchDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopProductBatchDto>({
      method: 'PUT',
      url: `/api/app/shop-product-batch/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
