import type { CreateUpdateFeeStructureDto, FeeStructureDto, FeeStructureLookupDto, GetFeeStructureListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class FeeStructureService {
  apiName = 'Default';
  

  create = (input: CreateUpdateFeeStructureDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeeStructureDto>({
      method: 'POST',
      url: '/api/app/fee-structures',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/fee-structures/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeeStructureDto>({
      method: 'GET',
      url: `/api/app/fee-structures/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getFeeStructureLookup = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeeStructureLookupDto[]>({
      method: 'GET',
      url: '/api/app/fee-structures/fee-structure-lookup',
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetFeeStructureListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<FeeStructureDto>>({
      method: 'GET',
      url: '/api/app/fee-structures',
      params: { gradeLevel: input.gradeLevel, shift: input.shift, term: input.term, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  setActive = (id: string, isActive: boolean, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: `/api/app/fee-structures/${id}/active`,
      body: isActive,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateFeeStructureDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeeStructureDto>({
      method: 'PUT',
      url: `/api/app/fee-structures/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
