import type { StaffDocumentDto, UpdateStaffDocumentDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StaffDocumentService {
  apiName = 'Default';
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/staff-documents/${id}`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateStaffDocumentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StaffDocumentDto>({
      method: 'PUT',
      url: `/api/app/staff-documents/update/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
