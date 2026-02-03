import { AfterViewInit, Component, ElementRef, inject, OnDestroy, ViewChild } from '@angular/core';
import Chart from 'chart.js/auto';
import { finalize } from 'rxjs';
import {
  ExpenseCategoryService,
  ExpenseCategoryLookupDto,
} from 'src/app/proxy/expenses/expense-categories';
import {
  ExpensesDashboardService,
  ExpensesDashboardDto,
  GetExpensesDashboardInput,
} from 'src/app/proxy/expenses/expense-dashboards';

@Component({
  selector: 'app-expense-dashboard',
  standalone: false,
  templateUrl: './expense-dashboard.component.html',
  styleUrl: './expense-dashboard.component.scss',
})
export class ExpenseDashboardComponent implements AfterViewInit, OnDestroy {
  private readonly dashboardService = inject(ExpensesDashboardService);
  private readonly categoryService = inject(ExpenseCategoryService);

  loading = false;
  showFilter = false;

  data?: ExpensesDashboardDto;

  // Proxy DTO as UI model (month is UI "YYYY-MM", converted at API call time)
  filters: GetExpensesDashboardInput = {
    filter: null,
    month: null,
    fromDate: null,
    toDate: null,
    expenseCategoryId: null,
    skipCount: 0,
    maxResultCount: 1000,
    sorting: null,
  } as any;

  categoryOptions: ExpenseCategoryLookupDto[] = [];

  @ViewChild('categoryBarCanvas', { static: true })
  categoryBarCanvas!: ElementRef<HTMLCanvasElement>;

  @ViewChild('trendLineCanvas', { static: true })
  trendLineCanvas!: ElementRef<HTMLCanvasElement>;

  private categoryBarChart?: Chart;
  private trendLineChart?: Chart;

  private resizeObs?: ResizeObserver;

  ngAfterViewInit(): void {
    this.filters.month = this.toMonthInputValue(new Date());
    this.loadCategories();

    // IMPORTANT: SPA navigation + ABP layout causes container width to settle after view init.
    // Delay chart init + first data load to avoid "extended length" bug.
    setTimeout(() => {
      this.initCharts();
      this.setupChartResizeObserver();

      setTimeout(() => {
        this.search();
      }, 0);
    }, 0);
  }

  ngOnDestroy(): void {
    this.resizeObs?.disconnect();
    this.categoryBarChart?.destroy();
    this.trendLineChart?.destroy();
  }

  // ---------------- Data ----------------

  private loadCategories(): void {
    this.categoryService.getExpenseCategoryLookup().subscribe({
      next: res => (this.categoryOptions = res ?? []),
      error: () => (this.categoryOptions = []),
    });
  }

  clearFilters(): void {
    this.filters.filter = null;
    this.filters.month = this.toMonthInputValue(new Date());
    this.filters.fromDate = null;
    this.filters.toDate = null;
    this.filters.expenseCategoryId = null;

    this.search();
  }

