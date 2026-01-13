import type { CreateSubjectDto, GetSubjectListDto, SubjectDto, UpdateSubjectDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SubjectService {
  apiName = 'Default';
  

  create = (input: CreateSubjectDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SubjectDto>({
      method: 'POST',
      url: '/api/app/subjects',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/subjects/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SubjectDto>({
      method: 'GET',
      url: `/api/app/subjects/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetSubjectListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SubjectDto>>({
      method: 'GET',
      url: '/api/app/subjects',
      params: { filter: input.filter, code: input.code, name: input.name, gradeLevel: input.gradeLevel, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateSubjectDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: `/api/app/subjects/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
