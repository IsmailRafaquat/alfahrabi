import { RestService } from "@abp/ng.core";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { GetAttendanceLeaderboardDto, StudentAttendanceLeaderboardDto } from "src/app/proxy/student-attendances";

@Injectable({ providedIn: 'root' })
export class StudentAttendanceInsightsService {
  constructor(private rest: RestService) {}

  getLeaderboard(input: GetAttendanceLeaderboardDto): Observable<StudentAttendanceLeaderboardDto> {
    return this.rest.request<any, StudentAttendanceLeaderboardDto>(
      {
        method: 'GET',
        url: '/api/app/student-attendance/attendance-leaderboard',
        params: input,
      },
      { apiName: 'default' }
    );
  }
}