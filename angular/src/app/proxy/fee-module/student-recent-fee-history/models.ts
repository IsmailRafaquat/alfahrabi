
export interface StudentRecentFeeHistoryDto {
  studentId?: string;
  studentName?: string;
  selectedStudentMonthlyFeeId?: string;
  selectedMonth?: string;
  totalBalance: number;
  previousBalance: number;
  months: StudentRecentFeeHistoryMonthDto[];
}

export interface StudentRecentFeeHistoryLineDto {
  studentMonthlyFeeLineId?: string;
  feeHeadId?: string;
  feeHeadName?: string;
  expectedAmount: number;
  discountAmount: number;
  adjustmentAmount: number;
  lateFeeAmount: number;
  netAmount: number;
  paidAmount: number;
  balance: number;
}

export interface StudentRecentFeeHistoryMonthDto {
  studentMonthlyFeeId?: string;
  month?: string;
  expectedAmount: number;
  discountAmount: number;
  adjustmentAmount: number;
  lateFeeAmount: number;
  netAmount: number;
  paidAmount: number;
  balance: number;
  lines: StudentRecentFeeHistoryLineDto[];
}
