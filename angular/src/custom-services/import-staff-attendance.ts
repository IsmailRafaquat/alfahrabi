import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface ImportStaffAttendanceResultDto {
  staffInFile: number;
  staffMatched: number;
  datesInFile: number;
  recordsUpserted: number;
  errors: string[];
}

@Injectable({ providedIn: 'root' })
export class StaffAttendanceImportApi {
  constructor(private rest: RestService) {}

  importFromExcel(file: File): Observable<ImportStaffAttendanceResultDto> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.rest.request<FormData, ImportStaffAttendanceResultDto>(
      {
        method: 'POST',
        url: '/api/app/staff-attendance/import-excel',
        body: formData,
      },
      { apiName: 'default' }
    );
  }
}
