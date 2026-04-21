export interface PeriodFilterLike {
  periodStart?: string | Date | null;
  periodEnd?: string | Date | null;
}

export function normalizeAndValidatePeriod(filters: PeriodFilterLike): void {
  const now = new Date();
  const fallbackStart = `${now.getFullYear()}-01`;
  const fallbackEnd = `${now.getFullYear()}-12`;

  let start = normalizeMonthInput(filters.periodStart) ?? fallbackStart;
  let end = normalizeMonthInput(filters.periodEnd) ?? fallbackEnd;

  if (start > end) {
    [start, end] = [end, start];
  }

  filters.periodStart = start;
  filters.periodEnd = end;
}

export function getPeriodDatesFromFilter(filters: PeriodFilterLike): {
  start: Date;
  end: Date;
} {
  const startMonth = normalizeMonthInput(filters.periodStart);
  const endMonth = normalizeMonthInput(filters.periodEnd);

  if (!startMonth || !endMonth) {
    const now = new Date();
    return {
      start: new Date(now.getFullYear(), 0, 1),
      end: new Date(now.getFullYear(), 11, 1),
    };
  }

  const [startYear, startM] = startMonth.split('-').map(Number);
  const [endYear, endM] = endMonth.split('-').map(Number);

  return {
    start: new Date(startYear, startM - 1, 1),
    end: new Date(endYear, endM - 1, 1),
  };
}

export function buildMonthKeys(start: Date, end: Date): string[] {
  const result: string[] = [];

  const current = new Date(start.getFullYear(), start.getMonth(), 1);
  const last = new Date(end.getFullYear(), end.getMonth(), 1);

  while (current <= last) {
    result.push(toMonthKey(current));
    current.setMonth(current.getMonth() + 1);
  }

  return result;
}

export function formatMonthLabel(monthKey: string): string {
  const [year, month] = monthKey.split('-').map(Number);
  const date = new Date(year, month - 1, 1);

  return date.toLocaleString('en-US', {
    month: 'short',
    year: 'numeric',
  });
}

export function getMonthForecastValue(
  item: { monthColumns?: Array<{ month?: string | Date | null; forecast?: number | null }> },
  monthKey: string
): number {
  if (!item?.monthColumns?.length) {
    return 0;
  }

  const found = item.monthColumns.find(x => {
    const key = normalizeMonthInput(x.month);
    return key === monthKey;
  });

  return found?.forecast ?? 0;
}

function normalizeMonthInput(value: string | Date | null | undefined): string | null {
  if (!value) {
    return null;
  }

  if (value instanceof Date) {
    if (isNaN(value.getTime())) {
      return null;
    }

    return toMonthKey(value);
  }

  const str = String(value).trim();

  if (!str) {
    return null;
  }

  const monthMatch = str.match(/^(\d{4})-(\d{2})$/);
  if (monthMatch) {
    return `${monthMatch[1]}-${monthMatch[2]}`;
  }

  const date = new Date(str);
  if (isNaN(date.getTime())) {
    return null;
  }

  return toMonthKey(date);
}

function toMonthKey(date: Date): string {
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  return `${year}-${month}`;
}