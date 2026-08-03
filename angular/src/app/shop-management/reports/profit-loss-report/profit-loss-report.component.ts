import { Component, ElementRef, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { Chart, registerables } from 'chart.js';
import { finalize } from 'rxjs';
import { saveAs } from 'file-saver';
import { ShopProfitLossService } from '../../../proxy/shop-management/profit-loss/shop-profit-loss.service';
import { GetShopProfitLossInput, ShopProfitLossDto } from '../../../proxy/shop-management/profit-loss/models';
import { ShopProfitLossResultStatus } from '../../../proxy/shop-management/profit-loss/shop-profit-loss-result-status.enum';
import { ShopReportPeriod } from '../../../proxy/shop-management/reports/shop-report-period.enum';
import { ShopReportExportFormat } from '../../../proxy/shop-management/reports/shop-report-export-format.enum';
import { ReportTotalCard } from '../../../shared/reports/report-totals-cards.component';

Chart.register(...registerables);

@Component({
  selector: 'app-profit-loss-report',
  standalone: false,
  templateUrl: './profit-loss-report.component.html',
  styleUrl: './profit-loss-report.component.scss',
})
export class ProfitLossReportComponent implements OnInit, OnDestroy {
  private readonly service = inject(ShopProfitLossService);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  @ViewChild('salesVsCogsCanvas') salesVsCogsCanvas?: ElementRef<HTMLCanvasElement>;
  @ViewChild('profitVsExpenseCanvas') profitVsExpenseCanvas?: ElementRef<HTMLCanvasElement>;
  @ViewChild('netProfitTrendCanvas') netProfitTrendCanvas?: ElementRef<HTMLCanvasElement>;
  @ViewChild('expenseCategoryCanvas') expenseCategoryCanvas?: ElementRef<HTMLCanvasElement>;

  private salesVsCogsChart?: Chart;
  private profitVsExpenseChart?: Chart;
  private netProfitTrendChart?: Chart;
  private expenseCategoryChart?: Chart;

  readonly ShopReportPeriod = ShopReportPeriod;
  readonly ShopProfitLossResultStatus = ShopProfitLossResultStatus;

  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.ProfitLoss.ViewCost');
  readonly canViewExpenses = this.permissions.getGrantedPolicy('ShopManagement.ProfitLoss.ViewExpenses');
  readonly canViewMargins = this.permissions.getGrantedPolicy('ShopManagement.ProfitLoss.ViewMargins');
  readonly canViewProductContribution = this.permissions.getGrantedPolicy('ShopManagement.ProfitLoss.ViewProductContribution');
  readonly canExport = this.permissions.getGrantedPolicy('ShopManagement.ProfitLoss.Export');
  readonly canPrint = this.permissions.getGrantedPolicy('ShopManagement.ProfitLoss.Print');

  input: GetShopProfitLossInput = {
    period: ShopReportPeriod.ThisMonth,
    compareWithPreviousPeriod: true,
    includeExpenseBreakdown: true,
    includeProductContribution: true,
    topProductCount: 10,
  };

  result?: ShopProfitLossDto;
  loading = false;
  exporting = false;

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.salesVsCogsChart?.destroy();
    this.profitVsExpenseChart?.destroy();
    this.netProfitTrendChart?.destroy();
    this.expenseCategoryChart?.destroy();
  }

  onFilterChange(): void {
    if (this.input.period === ShopReportPeriod.Custom && (!this.input.dateFrom || !this.input.dateTo)) return;
    this.load();
  }

  load(): void {
    if (this.loading) return;
    if (this.input.period === ShopReportPeriod.Custom && (!this.input.dateFrom || !this.input.dateTo)) return;

    this.loading = true;
    this.service
      .get(this.input)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: r => {
          this.result = r;
          setTimeout(() => this.renderCharts());
        },
        error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
      });
  }

  export(format: ShopReportExportFormat): void {
    this.exporting = true;
    this.service
      .export(this.input, format)
      .pipe(finalize(() => (this.exporting = false)))
      .subscribe({
        next: blob => saveAs(blob, `profit-loss.${this.extensionFor(format)}`),
        error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
      });
  }

  private extensionFor(format: ShopReportExportFormat): string {
    switch (format) {
      case ShopReportExportFormat.Excel: return 'xlsx';
      case ShopReportExportFormat.Csv: return 'csv';
      default: return 'html';
    }
  }

  fmt(value: number | null | undefined): string {
    if (value == null) return '—';
    const symbol = this.result?.summary?.currencySymbol;
    const formatted = value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    return symbol ? `${symbol} ${formatted}` : formatted;
  }

  pct(value: number | null | undefined): string {
    return value == null ? 'N/A' : `${value.toFixed(2)}%`;
  }

  get statusVariant(): 'success' | 'warning' | 'danger' | 'secondary' {
    switch (this.result?.summary?.resultStatus) {
      case ShopProfitLossResultStatus.Profit: return 'success';
      case ShopProfitLossResultStatus.BreakEven: return 'warning';
      case ShopProfitLossResultStatus.Loss: return 'danger';
      default: return 'secondary';
    }
  }

  get statusIcon(): string {
    switch (this.result?.summary?.resultStatus) {
      case ShopProfitLossResultStatus.Profit: return 'fas fa-arrow-trend-up';
      case ShopProfitLossResultStatus.BreakEven: return 'fas fa-equals';
      case ShopProfitLossResultStatus.Loss: return 'fas fa-arrow-trend-down';
      default: return 'fas fa-question';
    }
  }

  get statusLabel(): string {
    switch (this.result?.summary?.resultStatus) {
      case ShopProfitLossResultStatus.Profit: return '::Profit';
      case ShopProfitLossResultStatus.BreakEven: return '::BreakEven';
      case ShopProfitLossResultStatus.Loss: return '::Loss';
      default: return '';
    }
  }

  get totalsCards(): ReportTotalCard[] {
    const s = this.result?.summary;
    if (!s) return [];
    const cards: ReportTotalCard[] = [
      { label: 'Net Sales', value: this.fmt(s.netSales), icon: 'fas fa-coins', colorClass: 'primary' },
    ];
    if (this.canViewCost) {
      cards.push({ label: 'Cost of Goods Sold', value: this.fmt(s.costOfGoodsSold), icon: 'fas fa-dolly', colorClass: 'warning' });
      cards.push({ label: 'Gross Profit', value: this.fmt(s.grossProfit), icon: 'fas fa-chart-line', colorClass: (s.grossProfit ?? 0) >= 0 ? 'success' : 'danger' });
    }
    if (this.canViewExpenses) {
      cards.push({ label: 'Operating Expenses', value: this.fmt(s.operatingExpenses), icon: 'fas fa-file-invoice-dollar', colorClass: 'danger' });
    }
    if (this.canViewCost && this.canViewExpenses) {
      cards.push({ label: 'Net Profit / (Loss)', value: this.fmt(s.netProfit), icon: 'fas fa-balance-scale', colorClass: (s.netProfit ?? 0) >= 0 ? 'success' : 'danger' });
    }
    if (this.canViewMargins) {
      cards.push({ label: 'Gross Margin', value: this.pct(s.grossProfitMarginPercentage), icon: 'fas fa-percent', colorClass: 'info' });
      cards.push({ label: 'Net Margin', value: this.pct(s.netProfitMarginPercentage), icon: 'fas fa-percent', colorClass: 'info' });
    }
    return cards;
  }

  private renderCharts(): void {
    this.renderSalesVsCogs();
    this.renderProfitVsExpense();
    this.renderNetProfitTrend();
    this.renderExpenseCategory();
  }

  private renderSalesVsCogs(): void {
    this.salesVsCogsChart?.destroy();
    this.salesVsCogsChart = undefined;
    const canvas = this.salesVsCogsCanvas?.nativeElement;
    const trend = this.result?.trend;
    if (!canvas || !trend?.length) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    this.salesVsCogsChart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: trend.map(t => t.label),
        datasets: [
          { label: 'Net Sales', data: trend.map(t => t.netSales), backgroundColor: '#2369a3' },
          ...(this.canViewCost ? [{ label: 'COGS', data: trend.map(t => t.costOfGoodsSold ?? 0), backgroundColor: '#c83e4d' }] : []),
        ],
      },
      options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'top' } } },
    });
  }

  private renderProfitVsExpense(): void {
    this.profitVsExpenseChart?.destroy();
    this.profitVsExpenseChart = undefined;
    if (!this.canViewCost || !this.canViewExpenses) return;
    const canvas = this.profitVsExpenseCanvas?.nativeElement;
    const trend = this.result?.trend;
    if (!canvas || !trend?.length) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    this.profitVsExpenseChart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: trend.map(t => t.label),
        datasets: [
          { label: 'Gross Profit', data: trend.map(t => t.grossProfit ?? 0), backgroundColor: '#2f9e44' },
          { label: 'Operating Expenses', data: trend.map(t => t.operatingExpenses ?? 0), backgroundColor: '#e8590c' },
        ],
      },
      options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'top' } } },
    });
  }

  private renderNetProfitTrend(): void {
    this.netProfitTrendChart?.destroy();
    this.netProfitTrendChart = undefined;
    if (!this.canViewCost || !this.canViewExpenses) return;
    const canvas = this.netProfitTrendCanvas?.nativeElement;
    const trend = this.result?.trend;
    if (!canvas || !trend?.length) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    this.netProfitTrendChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: trend.map(t => t.label),
        datasets: [
          { label: 'Net Profit', data: trend.map(t => t.netProfit ?? 0), borderColor: '#2369a3', backgroundColor: 'rgba(35, 105, 163, 0.12)', tension: 0.3, fill: true },
        ],
      },
      options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'top' } } },
    });
  }

  private renderExpenseCategory(): void {
    this.expenseCategoryChart?.destroy();
    this.expenseCategoryChart = undefined;
    const canvas = this.expenseCategoryCanvas?.nativeElement;
    const breakdown = this.result?.expenseBreakdown;
    if (!canvas || !breakdown?.length) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    this.expenseCategoryChart = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: breakdown.map(b => b.expenseCategoryName),
        datasets: [
          {
            data: breakdown.map(b => b.amount),
            backgroundColor: ['#2369a3', '#c83e4d', '#2f9e44', '#e8590c', '#7048e8', '#f08c00', '#087f5b', '#495057'],
          },
        ],
      },
      options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'right' } } },
    });
  }
}
