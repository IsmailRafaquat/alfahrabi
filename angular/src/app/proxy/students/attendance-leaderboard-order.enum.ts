import { mapEnumToOptions } from '@abp/ng.core';

export enum AttendanceLeaderboardOrder {
  Top = 1,
  Bottom = 2,
}

export const attendanceLeaderboardOrderOptions = mapEnumToOptions(AttendanceLeaderboardOrder);
