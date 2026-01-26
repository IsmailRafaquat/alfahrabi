import type { CreateStudentDto, GenerateStudentImportTemplateDto, GetStudentListDto, StudentDto, StudentLookupDto, UpdateStudentDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StudentService {
  apiName = 'Default';
  

  create = (input: CreateStudentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentDto>({
      method: 'POST',
      url: '/api/app/students',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/students/${id}`,
    },
    { apiName: this.apiName,...config });
  

  downloadImportTemplate = (input: GenerateStudentImportTemplateDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/students/download-import-template',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentDto>({
      method: 'GET',
      url: `/api/app/students/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetStudentListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<StudentDto>>({
      method: 'GET',
      url: '/api/app/students',
      params: { filter: input.filter, admissionNo: input.admissionNo, firstName: input.firstName, lastName: input.lastName, gradeLevel: input.gradeLevel, section: input.section, term: input.term, shift: input.shift, dob: input.dob, gender: input.gender, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getStudentLookup = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentLookupDto[]>({
      method: 'GET',
      url: '/api/app/students/get-student-lookup-async',
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateStudentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: `/api/app/students/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
