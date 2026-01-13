import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface ImportStudentAttendanceResultDto {
  studentsInFile: number;
  studentsMatched: number;
  datesInFile: number;
  recordsUpserted: number;
  errors: string[];
}

@Injectable({ providedIn: 'root' })
export class StudentAttendanceImportApi {
  constructor(private rest: RestService) {}

  importFromExcel(file: File): Observable<ImportStudentAttendanceResultDto> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.rest.request<FormData, ImportStudentAttendanceResultDto>(
      {
        method: 'POST',
        url: '/api/app/student-attendance/import-excel',
        body: formData,
      },
      { apiName: 'default' }
    );
  }
}
