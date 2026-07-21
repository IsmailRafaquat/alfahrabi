import type { Department } from '../staffs/department.enum';
import type { Shift } from '../students/shift.enum';
import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { AttendanceLeaderboardOrder } from '../students/attendance-leaderboard-order.enum';
import type { AttendanceStatus } from '../attendance-statuss/attendance-status.enum';

export interface GenerateStaffAttendanceTemplateDto {
  department?: Department;
  shift?: Shift;
  dateFrom?: string;
  dateTo?: string;
}

export interface GetStaffAttendanceLeaderboardDto extends EntityDto<string> {
  department?: Department;
  count: number;
  order?: AttendanceLeaderboardOrder;
  dateFrom?: string;
  dateTo?: string;
}

export interface GetStaffAttendanceListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  staffId?: string;
  dateFrom?: string;
  dateTo?: string;
  status?: AttendanceStatus;
  firstName?: string;
  lastName?: string;
  employeeCode?: string;
}

export interface MarkStaffAttendanceDto {
  staffId?: string;
  attendanceDate?: string;
  status?: AttendanceStatus;
  remarks?: string;
}

export interface StaffAttendanceDto extends FullAuditedEntityDto<string> {
  staffId?: string;
  attendanceDate?: string;
  status?: AttendanceStatus;
  remarks?: string;
  staffName?: string;
  employeeCode?: string;
}

export interface StaffAttendanceLeaderboardDto {
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
  items: StaffAttendanceLeaderboardItemDto[];
}

export interface StaffAttendanceLeaderboardItemDto {
  staffId?: string;
  employeeCode?: string;
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