  search(): void {
    const input = this.buildApiInputFromFilters();

    // reset before new render
    this.resetTrendChart();

    this.loading = true;
    this.dashboardService
      .get(input as any)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: res => {
          this.data = res;
          this.updateCharts(res);

          // ensure correct sizing after data paint
          requestAnimationFrame(() => {
            this.categoryBarChart?.resize();
            this.trendLineChart?.resize();
          });
        },
        error: () => {
          this.data = undefined;
          this.clearCharts();
        },
      });
  }

  /**
   * UI month: "YYYY-MM" (input type="month")
   * API month: "YYYY-MM-01" (first day of month)
   * fromDate/toDate: "YYYY-MM-DD"
   */
  private buildApiInputFromFilters(): GetExpensesDashboardInput {
    const uiMonth = this.trimOrNull(this.filters.month as any);
    const apiMonth = this.normalizeMonthToFirstDay(uiMonth);

    const fromDate = this.normalizeDate(this.filters.fromDate as any);
    const toDate = this.normalizeDate(this.filters.toDate as any);

    // Month wins (predictable)
    const finalFrom = apiMonth ? null : fromDate;
    const finalTo = apiMonth ? null : toDate;

    return {
      filter: this.trimOrNull(this.filters.filter as any),
      expenseCategoryId: (this.filters.expenseCategoryId as any) || null,
      month: apiMonth,
      fromDate: finalFrom,
      toDate: finalTo,
      skipCount: Number(this.filters.skipCount ?? 0),
      maxResultCount: Number(this.filters.maxResultCount ?? 1000),
      sorting: (this.filters.sorting as any) ?? null,
    } as any;
  }

  // ---------------- Charts ----------------

  private initCharts(): void {
    this.categoryBarChart?.destroy();
    this.trendLineChart?.destroy();

    // Bar chart
    const barCtx = this.categoryBarCanvas.nativeElement.getContext('2d');
    if (barCtx) {
      this.categoryBarChart = new Chart(barCtx, {
        type: 'bar',
        data: { labels: [], datasets: [{ label: 'Amount', data: [] }] },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: { display: false },
            tooltip: {
              callbacks: {
                label: ctx => this.toNumber(ctx.raw).toLocaleString(),
              },
            },
          },
          scales: {
            x: { grid: { display: false } },
            y: {
              beginAtZero: true,
              ticks: {
                callback: v => this.toNumber(v).toLocaleString(),
              },
            },
          },
        },
      });
    }

    // Line chart
    const lineCtx = this.trendLineCanvas.nativeElement.getContext('2d');
    if (lineCtx) {
      this.trendLineChart = new Chart(lineCtx, {
        type: 'line',
        data: { labels: [], datasets: [{ label: 'Total', data: [], tension: 0.25, fill: false }] },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: { display: false },
            tooltip: {
              callbacks: {
                label: ctx => this.toNumber(ctx.raw).toLocaleString(),
              },
            },
          },
          scales: {
            // ✅ Prevent "extended length": auto-skip + limited ticks + compact labels (DD)
            x: {
              type: 'category',
              grid: { display: false },
              ticks: {
                autoSkip: true,
                maxTicksLimit: 10,
                maxRotation: 0,
                minRotation: 0,
                callback: (_value, index) => {
                  const lbl = (this.trendLineChart?.data.labels?.[index] ?? '') as string;
                  return typeof lbl === 'string' && lbl.length >= 2 ? lbl.slice(-2) : '';
                },
              },
            },
            y: {
              beginAtZero: true,
              suggestedMax: 1,
              ticks: {
                callback: v => this.toNumber(v).toLocaleString(),
              },
            },
          },
        },
      });
    }

    // after creation, force a resize once layout is stable
    requestAnimationFrame(() => {
      this.categoryBarChart?.resize();
      this.trendLineChart?.resize();
    });
  }

  private setupChartResizeObserver(): void {
    const barHost = this.categoryBarCanvas?.nativeElement?.parentElement;
    const lineHost = this.trendLineCanvas?.nativeElement?.parentElement;

    this.resizeObs?.disconnect();
    this.resizeObs = new ResizeObserver(() => {
      this.categoryBarChart?.resize();
      this.trendLineChart?.resize();
    });

    if (barHost) this.resizeObs.observe(barHost);
    if (lineHost) this.resizeObs.observe(lineHost);

    requestAnimationFrame(() => {
      this.categoryBarChart?.resize();
      this.trendLineChart?.resize();
    });
  }

  private resetTrendChart(): void {
    if (!this.trendLineChart) return;
    this.trendLineChart.data.labels = [];
    this.trendLineChart.data.datasets[0].data = [];
    this.trendLineChart.update('none');
    this.trendLineChart.resize();
  }

  private clearCharts(): void {
    if (this.categoryBarChart) {
      this.categoryBarChart.data.labels = [];
      this.categoryBarChart.data.datasets[0].data = [];
      this.categoryBarChart.update('none');
      this.categoryBarChart.resize();
    }

    if (this.trendLineChart) {
      this.trendLineChart.data.labels = [];
      this.trendLineChart.data.datasets[0].data = [];
      this.trendLineChart.update('none');
      this.trendLineChart.resize();
    }
  }

  private updateCharts(res: ExpensesDashboardDto): void {
    // ---------------- Top Categories (Bar) ----------------
    const categories = (res as any)?.byCategory ?? [];
    const top = categories
      .slice()
      .sort((a: any, b: any) => this.toNumber(b?.amount) - this.toNumber(a?.amount))
      .slice(0, 10);

    const barLabels = top.map((x: any) =>
      String(x?.categoryName ?? '').trim() ? String(x.categoryName) : '—',
    );
    const barValues = top.map((x: any) => this.toNumber(x?.amount));

    if (this.categoryBarChart) {
      this.categoryBarChart.data.labels = barLabels as any;
      this.categoryBarChart.data.datasets[0].data = barValues as any;
      this.categoryBarChart.update('none');
    }

    // ---------------- Trend (Line) ----------------
    const byDay = (res as any)?.byDay ?? [];
    const uiMonth = this.trimOrNull(this.filters.month as any);

    // Helper: trim trailing zeros so the line doesn't look "endless"
    const trimTrailingZeros = (labels: string[], values: number[]) => {
      // find last non-zero
      let lastNonZero = -1;
      for (let i = values.length - 1; i >= 0; i--) {
        if (values[i] !== 0) {
          lastNonZero = i;
          break;
        }
      }

      // show at least N days when we have some data (optional)
      const minDaysToShow = 7;

      if (lastNonZero < 0) {
        // no non-zero data => return empty => UI shows NoData
        return { labels: [] as string[], values: [] as number[] };
      }

      const takeCount = Math.max(lastNonZero + 1, minDaysToShow);
      return {
        labels: labels.slice(0, Math.min(takeCount, labels.length)),
        values: values.slice(0, Math.min(takeCount, values.length)),
      };
    };

    if (uiMonth) {
      const ym = this.parseUiMonth(uiMonth);
      if (!ym) {
        this.resetTrendChart();
        return;
      }

      const totalDays = this.daysInMonth(ym.year, ym.month);

      // Build a map: "YYYY-MM-DD" -> amount (sum if duplicates)
      const map = new Map<string, number>();
      for (const x of byDay) {
        const dt = this.toDate(x?.date);
        if (!dt) continue;
        const key = this.formatDateLabel(dt); // "YYYY-MM-DD"
        const prev = map.get(key) ?? 0;
        map.set(key, prev + this.toNumber(x?.amount));
      }

      // Build full month series (01..last day)
      const labels: string[] = [];
      const values: number[] = [];
      const mm = String(ym.month).padStart(2, '0');

      for (let d = 1; d <= totalDays; d++) {
        const dd = String(d).padStart(2, '0');
        const key = `${ym.year}-${mm}-${dd}`;
        labels.push(key);
        values.push(map.get(key) ?? 0);
      }

      // ✅ Trim trailing zero days (stop the "infinite" flat line)
      const trimmed = trimTrailingZeros(labels, values);

      if (this.trendLineChart) {
        this.trendLineChart.data.labels = trimmed.labels as any;
        this.trendLineChart.data.datasets[0].data = trimmed.values as any;
        this.trendLineChart.update('none');
      }

      return;
    }

    // No month selected => use server points only (sorted)
    const points = byDay
      .map((x: any) => ({
        dt: this.toDate(x?.date),
        amount: this.toNumber(x?.amount),
      }))
      .filter((x: any) => x.dt !== null)
      .sort((a: any, b: any) => a.dt!.getTime() - b.dt!.getTime());

    const labels = points.map((x: any) => this.formatDateLabel(x.dt!));
    const values = points.map((x: any) => x.amount);

    // also trim trailing zeros here (optional but consistent)
    const trimmed = trimTrailingZeros(labels, values);

    if (this.trendLineChart) {
      this.trendLineChart.data.labels = trimmed.labels as any;
      this.trendLineChart.data.datasets[0].data = trimmed.values as any;
      this.trendLineChart.update('none');
    }
  }

  // ---------------- Helpers ----------------

  private trimOrNull(v?: string | null): string | null {
    if (!v) return null;
    const t = String(v).trim();
    return t.length ? t : null;
  }

  private toMonthInputValue(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    return `${y}-${m}`;
  }

  private normalizeMonthToFirstDay(uiMonth?: string | null): string | null {
    const v = this.trimOrNull(uiMonth);
    if (!v) return null;

    const m = /^(\d{4})-(\d{2})$/.exec(v);
    if (!m) return null;

    const yy = Number(m[1]);
    const mm = Number(m[2]);
    if (!Number.isFinite(yy) || !Number.isFinite(mm) || mm < 1 || mm > 12) return null;

    return `${m[1]}-${m[2]}-01`;
  }

  private normalizeDate(v?: string | null): string | null {
    const s = this.trimOrNull(v);
    if (!s) return null;

    const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(s);
    if (!m) return null;

    const yy = Number(m[1]);
    const mm = Number(m[2]);
    const dd = Number(m[3]);

    if (!Number.isFinite(yy) || !Number.isFinite(mm) || !Number.isFinite(dd)) return null;
    if (mm < 1 || mm > 12) return null;
    if (dd < 1 || dd > 31) return null;

    return s;
  }

  private toNumber(v: any): number {
    if (v === null || v === undefined) return 0;
    if (typeof v === 'number') return Number.isFinite(v) ? v : 0;
    const s = String(v).replace(/,/g, '').trim();
    const n = Number(s);
    return Number.isFinite(n) ? n : 0;
  }

  private toDate(v: any): Date | null {
    if (!v) return null;
    if (v instanceof Date) return isNaN(v.getTime()) ? null : v;
    const dt = new Date(String(v));
    return isNaN(dt.getTime()) ? null : dt;
  }

  private formatDateLabel(dt: Date): string {
    const y = dt.getFullYear();
    const m = String(dt.getMonth() + 1).padStart(2, '0');
    const d = String(dt.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  private daysInMonth(year: number, month1to12: number): number {
    return new Date(year, month1to12, 0).getDate();
  }

  private parseUiMonth(uiMonth: string): { year: number; month: number } | null {
    const m = /^(\d{4})-(\d{2})$/.exec(uiMonth);
    if (!m) return null;

    const year = Number(m[1]);
    const month = Number(m[2]);
    if (!Number.isFinite(year) || !Number.isFinite(month) || month < 1 || month > 12) return null;

    return { year, month };
  }
}
