import type { StudentFeeClassReportDto, StudentFeeReportFilterDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class StudentFeeReportService {
  apiName = 'Default';
  

  getList = (input: StudentFeeReportFilterDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, StudentFeeClassReportDto[]>({
      method: 'GET',
      url: '/api/app/student-fee-report',
      params: { monthStart: input.monthStart, monthEnd: input.monthEnd, classId: input.classId, filter: input.filter, gradeLevel: input.gradeLevel },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
