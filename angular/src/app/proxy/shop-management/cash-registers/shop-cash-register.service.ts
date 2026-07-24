import type { CancelShopCashClosingDto, CloseShopCashRegisterDto, CreateManualCashMovementDto, CreateUpdateShopCashRegisterDto, GetShopCashRegistersInput, GetShopCashTransactionsInput, OpenShopCashRegisterDto, ShopCashClosingDto, ShopCashRegisterDto, ShopCashRegisterLookupDto, ShopCashRegisterSummaryDto, ShopCashRegisterTransactionDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopCashRegisterService {
  apiName = 'Default';
  

  cancelClosing = (closingId: string, input: CancelShopCashClosingDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCashClosingDto>({
      method: 'POST',
      url: `/api/app/shop-cash-register/cancel-closing/${closingId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  close = (closingId: string, input: CloseShopCashRegisterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCashClosingDto>({
      method: 'POST',
      url: `/api/app/shop-cash-register/close/${closingId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateShopCashRegisterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCashRegisterDto>({
      method: 'POST',
      url: '/api/app/shop-cash-register',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  createManualMovement = (input: CreateManualCashMovementDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCashRegisterTransactionDto>({
      method: 'POST',
      url: '/api/app/shop-cash-register/manual-movement',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/shop-cash-register/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCashRegisterDto>({
      method: 'GET',
      url: `/api/app/shop-cash-register/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getClosing = (closingId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCashClosingDto>({
      method: 'GET',
      url: `/api/app/shop-cash-register/closing/${closingId}`,
    },
    { apiName: this.apiName,...config });
  

  getClosings = (input: GetShopCashTransactionsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopCashClosingDto>>({
      method: 'GET',
      url: '/api/app/shop-cash-register/closings',
      params: { filter: input.filter, cashRegisterId: input.cashRegisterId, cashClosingId: input.cashClosingId, transactionType: input.transactionType, direction: input.direction, transactionDateFrom: input.transactionDateFrom, transactionDateTo: input.transactionDateTo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetShopCashRegistersInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopCashRegisterDto>>({
      method: 'GET',
      url: '/api/app/shop-cash-register',
      params: { filter: input.filter, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getLookup = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ShopCashRegisterLookupDto>>({
      method: 'GET',
      url: '/api/app/shop-cash-register/lookup',
    },
    { apiName: this.apiName,...config });
  

  getOpenClosing = (cashRegisterId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCashClosingDto>({
      method: 'GET',
      url: `/api/app/shop-cash-register/open-closing/${cashRegisterId}`,
    },
    { apiName: this.apiName,...config });
  

  getSummary = (closingId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCashRegisterSummaryDto>({
      method: 'GET',
      url: `/api/app/shop-cash-register/summary/${closingId}`,
    },
    { apiName: this.apiName,...config });
  

  getTransactions = (input: GetShopCashTransactionsInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ShopCashRegisterTransactionDto>>({
      method: 'GET',
      url: '/api/app/shop-cash-register/transactions',
      params: { filter: input.filter, cashRegisterId: input.cashRegisterId, cashClosingId: input.cashClosingId, transactionType: input.transactionType, direction: input.direction, transactionDateFrom: input.transactionDateFrom, transactionDateTo: input.transactionDateTo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  open = (cashRegisterId: string, input: OpenShopCashRegisterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCashClosingDto>({
      method: 'POST',
      url: `/api/app/shop-cash-register/open/${cashRegisterId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateShopCashRegisterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCashRegisterDto>({
      method: 'PUT',
      url: `/api/app/shop-cash-register/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
