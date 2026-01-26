import type { CreateUpdateLateFeePolicyDto, GetLateFeePolicyListDto, LateFeePolicyDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LateFeePolicyService {
  apiName = 'Default';
  

  create = (input: CreateUpdateLateFeePolicyDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LateFeePolicyDto>({
      method: 'POST',
      url: '/api/fee-module/late-fee-policies',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/fee-module/late-fee-policies/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LateFeePolicyDto>({
      method: 'GET',
      url: `/api/fee-module/late-fee-policies/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getApplicablePolicy = (gradeLevel: number, section: number, shift: number, term: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LateFeePolicyDto>({
      method: 'GET',
      url: '/api/fee-module/late-fee-policies/applicable',
      params: { gradeLevel, section, shift, term },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetLateFeePolicyListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<LateFeePolicyDto>>({
      method: 'GET',
      url: '/api/fee-module/late-fee-policies',
      params: { filter: input.filter, gradeLevel: input.gradeLevel, section: input.section, shift: input.shift, term: input.term, type: input.type, isActive: input.isActive, isGlobal: input.isGlobal, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateLateFeePolicyDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: `/api/fee-module/late-fee-policies/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
