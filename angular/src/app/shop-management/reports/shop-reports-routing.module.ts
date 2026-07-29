import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { permissionGuard } from '@abp/ng.core';
import { ShopReportsComponent } from './shop-reports.component';

const routes: Routes = [
  { path: '', component: ShopReportsComponent, canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Reports' } },
  {
    path: 'sales', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Reports.Sales' },
    loadChildren: () => import('./sales-report/sales-report.module').then(m => m.SalesReportModule),
  },
  {
    path: 'purchase', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Reports.Purchases' },
    loadChildren: () => import('./purchase-report/purchase-report.module').then(m => m.PurchaseReportModule),
  },
  {
    path: 'stock', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Reports.Stock' },
    loadChildren: () => import('./stock-report/stock-report.module').then(m => m.StockReportModule),
  },
  {
    path: 'stock-movement', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Reports.StockMovements' },
    loadChildren: () => import('./stock-movement-report/stock-movement-report.module').then(m => m.StockMovementReportModule),
  },
  {
    path: 'batch-expiry', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Reports.BatchExpiry' },
    loadChildren: () => import('./batch-expiry-report/batch-expiry-report.module').then(m => m.BatchExpiryReportModule),
  },
  {
    path: 'customer-receivables', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Reports.CustomerReceivables' },
    loadChildren: () => import('./customer-receivables-report/customer-receivables-report.module').then(m => m.CustomerReceivablesReportModule),
  },
  {
    path: 'customer-transactions', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Reports.CustomerTransactions' },
    loadChildren: () => import('./customer-transaction-report/customer-transaction-report.module').then(m => m.CustomerTransactionReportModule),
  },
  {
    path: 'supplier-payables', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Reports.SupplierPayables' },
    loadChildren: () => import('./supplier-payables-report/supplier-payables-report.module').then(m => m.SupplierPayablesReportModule),
  },
  {
    path: 'supplier-transactions', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Reports.SupplierTransactions' },
    loadChildren: () => import('./supplier-transaction-report/supplier-transaction-report.module').then(m => m.SupplierTransactionReportModule),
  },
  {
    path: 'expenses', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Reports.Expenses' },
    loadChildren: () => import('./expense-report/expense-report.module').then(m => m.ExpenseReportModule),
  },
  {
    path: 'cash', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Reports.Cash' },
    loadChildren: () => import('./cash-report/cash-report.module').then(m => m.CashReportModule),
  },
  {
    path: 'bank-transactions', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Reports.Bank' },
    loadChildren: () => import('./bank-transaction-report/bank-transaction-report.module').then(m => m.BankTransactionReportModule),
  },
  {
    path: 'tax-summary', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Reports.TaxSummary' },
    loadChildren: () => import('./tax-summary-report/tax-summary-report.module').then(m => m.TaxSummaryReportModule),
  },
  {
    path: 'product-performance', canActivate: [permissionGuard], data: { requiredPolicy: 'ShopManagement.Reports.ProductPerformance' },
    loadChildren: () => import('./product-performance-report/product-performance-report.module').then(m => m.ProductPerformanceReportModule),
  },
];

@NgModule({ imports: [RouterModule.forChild(routes)], exports: [RouterModule] })
export class ShopReportsRoutingModule {}
