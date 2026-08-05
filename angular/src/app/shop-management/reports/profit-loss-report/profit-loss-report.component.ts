import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { saveAs } from 'file-saver';
import { ShopProfitLossService } from '../../../proxy/shop-management/profit-loss/shop-profit-loss.service';
import { GetShopProfitLossInput, ShopProfitLossSummaryDto } from '../../../proxy/shop-management/profit-loss/models';
import { ShopReportPeriod } from '../../../proxy/shop-management/reports/shop-report-period.enum';
import { ShopReportExportFormat } from '../../../proxy/shop-management/reports/shop-report-export-format.enum';

export type ProfitLossStatus = 'profitable' | 'loss' | 'breakEven';

/**
 * Everything below is derived once from the loaded ShopProfitLossSummaryDto (see buildView()) and
 * cached on the component instead of recomputed via template getters, so Angular's change detector
 * never re-runs a percentage/ratio calculation on every check - it just reads plain fields.
 */
export interface ProfitLossView {
  totalSales: number;
  costOfGoodsSold: number;
  totalExpenses: number;
  grossProfit: number;
  netProfit: number;
  grossMarginPct: number;
  expenseRatioPct: number;
  netMarginPct: number;
  status: ProfitLossStatus;
  isZeroData: boolean;
  maxComparisonValue: number;
  salesBarPct: number;
  costOfGoodsSoldBarPct: number;
  expensesBarPct: number;
}

/**
 * Deliberately thin: this page shows only the four top-line Profit & Loss figures (Total Sales,
 * COGS, Total Expenses, Net Profit/Loss) plus a frontend-only breakdown of those same numbers, so it
 * calls ShopProfitLossService.getSummary() - the aggregate-only backend path - exactly once per load.
 * Every ratio/bar/insight below is derived client-side from that one response; none of it triggers a
 * second request.
 */
@Component({
  selector: 'app-profit-loss-report',
  standalone: false,
  templateUrl: './profit-loss-report.component.html',
  styleUrl: './profit-loss-report.component.scss',
})
export class ProfitLossReportComponent implements OnInit {
  private readonly service = inject(ShopProfitLossService);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly ShopReportPeriod = ShopReportPeriod;

  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.ProfitLoss.ViewCost');
  readonly canViewExpenses = this.permissions.getGrantedPolicy('ShopManagement.ProfitLoss.ViewExpenses');
  readonly canExport = this.permissions.getGrantedPolicy('ShopManagement.ProfitLoss.Export');
  readonly canPrint = this.permissions.getGrantedPolicy('ShopManagement.ProfitLoss.Print');

  /** Only meaningful once both cost and expenses are visible - otherwise there's no complete breakdown to show. */
  readonly canViewBreakdown = this.canViewCost && this.canViewExpenses;

  input: GetShopProfitLossInput = {
    period: ShopReportPeriod.ThisMonth,
    compareWithPreviousPeriod: false,
    includeExpenseBreakdown: false,
    includeProductContribution: false,
    topProductCount: 10,
  };

  result?: ShopProfitLossSummaryDto;
  view?: ProfitLossView;
  loading = false;
  exporting = false;
  loadError = false;

  ngOnInit(): void {
    this.load();
  }

  onFilterChange(): void {
    if (this.input.period === ShopReportPeriod.Custom && (!this.input.dateFrom || !this.input.dateTo)) return;
    this.load();
  }

  load(): void {
    if (this.loading) return;
    if (this.input.period === ShopReportPeriod.Custom && (!this.input.dateFrom || !this.input.dateTo)) return;

    this.loading = true;
    this.loadError = false;
    this.service
      .getSummary(this.input)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: r => {
          this.result = r;
          this.view = this.buildView(r);
        },
        error: e => {
          this.loadError = true;
          this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError');
        },
      });
  }

  export(format: ShopReportExportFormat): void {
    if (this.exporting) return;
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
    const symbol = this.result?.currencySymbol;
    const formatted = Math.abs(value).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    return symbol ? `${symbol} ${formatted}` : formatted;
  }

  pct(value: number): string {
    return `${value.toFixed(2)}%`;
  }

  statusLabelKey(status: ProfitLossStatus): string {
    switch (status) {
      case 'profitable': return '::ProfitLossStatusProfitable';
      case 'loss': return '::ProfitLossStatusLoss';
      default: return '::ProfitLossStatusBreakEven';
    }
  }

  insightKey(status: ProfitLossStatus): string {
    switch (status) {
      case 'profitable': return '::ProfitLossInsightProfit';
      case 'loss': return '::ProfitLossInsightLoss';
      default: return '::ProfitLossInsightBreakEven';
    }
  }

  /** grossProfit is frontend-derived (totalSales - costOfGoodsSold); everything else uses the API's own totals. */
  private buildView(s: ShopProfitLossSummaryDto): ProfitLossView {
    const totalSales = s.netSales ?? 0;
    const costOfGoodsSold = this.canViewCost ? (s.costOfGoodsSold ?? 0) : 0;
    const totalExpenses = this.canViewExpenses ? (s.operatingExpenses ?? 0) : 0;
    const grossProfit = totalSales - costOfGoodsSold;
    const netProfit = this.canViewBreakdown ? (s.netProfit ?? grossProfit - totalExpenses) : grossProfit - totalExpenses;

    const status: ProfitLossStatus = netProfit > 0 ? 'profitable' : netProfit < 0 ? 'loss' : 'breakEven';
    const isZeroData = totalSales === 0 && costOfGoodsSold === 0 && totalExpenses === 0;

    const maxComparisonValue = Math.max(totalSales, costOfGoodsSold, totalExpenses, 0);
    const barPct = (value: number) => (maxComparisonValue > 0 ? Math.min(100, Math.round((Math.abs(value) / maxComparisonValue) * 100)) : 0);

    return {
      totalSales,
      costOfGoodsSold,
      totalExpenses,
      grossProfit,
      netProfit,
      grossMarginPct: totalSales !== 0 ? (grossProfit / totalSales) * 100 : 0,
      expenseRatioPct: totalSales !== 0 ? (totalExpenses / totalSales) * 100 : 0,
      netMarginPct: totalSales !== 0 ? (netProfit / totalSales) * 100 : 0,
      status,
      isZeroData,
      maxComparisonValue,
      salesBarPct: barPct(totalSales),
      costOfGoodsSoldBarPct: barPct(costOfGoodsSold),
      expensesBarPct: barPct(totalExpenses),
    };
  }
}
