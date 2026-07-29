import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';

interface ShopReportCardItem {
  titleKey: string;
  descriptionKey: string;
  icon: string;
  route: string;
  colorClass: string;
  permission: string;
}

@Component({
  selector: 'app-shop-reports',
  standalone: false,
  templateUrl: './shop-reports.component.html',
  styleUrl: './shop-reports.component.scss',
})
export class ShopReportsComponent {
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);

  readonly reportCards: ShopReportCardItem[] = [
    { titleKey: '::ShopSalesReport', descriptionKey: '::ShopSalesReportDescription', icon: 'fas fa-receipt', route: 'sales', colorClass: 'report-card--success', permission: 'ShopManagement.Reports.Sales' },
    { titleKey: '::ShopPurchaseReport', descriptionKey: '::ShopPurchaseReportDescription', icon: 'fas fa-file-invoice-dollar', route: 'purchase', colorClass: 'report-card--info', permission: 'ShopManagement.Reports.Purchases' },
    { titleKey: '::ShopStockReport', descriptionKey: '::ShopStockReportDescription', icon: 'fas fa-boxes', route: 'stock', colorClass: 'report-card--warning', permission: 'ShopManagement.Reports.Stock' },
    { titleKey: '::ShopStockMovementReport', descriptionKey: '::ShopStockMovementReportDescription', icon: 'fas fa-exchange-alt', route: 'stock-movement', colorClass: 'report-card--primary', permission: 'ShopManagement.Reports.StockMovements' },
    { titleKey: '::ShopBatchExpiryReport', descriptionKey: '::ShopBatchExpiryReportDescription', icon: 'fas fa-hourglass-half', route: 'batch-expiry', colorClass: 'report-card--danger', permission: 'ShopManagement.Reports.BatchExpiry' },
    { titleKey: '::ShopCustomerReceivablesReport', descriptionKey: '::ShopCustomerReceivablesReportDescription', icon: 'fas fa-hand-holding-usd', route: 'customer-receivables', colorClass: 'report-card--success', permission: 'ShopManagement.Reports.CustomerReceivables' },
    { titleKey: '::ShopCustomerTransactionReport', descriptionKey: '::ShopCustomerTransactionReportDescription', icon: 'fas fa-book', route: 'customer-transactions', colorClass: 'report-card--info', permission: 'ShopManagement.Reports.CustomerTransactions' },
    { titleKey: '::ShopSupplierPayablesReport', descriptionKey: '::ShopSupplierPayablesReportDescription', icon: 'fas fa-money-bill-wave', route: 'supplier-payables', colorClass: 'report-card--warning', permission: 'ShopManagement.Reports.SupplierPayables' },
    { titleKey: '::ShopSupplierTransactionReport', descriptionKey: '::ShopSupplierTransactionReportDescription', icon: 'fas fa-book', route: 'supplier-transactions', colorClass: 'report-card--primary', permission: 'ShopManagement.Reports.SupplierTransactions' },
    { titleKey: '::ShopExpenseReport', descriptionKey: '::ShopExpenseReportDescription', icon: 'fas fa-file-invoice', route: 'expenses', colorClass: 'report-card--danger', permission: 'ShopManagement.Reports.Expenses' },
    { titleKey: '::ShopCashReport', descriptionKey: '::ShopCashReportDescription', icon: 'fas fa-cash-register', route: 'cash', colorClass: 'report-card--success', permission: 'ShopManagement.Reports.Cash' },
    { titleKey: '::ShopBankTransactionReport', descriptionKey: '::ShopBankTransactionReportDescription', icon: 'fas fa-university', route: 'bank-transactions', colorClass: 'report-card--info', permission: 'ShopManagement.Reports.Bank' },
    { titleKey: '::ShopTaxSummaryReport', descriptionKey: '::ShopTaxSummaryReportDescription', icon: 'fas fa-percentage', route: 'tax-summary', colorClass: 'report-card--warning', permission: 'ShopManagement.Reports.TaxSummary' },
    { titleKey: '::ShopProductPerformanceReport', descriptionKey: '::ShopProductPerformanceReportDescription', icon: 'fas fa-chart-bar', route: 'product-performance', colorClass: 'report-card--primary', permission: 'ShopManagement.Reports.ProductPerformance' },
  ];

  get visibleReportCards(): ShopReportCardItem[] {
    return this.reportCards.filter(c => this.permissions.getGrantedPolicy(c.permission));
  }

  openReport(route: string) {
    this.router.navigate(['/shop-management/reports', route]);
  }
}
