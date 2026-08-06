
export interface StaffSalaryReportDto {
  rows: StaffSalaryReportRowDto[];
  grandTotal: number;
}

export interface StaffSalaryReportFilterDto {
  periodStart?: string;
  periodEnd?: string;
  filter?: string;
  staffIds: string[];
}

export interface StaffSalaryReportRowDto {
  staffId?: string;
  staffName?: string;
  salaryDate?: string;
  salaryAmount: number;
}
