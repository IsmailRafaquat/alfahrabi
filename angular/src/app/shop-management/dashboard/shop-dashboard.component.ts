import { Component, ElementRef, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { Chart, registerables } from 'chart.js';
import { finalize } from 'rxjs';
import {
  ShopDashboardDto,
  ShopDashboardPeriod,
  ShopDashboardService,
  shopDashboardPeriodOptions,
} from '../../proxy/shop-management/dashboard';
import { ShopGoodsReceiptPaymentStatus } from '../../proxy/shop-management/goods-receipts';
import { ShopSaleStatus } from '../../proxy/shop-management/sales';
import { ShopExpenseStatus } from '../../proxy/shop-management/expenses';
import { ShopProductBatchStatus } from '../../proxy/shop-management/product-batches';

Chart.register(...registerables);

@Component({ selector: 'app-shop-dashboard', standalone: false, templateUrl: './shop-dashboard.component.html', styleUrl: './shop-dashboard.component.scss' })
export class ShopDashboardComponent implements OnInit, OnDestroy {
  private readonly service = inject(ShopDashboardService);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);
  private readonly router = inject(Router);

  @ViewChild('salesExpenseCanvas') salesExpenseCanvas?: ElementRef<HTMLCanvasElement>;
  private chart?: Chart;

  readonly ShopDashboardPeriod = ShopDashboardPeriod;
  readonly ShopGoodsReceiptPaymentStatus = ShopGoodsReceiptPaymentStatus;
  readonly ShopSaleStatus = ShopSaleStatus;
  readonly ShopExpenseStatus = ShopExpenseStatus;
  readonly ShopProductBatchStatus = ShopProductBatchStatus;
  readonly periodOptions = shopDashboardPeriodOptions;

  readonly canViewFinancialSummary = this.permissions.getGrantedPolicy('ShopManagement.Dashboard.ViewFinancialSummary');
  readonly canViewInventoryValue = this.permissions.getGrantedPolicy('ShopManagement.Dashboard.ViewInventoryValue');
  readonly canViewBalances = this.permissions.getGrantedPolicy('ShopManagement.Dashboard.ViewBalances');
  readonly canViewSalesChart = this.permissions.getGrantedPolicy('ShopManagement.Dashboard.ViewSalesChart');
  readonly canViewTopProducts = this.permissions.getGrantedPolicy('ShopManagement.Dashboard.ViewTopProducts');
  readonly canViewStockAlerts = this.permissions.getGrantedPolicy('ShopManagement.Dashboard.ViewStockAlerts');
  readonly canViewRecentTransactions = this.permissions.getGrantedPolicy('ShopManagement.Dashboard.ViewRecentTransactions');

  readonly canCreateSale = this.permissions.getGrantedPolicy('ShopManagement.Sales.Create');
  readonly canCreatePurchaseOrder = this.permissions.getGrantedPolicy('ShopManagement.PurchaseOrders.Create');
  readonly canReceiveGoods = this.permissions.getGrantedPolicy('ShopManagement.GoodsReceipts.Create');
  readonly canCreateExpense = this.permissions.getGrantedPolicy('ShopManagement.Expenses.Create');
  readonly canCreateCustomerPayment = this.permissions.getGrantedPolicy('ShopManagement.CustomerPayments.Create');
  readonly canCreateSupplierPayment = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.Create');
  readonly canCreateStockAdjustment = this.permissions.getGrantedPolicy('ShopManagement.StockAdjustments.Create');
  readonly canCreateStockCount = this.permissions.getGrantedPolicy('ShopManagement.StockCounts.Create');

  readonly hasAnyQuickAction =
    this.canCreateSale || this.canCreatePurchaseOrder || this.canReceiveGoods || this.canCreateExpense ||
    this.canCreateCustomerPayment || this.canCreateSupplierPayment || this.canCreateStockAdjustment || this.canCreateStockCount;

  period: ShopDashboardPeriod = ShopDashboardPeriod.Today;
  dateFrom: string | null = null;
  dateTo: string | null = null;

  loading = false;
  error = false;
  dto?: ShopDashboardDto;

  tooltipLang: 'en' | 'ur' = 'en';

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  onPeriodChange(): void {
    if (this.period !== ShopDashboardPeriod.Custom) this.load();
  }

  applyCustomRange(): void {
    if (this.dateFrom && this.dateTo) this.load();
  }

  load(): void {
    if (this.loading) return;
    if (this.period === ShopDashboardPeriod.Custom && (!this.dateFrom || !this.dateTo)) return;

    this.loading = true;
    this.error = false;

    this.service
      .get({
        period: this.period,
        dateFrom: this.period === ShopDashboardPeriod.Custom ? this.dateFrom! : undefined,
        dateTo: this.period === ShopDashboardPeriod.Custom ? this.dateTo! : undefined,
        topProductsCount: 5,
        recentItemsCount: 5,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: dto => {
          this.dto = dto;
          setTimeout(() => this.renderChart());
        },
        error: e => {
          this.error = true;
          this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError');
        },
      });
  }

  fmt(value: number | null | undefined): string {
    if (value == null) return '—';
    const decimals = this.dto?.decimalPlaces ?? 2;
    const formatted = value.toLocaleString(undefined, { minimumFractionDigits: decimals, maximumFractionDigits: decimals });
    return this.dto?.currencySymbol ? `${this.dto.currencySymbol} ${formatted}` : formatted;
  }

  num(value: number | null | undefined): string {
    if (value == null) return '—';
    return value.toLocaleString(undefined, { minimumFractionDigits: 0, maximumFractionDigits: 4 });
  }

  batchStatusClass(status?: ShopProductBatchStatus): string {
    switch (status) {
      case ShopProductBatchStatus.Active: return 'active';
      case ShopProductBatchStatus.NearExpiry: return 'near-expiry';
      case ShopProductBatchStatus.Expired: return 'expired';
      case ShopProductBatchStatus.Exhausted: return 'exhausted';
      case ShopProductBatchStatus.Blocked: return 'blocked';
      default: return 'active';
    }
  }

  paymentStatusClass(status?: ShopGoodsReceiptPaymentStatus): string {
    switch (status) {
      case ShopGoodsReceiptPaymentStatus.Paid: return 'paid';
      case ShopGoodsReceiptPaymentStatus.PartiallyPaid: return 'partial';
      case ShopGoodsReceiptPaymentStatus.Unpaid: return 'unpaid';
      default: return 'unpaid';
    }
  }

  paymentStatusLabel(status?: ShopGoodsReceiptPaymentStatus): string {
    return status == null ? '' : '::' + ShopGoodsReceiptPaymentStatus[status];
  }

  saleStatusLabel(status?: ShopSaleStatus): string {
    return status == null ? '' : '::' + ShopSaleStatus[status];
  }

  expenseStatusLabel(status?: ShopExpenseStatus): string {
    return status == null ? '' : '::' + ShopExpenseStatus[status];
  }

  batchStatusLabel(status?: ShopProductBatchStatus): string {
    return status == null ? '' : '::' + ShopProductBatchStatus[status];
  }

  goCreateSale(): void { this.router.navigate(['/shop-management/sales/create']); }
  goCreatePurchaseOrder(): void { this.router.navigate(['/shop-management/purchase-orders/create']); }
  goReceiveGoods(): void { this.router.navigate(['/shop-management/purchase-orders']); }
  goCreateExpense(): void { this.router.navigate(['/shop-management/expenses/create']); }
  goCreateCustomerPayment(): void { this.router.navigate(['/shop-management/customer-payments/create']); }
  goCreateSupplierPayment(): void { this.router.navigate(['/shop-management/supplier-payments/create']); }
  goCreateStockAdjustment(): void { this.router.navigate(['/shop-management/stock-adjustments/create']); }
  goCreateStockCount(): void { this.router.navigate(['/shop-management/stock-counts/create']); }

  goSales(): void { this.router.navigate(['/shop-management/sales']); }
  goPurchases(): void { this.router.navigate(['/shop-management/purchase-orders']); }
  goExpenses(): void { this.router.navigate(['/shop-management/expenses']); }
  goCustomerLedger(): void { this.router.navigate(['/shop-management/customers']); }
  goSupplierLedger(): void { this.router.navigate(['/shop-management/suppliers']); }
  goCashManagement(): void { this.router.navigate(['/shop-management/cash-registers']); }
  goBankManagement(): void { this.router.navigate(['/shop-management/bank-accounts']); }
  goProducts(): void { this.router.navigate(['/shop-management/products']); }
  goProductBatches(): void { this.router.navigate(['/shop-management/product-batches']); }

  private renderChart(): void {
    this.chart?.destroy();
    this.chart = undefined;

    const canvas = this.salesExpenseCanvas?.nativeElement;
    const chartData = this.dto?.salesExpenseChart;
    if (!canvas || !chartData) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    this.chart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: chartData.netSalesPoints.map(x => x.label),
        datasets: [
          {
            label: 'Net Sales',
            data: chartData.netSalesPoints.map(x => x.value),
            borderColor: '#2369a3',
            backgroundColor: 'rgba(35, 105, 163, 0.12)',
            tension: 0.3,
            fill: true,
          },
          {
            label: 'Expenses',
            data: chartData.expensePoints.map(x => x.value),
            borderColor: '#c83e4d',
            backgroundColor: 'rgba(200, 62, 77, 0.1)',
            tension: 0.3,
            fill: true,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        interaction: { mode: 'index', intersect: false },
        plugins: {
          legend: { position: 'top' },
          tooltip: {
            callbacks: {
              label: item => `${item.dataset.label}: ${this.fmt(item.parsed.y)}`,
            },
          },
        },
        scales: {
          x: { ticks: { maxRotation: 0 } },
          y: { beginAtZero: true },
        },
      },
    });
  }
}
