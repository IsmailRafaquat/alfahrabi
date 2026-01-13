import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class StudentAttendanceDownloadService {
  constructor(private rest: RestService) {}

  downloadTemplate(input: any): Observable<Blob> {
    return this.rest.request<any, Blob>(
      {
        method: 'POST',
        url: '/api/app/student-attendance/download-excel-template', // must match your controller route
        body: input,
        responseType: 'blob',
      },
      { apiName: 'default' }
    );
  }
}
