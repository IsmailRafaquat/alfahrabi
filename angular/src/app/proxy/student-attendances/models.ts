import type { GradeLevel } from '../students/grade-level.enum';
import type { Section } from '../students/section.enum';
import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { AttendanceLeaderboardOrder } from '../students/attendance-leaderboard-order.enum';
import type { AttendanceStatus } from '../attendance-statuss/attendance-status.enum';

export interface GenerateStudentAttendanceTemplateDto {
  gradeLevel?: GradeLevel;
  section?: Section;
  dateFrom?: string;
  dateTo?: string;
}

export interface GetAttendanceLeaderboardDto extends EntityDto<string> {
  gradeLevel?: GradeLevel;
  section?: Section;
  count: number;
  order?: AttendanceLeaderboardOrder;
  dateFrom?: string;
  dateTo?: string;
}

export interface GetStudentAttendanceListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  studentId?: string;
  dateFrom?: string;
  dateTo?: string;
  status?: AttendanceStatus;
  firstName?: string;
  lastName?: string;
  admissionNo?: string;
}

export interface MarkStudentAttendanceDto {
  studentId?: string;
  attendanceDate?: string;
  status?: AttendanceStatus;
  remarks?: string;
}

export interface StudentAttendanceDto extends FullAuditedEntityDto<string> {
  studentId?: string;
  attendanceDate?: string;
  status?: AttendanceStatus;
  remarks?: string;
  studentName?: string;
  admissionNo?: string;
}

export interface StudentAttendanceLeaderboardDto {
  dateFrom?: string;
  dateTo?: string;
  totalRecords: number;
  presentRecords: number;
  absentRecords: number;
  lateRecords: number;
  otherRecords: number;
  excusedRecords: number;
  sickRecords: number;
  leaveRecords: number;
  holidayRecords: number;
  items: StudentAttendanceLeaderboardItemDto[];
}

export interface StudentAttendanceLeaderboardItemDto {
  studentId?: string;
  admissionNo?: string;
  fullName?: string;
  totalDays: number;
  presentDays: number;
  absentDays: number;
  lateDays: number;
  excusedDays: number;
  sickDays: number;
  leaveDays: number;
  holidayDays: number;
  attendanceRate: number;
}
