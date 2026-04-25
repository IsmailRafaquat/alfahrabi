import { PageModule } from '@abp/ng.components/page';
import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { TopbarLayoutModule } from 'src/app/components/topbar-layout/topbar-layout.module';
import {
  StudentFeeReportService,
  StudentFeeReportFilterDto,
  StudentFeeClassReportDto,
} from 'src/app/proxy/reports/student-fee-report';
import { gradeLevelOptions } from 'src/app/proxy/students';
import { SharedModule } from 'src/app/shared/shared.module';

@Component({
  selector: 'app-student-fee-report',
  standalone: true,
  imports: [CommonModule, SharedModule, TopbarLayoutModule, PageModule],
  templateUrl: './student-fee-report.component.html',
  styleUrl: './student-fee-report.component.scss',
})
export class StudentFeeReportComponent implements OnInit {
  private readonly reportService = inject(StudentFeeReportService);

  filters: StudentFeeReportFilterDto = {
    filter: '',
    monthStart: undefined,
    monthEnd: undefined,
  };

  data: StudentFeeClassReportDto[] = [];
  gradeLevels = gradeLevelOptions;
  expanded = new Set<number>();

  ngOnInit(): void {
    const year = new Date().getFullYear();
    this.filters.monthStart = `${year}-01-01` as any;
    this.filters.monthEnd = `${year}-12-01` as any;

    this.load();
  }

  load(): void {
    this.reportService.getList({ ...this.filters } as any).subscribe({
      next: res => {
        this.data = res ?? [];
      },
      error: () => {
        this.data = [];
      },
    });
  }

  resetFilters(): void {
    const year = new Date().getFullYear();

    this.filters = {
      filter: '',
      monthStart: `${year}-01-01` as any,
      monthEnd: `${year}-12-01` as any,
    };

    this.expanded.clear();
    this.load();
  }

  toggle(row: StudentFeeClassReportDto): void {
    const key = row.classId;

    if (this.expanded.has(key)) {
      this.expanded.delete(key);
    } else {
      this.expanded.add(key);
    }
  }

  isExpanded(row: StudentFeeClassReportDto): boolean {
    return this.expanded.has(row.classId);
  }

  isPending(row: { pendingAmount?: number | null }): boolean {
    return (row.pendingAmount ?? 0) > 0;
  }

  trackByClass(_: number, row: StudentFeeClassReportDto): number {
    return row.classId;
  }

  getClassPendingMonths(row: StudentFeeClassReportDto): string {
    const months: string[] = [];

    (row.students ?? []).forEach(student => {
      (student.pendingMonths ?? []).forEach(month => {
        if (!months.includes(month)) {
          months.push(month);
        }
      });
    });

    return months.length ? months.join(', ') : '-';
  }
}
