import type { StudentDocumentDto, UpdateStudentDocumentDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StudentDocumentService {
  apiName = 'Default';
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/student-documents/${id}`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateStudentDocumentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentDocumentDto>({
      method: 'PUT',
      url: `/api/app/student-documents/update/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
