import type { CreateUpdateFeeStructureItemDto, FeeStructureItemDto, GetFeeStructureItemListInput } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class FeeStructureItemService {
  apiName = 'Default';
  

  create = (input: CreateUpdateFeeStructureItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeeStructureItemDto>({
      method: 'POST',
      url: '/api/app/fee-structure-items',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/fee-structure-items/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeeStructureItemDto>({
      method: 'GET',
      url: `/api/app/fee-structure-items/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetFeeStructureItemListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<FeeStructureItemDto>>({
      method: 'GET',
      url: '/api/app/fee-structure-items',
      params: { feeStructureId: input.feeStructureId, feeHeadId: input.feeHeadId, isMandatory: input.isMandatory, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateFeeStructureItemDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, FeeStructureItemDto>({
      method: 'PUT',
      url: `/api/app/fee-structure-items/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
