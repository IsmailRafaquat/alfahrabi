import { mapEnumToOptions } from '@abp/ng.core';

export enum AttendanceStatus {
  Present = 1,
  Absent = 2,
  Late = 3,
  Excused = 4,
  Sick = 5,
  Leave = 6,
  Holiday = 7,
}

export const attendanceStatusOptions = mapEnumToOptions(AttendanceStatus);
