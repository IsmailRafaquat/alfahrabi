import type { GetShopProfitLossInput, ShopProfitLossComparisonDto, ShopProfitLossDto, ShopProfitLossExpenseCategoryDto, ShopProfitLossProductContributionDto, ShopProfitLossSummaryDto, ShopProfitLossTrendPointDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { ShopReportExportFormat } from '../reports/shop-report-export-format.enum';

@Injectable({
  providedIn: 'root',
})
export class ShopProfitLossService {
  apiName = 'Default';
  

  export = (input: GetShopProfitLossInput, format: ShopReportExportFormat, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/shop-profit-loss/export',
      params: { format },
      body: input,
    },
    { apiName: this.apiName,...config });
  

  get = (input: GetShopProfitLossInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopProfitLossDto>({
      method: 'GET',
      url: '/api/app/shop-profit-loss',
      params: { period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, compareWithPreviousPeriod: input.compareWithPreviousPeriod, includeExpenseBreakdown: input.includeExpenseBreakdown, includeProductContribution: input.includeProductContribution, topProductCount: input.topProductCount },
    },
    { apiName: this.apiName,...config });
  

  getComparison = (input: GetShopProfitLossInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopProfitLossComparisonDto>({
      method: 'GET',
      url: '/api/app/shop-profit-loss/comparison',
      params: { period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, compareWithPreviousPeriod: input.compareWithPreviousPeriod, includeExpenseBreakdown: input.includeExpenseBreakdown, includeProductContribution: input.includeProductContribution, topProductCount: input.topProductCount },
    },
    { apiName: this.apiName,...config });
  

  getExpenseBreakdown = (input: GetShopProfitLossInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopProfitLossExpenseCategoryDto>>({
      method: 'GET',
      url: '/api/app/shop-profit-loss/expense-breakdown',
      params: { period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, compareWithPreviousPeriod: input.compareWithPreviousPeriod, includeExpenseBreakdown: input.includeExpenseBreakdown, includeProductContribution: input.includeProductContribution, topProductCount: input.topProductCount },
    },
    { apiName: this.apiName,...config });
  

  getProductContribution = (input: GetShopProfitLossInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopProfitLossProductContributionDto>>({
      method: 'GET',
      url: '/api/app/shop-profit-loss/product-contribution',
      params: { period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, compareWithPreviousPeriod: input.compareWithPreviousPeriod, includeExpenseBreakdown: input.includeExpenseBreakdown, includeProductContribution: input.includeProductContribution, topProductCount: input.topProductCount },
    },
    { apiName: this.apiName,...config });
  

  getSummary = (input: GetShopProfitLossInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopProfitLossSummaryDto>({
      method: 'GET',
      url: '/api/app/shop-profit-loss/summary',
      params: { period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, compareWithPreviousPeriod: input.compareWithPreviousPeriod, includeExpenseBreakdown: input.includeExpenseBreakdown, includeProductContribution: input.includeProductContribution, topProductCount: input.topProductCount },
    },
    { apiName: this.apiName,...config });
  

  getTrend = (input: GetShopProfitLossInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopProfitLossTrendPointDto>>({
      method: 'GET',
      url: '/api/app/shop-profit-loss/trend',
      params: { period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, compareWithPreviousPeriod: input.compareWithPreviousPeriod, includeExpenseBreakdown: input.includeExpenseBreakdown, includeProductContribution: input.includeProductContribution, topProductCount: input.topProductCount },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
