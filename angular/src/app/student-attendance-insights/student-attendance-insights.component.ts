import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import Chart from 'chart.js/auto';

import { StudentAttendanceInsightsService } from 'src/custom-services/student-attendance-insights.service';
import { StudentAttendanceLeaderboardDto } from '../proxy/student-attendances';
import { gradeLevelOptions, sectionOptions } from '../proxy/students';

@Component({
  selector: 'app-student-attendance-insights',
  standalone: false,
  templateUrl: './student-attendance-insights.component.html',
  styleUrls: ['./student-attendance-insights.component.scss'],
})
export class StudentAttendanceInsightsComponent implements AfterViewInit, OnDestroy {
  loading = false;
  data?: StudentAttendanceLeaderboardDto;

  @ViewChild('barCanvas', { static: true }) barCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('doughnutCanvas', { static: true }) doughnutCanvas!: ElementRef<HTMLCanvasElement>;

  private barChart?: Chart;
  private doughnutChart?: Chart;

  gradeLevels = gradeLevelOptions;
  sections = sectionOptions;

  // AttendanceStatus enum values: 1..7
  private readonly statusValues = [1, 2, 3, 4, 5, 6, 7];

  form = this.fb.group({
    gradeLevel: [this.gradeLevels[0]?.value ?? 1, [Validators.required, Validators.min(1)]],
    section: [this.sections[0]?.value ?? 1, [Validators.required, Validators.min(1)]],
    count: [10, [Validators.required, Validators.min(1), Validators.max(200)]],
    order: [1 as 1 | 2, [Validators.required]],
    dateFrom: [this.toDateInputValue(this.addDays(new Date(), -30)), [Validators.required]],
    dateTo: [this.toDateInputValue(new Date()), [Validators.required]],
  });

  constructor(private fb: FormBuilder, private api: StudentAttendanceInsightsService) {}

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

    // IMPORTANT: Avoid toISOString() to prevent date shifting (timezone).
    const payload = {
      gradeLevel: raw.gradeLevel,
      section: raw.section,
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
                  `Excused: ${row.excusedDays}`,
                  `Sick: ${row.sickDays}`,
                  `Leave: ${row.leaveDays}`,
                  `Holiday: ${row.holidayDays}`,
                  `Total: ${row.totalDays}`,
                ];
              },
            },
          },
        },
        scales: { y: { beginAtZero: true, suggestedMax: 100 } },
      },
    });

    // DOUGHNUT (7 statuses)
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

  private updateCharts(res: StudentAttendanceLeaderboardDto): void {
    // Bar
    const labels = (res.items ?? []).map((x) => x.fullName);
    const values = (res.items ?? []).map((x) => Number((x.attendanceRate ?? 0).toFixed(2)));

    if (this.barChart) {
      this.barChart.data.labels = labels;
      this.barChart.data.datasets[0].data = values;
      this.barChart.update();
    }

    // Doughnut (7 statuses)
    const doughnutData = [
      res.presentRecords ?? 0,
      res.absentRecords ?? 0,
      res.lateRecords ?? 0,
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
