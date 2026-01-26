import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import Chart from 'chart.js/auto';
import { StaffAttendanceLeaderboardDto } from '../proxy/staff-attendances';
import { departmentOptions } from '../proxy/staffs';
import { StaffAttendanceInsightsService } from 'src/custom-services/staff-attendance-insights.service';

@Component({
  selector: 'app-staff-attendance-insights',
  standalone: false,
  templateUrl: './staff-attendance-insights.component.html',
  styleUrls: ['./staff-attendance-insights.component.scss'],
})
export class StaffAttendanceInsightsComponent implements AfterViewInit, OnDestroy {
  loading = false;
  data?: StaffAttendanceLeaderboardDto;

  @ViewChild('barCanvas', { static: true }) barCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('doughnutCanvas', { static: true }) doughnutCanvas!: ElementRef<HTMLCanvasElement>;

  private barChart?: Chart;
  private doughnutChart?: Chart;

  departments = departmentOptions;

  // AttendanceStatus enum values: 1..7 (same as student)
  private readonly statusValues = [1, 2, 3, 4, 5, 6, 7];

  form = this.fb.group({
    department: [this.departments[0]?.value ?? 1, [Validators.required, Validators.min(1)]],
    count: [10, [Validators.required, Validators.min(1), Validators.max(200)]],
    order: [1 as 1 | 2, [Validators.required]],
    dateFrom: [this.toDateInputValue(this.addDays(new Date(), -30)), [Validators.required]],
    dateTo: [this.toDateInputValue(new Date()), [Validators.required]],
  });

  constructor(private fb: FormBuilder, private api: StaffAttendanceInsightsService) {}

  ngAfterViewInit(): void {
    this.initCharts();
  }

  ngOnDestroy(): void {
    this.barChart?.destroy();
    this.doughnutChart?.destroy();
  }

  load(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();

    // Avoid toISOString() to prevent timezone shifting
    const payload = {
      department: raw.department,
      count: raw.count,
      order: raw.order,
      dateFrom: raw.dateFrom ? `${raw.dateFrom}T00:00:00` : null,
      dateTo: raw.dateTo ? `${raw.dateTo}T00:00:00` : null,
    };

    this.loading = true;
    this.data = undefined;

    this.api
      .getLeaderboard(payload as any)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (res) => {
          this.data = res;
          this.updateCharts(res);
        },
      });
  }

  private initCharts(): void {
    this.barChart?.destroy();
    this.doughnutChart?.destroy();

    // BAR
    this.barChart = new Chart(this.barCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels: [],
        datasets: [{ label: 'Attendance Rate (%)', data: [] }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: true },
          tooltip: {
            callbacks: {
              afterBody: (items) => {
                const idx = items?.[0]?.dataIndex ?? -1;
                const row = this.data?.items?.[idx];
                if (!row) return '';

                return [
                  `Present: ${row.presentDays}`,
                  `Absent: ${row.absentDays}`,
                  `Late: ${row.lateDays}`,
                  `Excused: ${(row as any).excusedDays ?? 0}`,
                  `Sick: ${(row as any).sickDays ?? 0}`,
                  `Leave: ${(row as any).leaveDays ?? 0}`,
                  `Holiday: ${(row as any).holidayDays ?? 0}`,
                  `Total: ${row.totalDays}`,
                ];
              },
            },
          },
        },
        scales: { y: { beginAtZero: true, suggestedMax: 100 } },
      },
    });

    // DOUGHNUT
    this.doughnutChart = new Chart(this.doughnutCanvas.nativeElement, {
      type: 'doughnut',
      data: {
        labels: ['Present', 'Absent', 'Late', 'Excused', 'Sick', 'Leave', 'Holiday'],
        datasets: [{ data: new Array(this.statusValues.length).fill(0) }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: true } },
      },
    });
  }

  private updateCharts(res: StaffAttendanceLeaderboardDto): void {
    // Bar
    const labels = (res.items ?? []).map((x: any) => x.fullName);
    const values = (res.items ?? []).map((x: any) => Number((x.attendanceRate ?? 0).toFixed(2)));

    if (this.barChart) {
      this.barChart.data.labels = labels;
      this.barChart.data.datasets[0].data = values;
      this.barChart.update();
    }

    // Doughnut
    const doughnutData = [
      (res as any).presentRecords ?? 0,
      (res as any).absentRecords ?? 0,
      (res as any).lateRecords ?? 0,
      (res as any).excusedRecords ?? 0,
      (res as any).sickRecords ?? 0,
      (res as any).leaveRecords ?? 0,
      (res as any).holidayRecords ?? 0,
    ];

    if (this.doughnutChart) {
      this.doughnutChart.data.datasets[0].data = doughnutData;
      this.doughnutChart.update();
    }
  }

  private toDateInputValue(d: Date): string {
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private addDays(d: Date, days: number): Date {
    const x = new Date(d);
    x.setDate(x.getDate() + days);
    return x;
  }
}
