import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { TopbarLayoutModule } from 'src/app/components/topbar-layout/topbar-layout.module';
import { PageModule } from '@abp/ng.components/page';
import { saveAs } from 'file-saver';

import {
  createDefaultPeriodFilters,
  normalizeAndValidatePeriod,
  getPeriodDatesFromFilter,
  buildMonthKeys,
  formatMonthLabel,
} from '../report.helper';

import { StaffLookupDto, StaffService } from 'src/app/proxy/staffs';

import {
  StaffSalaryReportDto,
  StaffSalaryReportFilterDto,
} from 'src/app/proxy/reports/salary-report';

import { StaffSalaryReportExcelJsExporter } from './salary-report-excel.exporter';
import {
  buildStaffSalaryGroupedData,
  StaffSalaryGroupedRow,
  getStaffSalaryMonthDetails,
  getStaffSalaryMonthTotal,
  getStaffSalaryMaxRows,
  getStaffSalaryOverallMonthTotal,
} from './salary-report.helper';
import { StaffSalaryReportService } from 'src/app/proxy/reports/staff-salary';

@Component({
  selector: 'app-salary-report',
  standalone: true,
  imports: [CommonModule, SharedModule, TopbarLayoutModule, PageModule],
  templateUrl: './salary-report.component.html',
  styleUrl: './salary-report.component.scss',
})
export class SalaryReportComponent implements OnInit {
  private readonly salaryReportService = inject(StaffSalaryReportService);
  private readonly staffService = inject(StaffService);

  filters: StaffSalaryReportFilterDto = createDefaultPeriodFilters<StaffSalaryReportFilterDto>({
    staffIds: [],
  });

  report: StaffSalaryReportDto = {
    rows: [],
    grandTotal: 0,
  } as StaffSalaryReportDto;

  data: StaffSalaryGroupedRow[] = [];
  monthKeys: string[] = [];
  monthLabels: string[] = [];

  staffOptions: StaffLookupDto[] = [];

  ngOnInit(): void {
    this.buildStaffOptions();
    this.load();
  }

  load(): void {
    normalizeAndValidatePeriod(this.filters as any);

    const { start, end } = getPeriodDatesFromFilter(this.filters as any);
    this.monthKeys = buildMonthKeys(start, end);
    this.monthLabels = this.monthKeys.map(formatMonthLabel);

    this.salaryReportService.getList({ ...this.filters } as any).subscribe({
      next: res => {
        this.report = res ?? ({ rows: [], grandTotal: 0 } as StaffSalaryReportDto);
        this.data = buildStaffSalaryGroupedData(this.report.rows ?? []);
      },
      error: () => {
        this.report = { rows: [], grandTotal: 0 } as StaffSalaryReportDto;
        this.data = [];
      },
    });
  }

  resetFilters(): void {
    this.filters = createDefaultPeriodFilters<StaffSalaryReportFilterDto>({
      staffIds: [],
    });

    this.load();
  }

  private buildStaffOptions(): void {
    this.staffService.getStaffLookup().subscribe({
      next: res => {
        this.staffOptions = res ?? [];
      },
      error: () => {
        this.staffOptions = [];
      },
    });
  }

  getMonthDetails(item: StaffSalaryGroupedRow, monthKey: string) {
    return getStaffSalaryMonthDetails(item, monthKey);
  }

  getMonthTotal(item: StaffSalaryGroupedRow, monthKey: string): number {
    return getStaffSalaryMonthTotal(item, monthKey);
  }

  getMaxRows(item: StaffSalaryGroupedRow): number {
    return getStaffSalaryMaxRows(item, this.monthKeys);
  }

  getMonthDetailAt(item: StaffSalaryGroupedRow, monthKey: string, index: number) {
    const details = this.getMonthDetails(item, monthKey);
    return details[index] ?? null;
  }

  getOverallMonthTotal(monthKey: string): number {
    return getStaffSalaryOverallMonthTotal(this.data, monthKey);
  }

  trackByStaff(index: number, item: StaffSalaryGroupedRow): string {
    return String(item.staffId ?? index);
  }

  async exportExcel(): Promise<void> {
    const wb = await StaffSalaryReportExcelJsExporter.buildWorkbook({
      sheetName: 'Staff Salary Report',
      monthKeys: this.monthKeys,
      monthLabels: this.monthLabels,
      data: this.data,
    });

    const bytes = await wb.xlsx.writeBuffer();

    const fileName = `StaffSalaryReport_${this.filters.periodStart}_${this.filters.periodEnd}.xlsx`;

    saveAs(
      new Blob([bytes], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      }),
      fileName,
    );
  }
}
