import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import Chart from 'chart.js/auto';

import {
  CheckFeesDashboardDto,
  CheckFeesDashboardInput,
  StudentMonthlyFeeLineService,
} from 'src/app/proxy/fee-module/student-monthly-fee-lines';

import {
  gradeLevelOptions,
  sectionOptions,
  shiftOptions,
  termOptions,
} from 'src/app/proxy/students';

@Component({
  selector: 'app-check-fees-dashboard',
  standalone: false,
  templateUrl: './check-fees-dashboard.component.html',
  styleUrls: ['./check-fees-dashboard.component.scss'],
})
export class CheckFeesDashboardComponent implements AfterViewInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(StudentMonthlyFeeLineService);

  loading = false;
  data?: CheckFeesDashboardDto;

  // Enum options for dropdowns
  gradeLevels = gradeLevelOptions;
  sections = sectionOptions;
  shifts = shiftOptions;
  terms = termOptions;

  @ViewChild('feeHeadBarCanvas', { static: true })
  feeHeadBarCanvas!: ElementRef<HTMLCanvasElement>;

  @ViewChild('collectionDoughnutCanvas', { static: true })
  collectionDoughnutCanvas!: ElementRef<HTMLCanvasElement>;

  private feeHeadBarChart?: Chart;
  private collectionDoughnutChart?: Chart;

  form = this.fb.group({
    month: [this.toMonthInputValue(new Date()), [Validators.required]], // "YYYY-MM"
    asOfDate: [null],
    gradeLevel: [null as number | null],
    section: [null as number | null],
    shift: [null as number | null],
    term: [null as number | null],
    studentId: [null as string | null],
  });

  ngAfterViewInit(): void {
    this.initCharts();
    this.search();
  }

  ngOnDestroy(): void {
    this.feeHeadBarChart?.destroy();
    this.collectionDoughnutChart?.destroy();
  }

  search(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const monthDate = this.parseMonthValue(v.month);
    const asOfDate = this.parseDateValue(v.asOfDate);

    const input: CheckFeesDashboardInput = {
      month: monthDate, // ABP proxy sometimes expects "any" date wire type
      asOfDate: asOfDate,
      gradeLevel: v.gradeLevel,
      section: v.section,
      shift: v.shift,
      term: v.term,
      studentId: v.studentId,
      skipCount: 0,
      maxResultCount: 50,
    } as any;

    this.loading = true;

    this.service
      .getDashboard(input)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: res => {
          this.data = res;
          this.updateCharts(res);
        },
      });
  }

  clear(): void {
    this.form.reset({
      month: this.toMonthInputValue(new Date()),
      asOfDate: null,
      gradeLevel: null,
      section: null,
      shift: null,
      term: null,
      studentId: null,
    });

    this.search();
  }

  // ---------------- Charts ----------------

  private initCharts(): void {
    this.feeHeadBarChart?.destroy();
    this.collectionDoughnutChart?.destroy();

    // ---------- HORIZONTAL BAR (Modern & Professional) ----------
    const barCtx = this.feeHeadBarCanvas.nativeElement.getContext('2d')!;
    
    // Create sophisticated gradients
    const netGradient = this.makeHorizontalGradient(
      barCtx,
      'rgba(99, 102, 241, 0.15)',
      'rgba(99, 102, 241, 0.85)',
    );
    const paidGradient = this.makeHorizontalGradient(
      barCtx,
      'rgba(16, 185, 129, 0.15)',
      'rgba(16, 185, 129, 0.85)',
    );
    const pendingGradient = this.makeHorizontalGradient(
      barCtx,
      'rgba(251, 146, 60, 0.15)',
      'rgba(251, 146, 60, 0.85)',
    );

    this.feeHeadBarChart = new Chart(barCtx, {
      type: 'bar',
      data: {
        labels: [],
        datasets: [
          {
            label: 'Net Amount',
            data: [],
            backgroundColor: netGradient,
            borderColor: 'rgba(99, 102, 241, 1)',
            borderWidth: 1.5,
            borderRadius: 8,
            borderSkipped: false,
            barThickness: 28,
            order: 3,
          },
          {
            label: 'Collected',
            data: [],
            backgroundColor: paidGradient,
            borderColor: 'rgba(16, 185, 129, 1)',
            borderWidth: 1.5,
            borderRadius: 8,
            borderSkipped: false,
            barThickness: 28,
            order: 2,
          },
          {
            label: 'Outstanding',
            data: [],
            backgroundColor: pendingGradient,
            borderColor: 'rgba(251, 146, 60, 1)',
            borderWidth: 1.5,
            borderRadius: 8,
            borderSkipped: false,
            barThickness: 28,
            order: 1,
          },
        ],
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        maintainAspectRatio: false,
        interaction: { mode: 'nearest', intersect: false },
        layout: { padding: { top: 12, left: 8, right: 24, bottom: 8 } },
        plugins: {
          legend: {
            position: 'top',
            align: 'end',
            labels: { 
              usePointStyle: true, 
              boxWidth: 10, 
              boxHeight: 10, 
              padding: 18,
              font: { size: 11, weight: 500 }
            },
          },
          tooltip: {
            padding: 14,
            cornerRadius: 12,
            displayColors: true,
            backgroundColor: 'rgba(15, 23, 42, 0.95)',
            titleColor: 'rgba(255, 255, 255, 0.95)',
            bodyColor: 'rgba(255, 255, 255, 0.85)',
            borderColor: 'rgba(99, 102, 241, 0.3)',
            borderWidth: 1,
            titleFont: { size: 13, weight: 600 },
            bodyFont: { size: 12 },
            callbacks: {
              title: (items) => {
                return items[0]?.label || '';
              },
              label: ctx => {
                const v = Number(ctx.raw ?? 0);
                const total = this.calculateTotalForFeeHead(ctx.dataIndex);
                const percentage = total > 0 ? ((v / total) * 100).toFixed(1) : '0';
                return `${ctx.dataset.label}: ${v.toLocaleString()} (${percentage}%)`;
              },
              afterBody: (items) => {
                if (items.length > 0) {
                  const idx = items[0].dataIndex;
                  const total = this.calculateTotalForFeeHead(idx);
                  return `\nTotal: ${total.toLocaleString()}`;
                }
                return '';
              }
            },
          },
        },
        scales: {
          x: {
            beginAtZero: true,
            grid: { 
              display: true,
              color: 'rgba(148, 163, 184, 0.1)',
              lineWidth: 1,
            },
            border: { display: false },
            ticks: {
              callback: v => {
                const num = Number(v);
                if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
                if (num >= 1000) return (num / 1000).toFixed(0) + 'K';
                return num.toLocaleString();
              },
              font: { size: 10 },
              color: 'rgba(100, 116, 139, 0.8)',
            },
          },
          y: {
            grid: { display: false },
            border: { display: false },
            ticks: {
              autoSkip: false,
              font: { size: 11, weight: 500 },
              color: 'rgba(51, 65, 85, 0.9)',
              padding: 8,
            },
          },
        },
      },
    });

    // ---------- DOUGHNUT (Premium ring + center %) ----------
    const doughnutCtx = this.collectionDoughnutCanvas.nativeElement.getContext('2d')!;
    const centerTextPlugin = this.makeCenterTextPlugin(() => {
      const collected = Number(this.data?.totalPaid ?? 0);
      const pending = Number(this.data?.totalPending ?? 0);
      const total = collected + pending;
      const pct = total > 0 ? Math.round((collected / total) * 100) : 0;
      return { title: `${pct}%`, subtitle: 'Collected' };
    });

    this.collectionDoughnutChart = new Chart(doughnutCtx, {
      type: 'doughnut',
      data: {
        labels: ['Collected', 'Pending'],
        datasets: [
          {
            data: [0, 0],
            backgroundColor: ['rgba(16, 185, 129, 0.85)', 'rgba(251, 146, 60, 0.85)'],
            hoverBackgroundColor: ['rgba(16, 185, 129, 1)', 'rgba(251, 146, 60, 1)'],
            borderWidth: 0,
            spacing: 4,
            borderRadius: 12, // rounded ends
            hoverOffset: 10,
          },
        ],
      },
      plugins: [centerTextPlugin],
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '72%',
        rotation: -90,
        plugins: {
          legend: {
            position: 'bottom',
            labels: { usePointStyle: true, boxWidth: 8, boxHeight: 8, padding: 16 },
          },
          tooltip: {
            padding: 12,
            cornerRadius: 10,
            callbacks: {
              label: ctx => {
                const v = Number(ctx.raw ?? 0);
                return `${ctx.label}: ${v.toLocaleString()}`;
              },
            },
          },
        },
      },
    });
  }

  private updateCharts(res: CheckFeesDashboardDto): void {
    const feeHeads = (res.byFeeHead ?? [])
      .slice()
      .filter(x => Number(x.net ?? 0) > 0 || Number(x.paid ?? 0) > 0 || Number(x.pending ?? 0) > 0)
      .sort((a, b) => Number(b.net ?? 0) - Number(a.net ?? 0))
      .slice(0, 10);

    const labels = feeHeads.map(x => (x.feeHeadName?.trim() ? x.feeHeadName : '—'));
    const net = feeHeads.map(x => Number(x.net ?? 0));
    const paid = feeHeads.map(x => Number(x.paid ?? 0));
    const pending = feeHeads.map(x => Number(x.pending ?? 0));

    if (this.feeHeadBarChart) {
      this.feeHeadBarChart.data.labels = labels;
      // Datasets order: Net, Collected, Outstanding
      this.feeHeadBarChart.data.datasets[0].data = net as any;
      this.feeHeadBarChart.data.datasets[1].data = paid as any;
      this.feeHeadBarChart.data.datasets[2].data = pending as any;
      this.feeHeadBarChart.update();
    }

    const collected = Number(res.totalPaid ?? 0);
    const pend = Number(res.totalPending ?? 0);

    if (this.collectionDoughnutChart) {
      this.collectionDoughnutChart.data.datasets[0].data = [collected, pend];
      this.collectionDoughnutChart.update();
    }
  }

  // ---------------- Helpers ----------------

  private calculateTotalForFeeHead(index: number): number {
    if (!this.feeHeadBarChart) return 0;
    // Return only the Net Amount (first dataset) which is already the total after discount
    // Net = Collected + Outstanding (they are components, not additions)
    const netDataset = this.feeHeadBarChart.data.datasets[0];
    const value = netDataset.data[index];
    return typeof value === 'number' ? value : 0;
  }

  private makeVerticalGradient(ctx: CanvasRenderingContext2D, top: string, bottom: string) {
    const h = ctx.canvas.height || 300;
    const g = ctx.createLinearGradient(0, 0, 0, h);
    g.addColorStop(0, top);
    g.addColorStop(1, bottom);
    return g;
  }

  private makeHorizontalGradient(ctx: CanvasRenderingContext2D, left: string, right: string) {
    const w = ctx.canvas.width || 600;
    const g = ctx.createLinearGradient(0, 0, w, 0);
    g.addColorStop(0, left);
    g.addColorStop(1, right);
    return g;
  }

  private makeCenterTextPlugin(getText: () => { title: string; subtitle?: string }) {
    return {
      id: 'centerText',
      afterDraw(chart: any) {
        const { ctx } = chart;
        const meta = chart.getDatasetMeta(0);
        if (!meta?.data?.length) return;

        const x = meta.data[0].x;
        const y = meta.data[0].y;

        const t = getText();

        ctx.save();
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';

        ctx.font = '600 28px Inter, system-ui, -apple-system, Segoe UI, Roboto, Arial';
        ctx.fillStyle = 'rgba(15,23,42,0.9)';
        ctx.fillText(t.title, x, y - 6);

        if (t.subtitle) {
          ctx.font = '500 12px Inter, system-ui, -apple-system, Segoe UI, Roboto, Arial';
          ctx.fillStyle = 'rgba(100,116,139,0.95)';
          ctx.fillText(t.subtitle, x, y + 18);
        }

        ctx.restore();
      },
    };
  }

  private parseMonthValue(v: string | null | undefined): Date {
    // v is expected: "YYYY-MM"
    if (!v || typeof v !== 'string' || v.length < 7) {
      const now = new Date();
      return new Date(now.getFullYear(), now.getMonth(), 1);
    }

    const parts = v.split('-');
    const yy = Number(parts[0]);
    const mm = Number(parts[1]);

    if (!Number.isFinite(yy) || !Number.isFinite(mm) || mm < 1 || mm > 12) {
      const now = new Date();
      return new Date(now.getFullYear(), now.getMonth(), 1);
    }

    return new Date(yy, mm - 1, 1);
  }

  private toMonthInputValue(d: Date): string {
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    return `${year}-${month}`;
  }

  private parseDateValue(v: any): Date | null {
    // v expected "YYYY-MM-DD" from <input type="date">
    if (!v || typeof v !== 'string' || v.length < 10) return null;

    const [yy, mm, dd] = v.split('-').map(x => Number(x));
    if (!yy || !mm || !dd) return null;

    return new Date(yy, mm - 1, dd);
  }
}