import type { GetShopDashboardInput, ShopDashboardBalanceSummaryDto, ShopDashboardBatchAlertDto, ShopDashboardDto, ShopDashboardLowStockProductDto, ShopDashboardRecentExpenseDto, ShopDashboardRecentPurchaseDto, ShopDashboardRecentSaleDto, ShopDashboardSalesExpenseChartDto, ShopDashboardSummaryDto, ShopDashboardTopProductDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopDashboardService {
  apiName = 'Default';
  

  get = (input: GetShopDashboardInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopDashboardDto>({
      method: 'GET',
      url: '/api/app/shop-dashboard',
      params: { period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, topProductsCount: input.topProductsCount, recentItemsCount: input.recentItemsCount },
    },
    { apiName: this.apiName,...config });
  

  getBalances = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopDashboardBalanceSummaryDto>({
      method: 'GET',
      url: '/api/app/shop-dashboard/balances',
    },
    { apiName: this.apiName,...config });
  

  getExpiredBatches = (maxResultCount: number = 10, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopDashboardBatchAlertDto>>({
      method: 'GET',
      url: '/api/app/shop-dashboard/expired-batches',
      params: { maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getLowStockProducts = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopDashboardLowStockProductDto>>({
      method: 'GET',
      url: '/api/app/shop-dashboard/low-stock-products',
    },
    { apiName: this.apiName,...config });
  

  getNearExpiryBatches = (maxResultCount: number = 10, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopDashboardBatchAlertDto>>({
      method: 'GET',
      url: '/api/app/shop-dashboard/near-expiry-batches',
      params: { maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getRecentExpenses = (maxResultCount: number = 5, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopDashboardRecentExpenseDto>>({
      method: 'GET',
      url: '/api/app/shop-dashboard/recent-expenses',
      params: { maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getRecentPurchases = (maxResultCount: number = 5, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopDashboardRecentPurchaseDto>>({
      method: 'GET',
      url: '/api/app/shop-dashboard/recent-purchases',
      params: { maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getRecentSales = (maxResultCount: number = 5, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopDashboardRecentSaleDto>>({
      method: 'GET',
      url: '/api/app/shop-dashboard/recent-sales',
      params: { maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getSalesExpenseChart = (input: GetShopDashboardInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopDashboardSalesExpenseChartDto>({
      method: 'GET',
      url: '/api/app/shop-dashboard/sales-expense-chart',
      params: { period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, topProductsCount: input.topProductsCount, recentItemsCount: input.recentItemsCount },
    },
    { apiName: this.apiName,...config });
  

  getSummary = (input: GetShopDashboardInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopDashboardSummaryDto>({
      method: 'GET',
      url: '/api/app/shop-dashboard/summary',
      params: { period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, topProductsCount: input.topProductsCount, recentItemsCount: input.recentItemsCount },
    },
    { apiName: this.apiName,...config });
  

  getTopProducts = (input: GetShopDashboardInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopDashboardTopProductDto>>({
      method: 'GET',
      url: '/api/app/shop-dashboard/top-products',
      params: { period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, topProductsCount: input.topProductsCount, recentItemsCount: input.recentItemsCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
