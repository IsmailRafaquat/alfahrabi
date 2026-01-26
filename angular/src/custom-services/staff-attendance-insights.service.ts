import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { StaffAttendanceLeaderboardDto } from '../app/proxy/staff-attendances';

@Injectable({ providedIn: 'root' })
export class StaffAttendanceInsightsService {
  constructor(private rest: RestService) {}

  getLeaderboard(input: any): Observable<StaffAttendanceLeaderboardDto> {
    return this.rest.request<any, StaffAttendanceLeaderboardDto>(
      {
        method: 'GET',
        url: '/api/app/staff-attendance/attendance-leaderboard',
        params: input,
      },
      { apiName: 'default' }
    );
  }
}
