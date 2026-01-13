import type { GenerateStudentAttendanceTemplateDto, GetAttendanceLeaderboardDto, GetStudentAttendanceListDto, MarkStudentAttendanceDto, StudentAttendanceDto, StudentAttendanceLeaderboardDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StudentAttendanceService {
  apiName = 'Default';
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/student-attendance/${id}`,
    },
    { apiName: this.apiName,...config });
  

  downLoadTemplate = (input: GenerateStudentAttendanceTemplateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/student-attendance/download-excel-template',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentAttendanceDto>({
      method: 'GET',
      url: `/api/app/student-attendance/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getAttendanceLeaderboard = (input: GetAttendanceLeaderboardDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentAttendanceLeaderboardDto>({
      method: 'GET',
      url: '/api/app/student-attendance/attendance-leaderboard',
      params: { gradeLevel: input.gradeLevel, section: input.section, count: input.count, order: input.order, dateFrom: input.dateFrom, dateTo: input.dateTo, id: input.id },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetStudentAttendanceListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StudentAttendanceDto>>({
      method: 'GET',
      url: '/api/app/student-attendance',
      params: { filter: input.filter, studentId: input.studentId, dateFrom: input.dateFrom, dateTo: input.dateTo, status: input.status, firstName: input.firstName, lastName: input.lastName, admissionNo: input.admissionNo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  mark = (input: MarkStudentAttendanceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentAttendanceDto>({
      method: 'POST',
      url: '/api/app/student-attendance',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
