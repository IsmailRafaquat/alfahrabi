
export interface StudentFeeClassReportDto {
  classId: number;
  className?: string;
  expectedAmount: number;
  collectedAmount: number;
  pendingAmount: number;
  students: StudentFeeStudentReportDto[];
}

export interface StudentFeeReportFilterDto {
  monthStart?: string;
  monthEnd?: string;
  classId?: number;
  filter?: string;
  gradeLevel?: number;
}

export interface StudentFeeStudentReportDto {
  studentId?: string;
  studentName?: string;
  admissionNo?: string;
  expectedAmount: number;
  collectedAmount: number;
  pendingAmount: number;
  pendingMonths: string[];
  isPending: boolean;
}
