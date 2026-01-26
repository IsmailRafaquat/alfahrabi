import type { GenerateStaffAttendanceTemplateDto, GetStaffAttendanceLeaderboardDto, GetStaffAttendanceListDto, MarkStaffAttendanceDto, StaffAttendanceDto, StaffAttendanceLeaderboardDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StaffAttendanceService {
  apiName = 'Default';
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/staff-attendance/${id}`,
    },
    { apiName: this.apiName,...config });
  

  downLoadTemplate = (input: GenerateStaffAttendanceTemplateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/staff-attendance/download-excel-template',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StaffAttendanceDto>({
      method: 'GET',
      url: `/api/app/staff-attendance/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getAttendanceLeaderboard = (input: GetStaffAttendanceLeaderboardDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StaffAttendanceLeaderboardDto>({
      method: 'GET',
      url: '/api/app/staff-attendance/attendance-leaderboard',
      params: { department: input.department, count: input.count, order: input.order, dateFrom: input.dateFrom, dateTo: input.dateTo, id: input.id },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetStaffAttendanceListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StaffAttendanceDto>>({
      method: 'GET',
      url: '/api/app/staff-attendance',
      params: { filter: input.filter, staffId: input.staffId, dateFrom: input.dateFrom, dateTo: input.dateTo, status: input.status, firstName: input.firstName, lastName: input.lastName, employeeCode: input.employeeCode, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  mark = (input: MarkStaffAttendanceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StaffAttendanceDto>({
      method: 'POST',
      url: '/api/app/staff-attendance',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
