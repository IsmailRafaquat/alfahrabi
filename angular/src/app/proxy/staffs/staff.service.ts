import type { CreateStaffDto, GetStaffListDto, StaffDto, StaffLookupDto, UpdateStaffDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StaffService {
  apiName = 'Default';
  

  create = (input: CreateStaffDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StaffDto>({
      method: 'POST',
      url: '/api/app/staffs',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/staffs/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StaffDto>({
      method: 'GET',
      url: `/api/app/staffs/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetStaffListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StaffDto>>({
      method: 'GET',
      url: '/api/app/staffs',
      params: { filter: input.filter, firstName: input.firstName, lastName: input.lastName, department: input.department, jobStatus: input.jobStatus, shift: input.shift, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getStaffLookup = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, StaffLookupDto[]>({
      method: 'GET',
      url: '/api/app/staffs/get-staff-lookup-async',
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateStaffDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: `/api/app/staffs/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
