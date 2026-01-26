import type { CreateUpdateFeeHeadDto, FeeHeadDto, FeeHeadLookupDto, GetFeeHeadListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class FeeHeadService {
  apiName = 'Default';
  

  create = (input: CreateUpdateFeeHeadDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeeHeadDto>({
      method: 'POST',
      url: '/api/app/fee-heads',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/fee-heads/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeeHeadDto>({
      method: 'GET',
      url: `/api/app/fee-heads/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getFeeLookup = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeeHeadLookupDto[]>({
      method: 'GET',
      url: '/api/app/fee-heads/get-fee-lookup',
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetFeeHeadListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<FeeHeadDto>>({
      method: 'GET',
      url: '/api/app/fee-heads',
      params: { filter: input.filter, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  setActive = (id: string, isActive: boolean, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: `/api/app/fee-heads/${id}/active`,
      body: isActive,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateFeeHeadDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeeHeadDto>({
      method: 'PUT',
      url: `/api/app/fee-heads/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
