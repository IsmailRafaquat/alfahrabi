
export interface AttendanceLeaderboardItemDto {
  id?: string;
  code?: string;
  name?: string;
  totalDays: number;
  presentDays: number;
  absentDays: number;
  lateDays: number;
  attendanceRate: number;
}

export interface AttendanceStatusDistributionDto {
  total: number;
  present: number;
  absent: number;
  late: number;
  excused: number;
  sick: number;
  leave: number;
  holiday: number;
  other: number;
}

export interface ClassAttendanceDto {
  gradeLevel: number;
  section: number;
  totalDays: number;
  presentDays: number;
  attendanceRate: number;
}

export interface DashboardDto {
  kpis: DashboardKpisDto;
  expenseTrend: TimePointDto[];
  earnTrend: TimePointDto[];
  staffStatus: AttendanceStatusDistributionDto;
  studentStatus: AttendanceStatusDistributionDto;
  topStaff: AttendanceLeaderboardItemDto[];
  bottomStaff: AttendanceLeaderboardItemDto[];
  studentAttendanceByClass: ClassAttendanceDto[];
  topStudentsOverall: StudentTopDto[];
  expenseTrendByCategory: TimePointByCategoryDto[];
  salaryTrendByStaff: TimePointByNameDto[];
}

export interface DashboardInput {
  month?: string;
  fromDate?: string;
  toDate?: string;
  gradeLevel?: number;
  section?: number;
  shift?: number;
  term?: number;
  department?: number;
}

export interface DashboardKpisDto {
  totalExpense: number;
  totalEarned: number;
  netProfit: number;
  totalTransactions: number;
}

export interface StudentTopDto {
  studentId?: string;
  admissionNo?: string;
  fullName?: string;
  gradeLevel: number;
  section: number;
  totalDays: number;
  presentDays: number;
  attendanceRate: number;
}

export interface TimePointByCategoryDto {
  date?: string;
  categoryName?: string;
  amount: number;
}

export interface TimePointByNameDto {
  date?: string;
  name?: string;
  amount: number;
}

export interface TimePointDto {
  date?: string;
  amount: number;
}
