import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class StaffAttendanceDownloadService {
  constructor(private rest: RestService) {}

  downloadTemplate(input: any): Observable<Blob> {
    return this.rest.request<any, Blob>(
      {
        method: 'POST',
        url: '/api/app/staff-attendance/download-excel-template', // must match StaffAttendanceController route
        body: input,
        responseType: 'blob',
      },
      { apiName: 'default' }
    );
  }
}
