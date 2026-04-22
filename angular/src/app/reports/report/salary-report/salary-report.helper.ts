import { StaffSalaryReportRowDto } from 'src/app/proxy/reports/salary-report';
import { toMonthKey } from '../report.helper';

export interface StaffSalaryMonthDetail {
  salaryDate?: string | Date | null;
  salaryAmount?: number | null;
}

export interface StaffSalaryMonthColumn {
  month: string;
  details: StaffSalaryMonthDetail[];
}

export interface StaffSalaryGroupedRow {
  staffId?: string;
  staffName?: string;
  monthColumns: StaffSalaryMonthColumn[];
}

export function buildStaffSalaryGroupedData(
  rows: StaffSalaryReportRowDto[]
): StaffSalaryGroupedRow[] {
  const staffMap = new Map<string, StaffSalaryGroupedRow>();

  for (const row of rows ?? []) {
    const staffId = String(row.staffId ?? '');
    const staffName = row.staffName ?? '';
    const monthKey = toMonthKey(row.salaryDate as any);

    if (!staffMap.has(staffId)) {
      staffMap.set(staffId, {
        staffId,
        staffName,
        monthColumns: [],
      });
    }

    const staffEntry = staffMap.get(staffId)!;

    let monthEntry = staffEntry.monthColumns.find(x => x.month === monthKey);

    if (!monthEntry) {
      monthEntry = {
        month: monthKey,
        details: [],
      };
      staffEntry.monthColumns.push(monthEntry);
    }

    monthEntry.details.push({
      salaryDate: row.salaryDate,
      salaryAmount: row.salaryAmount,
    });
  }

  const result = Array.from(staffMap.values());

  result.forEach(staff => {
    staff.monthColumns.forEach(month => {
      month.details.sort((a, b) => {
        const da = new Date(a.salaryDate as any).getTime();
        const db = new Date(b.salaryDate as any).getTime();
        return da - db;
      });
    });

    staff.monthColumns.sort((a, b) => a.month.localeCompare(b.month));
  });

  return result.sort((a, b) => (a.staffName ?? '').localeCompare(b.staffName ?? ''));
}

export function getStaffSalaryMonthDetails(
  item: StaffSalaryGroupedRow,
  monthKey: string
): StaffSalaryMonthDetail[] {
  const month = (item.monthColumns ?? []).find(x => x.month === monthKey);
  return month?.details ?? [];
}

export function getStaffSalaryMonthTotal(
  item: StaffSalaryGroupedRow,
  monthKey: string
): number {
  return getStaffSalaryMonthDetails(item, monthKey).reduce(
    (sum, x) => sum + (x.salaryAmount ?? 0),
    0
  );
}

export function getStaffSalaryMaxRows(
  item: StaffSalaryGroupedRow,
  monthKeys: string[]
): number {
  const lengths = monthKeys.map(mk => getStaffSalaryMonthDetails(item, mk).length);
  const max = Math.max(...lengths, 0);
  return max > 0 ? max : 1;
}

export function getStaffSalaryOverallMonthTotal(
  data: StaffSalaryGroupedRow[],
  monthKey: string
): number {
  return (data ?? []).reduce(
    (sum, item) => sum + getStaffSalaryMonthTotal(item, monthKey),
    0
  );
}