import type { CancelShopStockAdjustmentDto, CreateShopStockAdjustmentDto, GetShopStockAdjustmentsInput, ShopStockAdjustmentDto, ShopStockAdjustmentProductLookupDto, UpdateShopStockAdjustmentDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopStockAdjustmentService {
  apiName = 'Default';
  

  cancel = (id: string, input: CancelShopStockAdjustmentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockAdjustmentDto>({
      method: 'POST',
      url: `/api/app/shop-stock-adjustment/${id}/cancel`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateShopStockAdjustmentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockAdjustmentDto>({
      method: 'POST',
      url: '/api/app/shop-stock-adjustment',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-stock-adjustment/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockAdjustmentDto>({
      method: 'GET',
      url: `/api/app/shop-stock-adjustment/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopStockAdjustmentsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopStockAdjustmentDto>>({
      method: 'GET',
      url: '/api/app/shop-stock-adjustment',
      params: { filter: input.filter, status: input.status, reason: input.reason, adjustmentDateFrom: input.adjustmentDateFrom, adjustmentDateTo: input.adjustmentDateTo, productId: input.productId, adjustmentType: input.adjustmentType, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getProductLookup = (filter?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopStockAdjustmentProductLookupDto>>({
      method: 'GET',
      url: '/api/app/shop-stock-adjustment/product-lookup',
      params: { filter },
    },
    { apiName: this.apiName,...config });
  

  post = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockAdjustmentDto>({
      method: 'POST',
      url: `/api/app/shop-stock-adjustment/${id}`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateShopStockAdjustmentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockAdjustmentDto>({
      method: 'PUT',
      url: `/api/app/shop-stock-adjustment/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
