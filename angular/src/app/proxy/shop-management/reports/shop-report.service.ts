import type { GetShopBankTransactionReportInput, GetShopBatchExpiryReportInput, GetShopCashReportInput, GetShopCustomerReceivablesReportInput, GetShopCustomerTransactionReportInput, GetShopExpenseReportInput, GetShopProductPerformanceReportInput, GetShopPurchaseReportInput, GetShopSalesReportInput, GetShopStockMovementReportInput, GetShopStockReportInput, GetShopSupplierPayablesReportInput, GetShopSupplierTransactionReportInput, GetShopTaxSummaryReportInput, ShopBankTransactionReportResultDto, ShopBatchExpiryReportResultDto, ShopCashReportResultDto, ShopCustomerReceivableReportResultDto, ShopCustomerTransactionReportResultDto, ShopExpenseReportResultDto, ShopProductPerformanceReportResultDto, ShopPurchaseReportResultDto, ShopSalesReportResultDto, ShopStockMovementReportResultDto, ShopStockReportResultDto, ShopSupplierPayableReportResultDto, ShopSupplierTransactionReportResultDto, ShopTaxSummaryReportDto } from './models';
import type { ShopReportExportFormat } from './shop-report-export-format.enum';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ShopReportService {
  apiName = 'Default';
  

  exportBankTransactionReport = (input: GetShopBankTransactionReportInput, format: ShopReportExportFormat, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/shop-report/export-bank-transaction-report',
      params: { format },
      body: input,
    },
    { apiName: this.apiName,...config });
  

  exportBatchExpiryReport = (input: GetShopBatchExpiryReportInput, format: ShopReportExportFormat, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/shop-report/export-batch-expiry-report',
      params: { format },
      body: input,
    },
    { apiName: this.apiName,...config });
  

  exportCashReport = (input: GetShopCashReportInput, format: ShopReportExportFormat, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/shop-report/export-cash-report',
      params: { format },
      body: input,
    },
    { apiName: this.apiName,...config });
  

  exportCustomerReceivablesReport = (input: GetShopCustomerReceivablesReportInput, format: ShopReportExportFormat, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/shop-report/export-customer-receivables-report',
      params: { format },
      body: input,
    },
    { apiName: this.apiName,...config });
  

  exportExpenseReport = (input: GetShopExpenseReportInput, format: ShopReportExportFormat, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/shop-report/export-expense-report',
      params: { format },
      body: input,
    },
    { apiName: this.apiName,...config });
  

  exportProductPerformanceReport = (input: GetShopProductPerformanceReportInput, format: ShopReportExportFormat, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/shop-report/export-product-performance-report',
      params: { format },
      body: input,
    },
    { apiName: this.apiName,...config });
  

  exportPurchaseReport = (input: GetShopPurchaseReportInput, format: ShopReportExportFormat, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/shop-report/export-purchase-report',
      params: { format },
      body: input,
    },
    { apiName: this.apiName,...config });
  

  exportSalesReport = (input: GetShopSalesReportInput, format: ShopReportExportFormat, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/shop-report/export-sales-report',
      params: { format },
      body: input,
    },
    { apiName: this.apiName,...config });
  

  exportStockMovementReport = (input: GetShopStockMovementReportInput, format: ShopReportExportFormat, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/shop-report/export-stock-movement-report',
      params: { format },
      body: input,
    },
    { apiName: this.apiName,...config });
  

  exportStockReport = (input: GetShopStockReportInput, format: ShopReportExportFormat, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/shop-report/export-stock-report',
      params: { format },
      body: input,
    },
    { apiName: this.apiName,...config });
  

  exportSupplierPayablesReport = (input: GetShopSupplierPayablesReportInput, format: ShopReportExportFormat, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'POST',
      responseType: 'blob',
      url: '/api/app/shop-report/export-supplier-payables-report',
      params: { format },
      body: input,
    },
    { apiName: this.apiName,...config });
  

  getBankTransactionReport = (input: GetShopBankTransactionReportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopBankTransactionReportResultDto>({
      method: 'GET',
      url: '/api/app/shop-report/bank-transaction-report',
      params: { bankAccountId: input.bankAccountId, transactionType: input.transactionType, referenceType: input.referenceType, minimumAmount: input.minimumAmount, maximumAmount: input.maximumAmount, period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getBatchExpiryReport = (input: GetShopBatchExpiryReportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopBatchExpiryReportResultDto>({
      method: 'GET',
      url: '/api/app/shop-report/batch-expiry-report',
      params: { productId: input.productId, productCategoryId: input.productCategoryId, supplierId: input.supplierId, batchStatus: input.batchStatus, expiryFrom: input.expiryFrom, expiryTo: input.expiryTo, nearExpiryOnly: input.nearExpiryOnly, expiredOnly: input.expiredOnly, activeOnly: input.activeOnly, hasAvailableStock: input.hasAvailableStock, includeBlocked: input.includeBlocked, period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getCashReport = (input: GetShopCashReportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCashReportResultDto>({
      method: 'GET',
      url: '/api/app/shop-report/cash-report',
      params: { cashRegisterId: input.cashRegisterId, transactionType: input.transactionType, referenceType: input.referenceType, period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getCustomerReceivablesReport = (input: GetShopCustomerReceivablesReportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCustomerReceivableReportResultDto>({
      method: 'GET',
      url: '/api/app/shop-report/customer-receivables-report',
      params: { filter: input.filter, customerId: input.customerId, hasOutstandingBalance: input.hasOutstandingBalance, hasAdvanceBalance: input.hasAdvanceBalance, minimumBalance: input.minimumBalance, maximumBalance: input.maximumBalance, includeInactiveCustomers: input.includeInactiveCustomers, lastTransactionFrom: input.lastTransactionFrom, lastTransactionTo: input.lastTransactionTo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getCustomerTransactionReport = (input: GetShopCustomerTransactionReportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopCustomerTransactionReportResultDto>({
      method: 'GET',
      url: '/api/app/shop-report/customer-transaction-report',
      params: { customerId: input.customerId, transactionType: input.transactionType, referenceNumber: input.referenceNumber, period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getExpenseReport = (input: GetShopExpenseReportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopExpenseReportResultDto>({
      method: 'GET',
      url: '/api/app/shop-report/expense-report',
      params: { expenseCategoryId: input.expenseCategoryId, paymentSource: input.paymentSource, cashRegisterId: input.cashRegisterId, bankAccountId: input.bankAccountId, expenseStatus: input.expenseStatus, minimumAmount: input.minimumAmount, maximumAmount: input.maximumAmount, createdByUserId: input.createdByUserId, groupBy: input.groupBy, period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getProductPerformanceReport = (input: GetShopProductPerformanceReportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopProductPerformanceReportResultDto>({
      method: 'GET',
      url: '/api/app/shop-report/product-performance-report',
      params: { productId: input.productId, productCategoryId: input.productCategoryId, supplierId: input.supplierId, includeInactiveProducts: input.includeInactiveProducts, soldOnly: input.soldOnly, purchasedOnly: input.purchasedOnly, period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getPurchaseReport = (input: GetShopPurchaseReportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopPurchaseReportResultDto>({
      method: 'GET',
      url: '/api/app/shop-report/purchase-report',
      params: { supplierId: input.supplierId, productId: input.productId, productCategoryId: input.productCategoryId, purchaseOrderId: input.purchaseOrderId, goodsReceiptStatus: input.goodsReceiptStatus, paymentStatus: input.paymentStatus, minimumAmount: input.minimumAmount, maximumAmount: input.maximumAmount, hasPendingAmount: input.hasPendingAmount, period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getSalesReport = (input: GetShopSalesReportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSalesReportResultDto>({
      method: 'GET',
      url: '/api/app/shop-report/sales-report',
      params: { customerId: input.customerId, productId: input.productId, productCategoryId: input.productCategoryId, saleStatus: input.saleStatus, paymentStatus: input.paymentStatus, paymentMethod: input.paymentMethod, invoiceNumber: input.invoiceNumber, minimumAmount: input.minimumAmount, maximumAmount: input.maximumAmount, hasPendingAmount: input.hasPendingAmount, createdByUserId: input.createdByUserId, groupBy: input.groupBy, period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getStockMovementReport = (input: GetShopStockMovementReportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockMovementReportResultDto>({
      method: 'GET',
      url: '/api/app/shop-report/stock-movement-report',
      params: { productId: input.productId, productCategoryId: input.productCategoryId, productBatchId: input.productBatchId, transactionType: input.transactionType, referenceType: input.referenceType, referenceNumber: input.referenceNumber, quantityDirection: input.quantityDirection, createdByUserId: input.createdByUserId, period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getStockReport = (input: GetShopStockReportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopStockReportResultDto>({
      method: 'GET',
      url: '/api/app/shop-report/stock-report',
      params: { productId: input.productId, productCategoryId: input.productCategoryId, unitId: input.unitId, supplierId: input.supplierId, stockStatus: input.stockStatus, batchTracked: input.batchTracked, expiryTracked: input.expiryTracked, lowStockOnly: input.lowStockOnly, outOfStockOnly: input.outOfStockOnly, inStockOnly: input.inStockOnly, includeInactiveProducts: input.includeInactiveProducts, minimumStock: input.minimumStock, maximumStock: input.maximumStock, period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getSupplierPayablesReport = (input: GetShopSupplierPayablesReportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSupplierPayableReportResultDto>({
      method: 'GET',
      url: '/api/app/shop-report/supplier-payables-report',
      params: { filter: input.filter, supplierId: input.supplierId, hasOutstandingBalance: input.hasOutstandingBalance, hasAdvanceBalance: input.hasAdvanceBalance, minimumBalance: input.minimumBalance, maximumBalance: input.maximumBalance, includeInactiveSuppliers: input.includeInactiveSuppliers, lastTransactionFrom: input.lastTransactionFrom, lastTransactionTo: input.lastTransactionTo, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getSupplierTransactionReport = (input: GetShopSupplierTransactionReportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopSupplierTransactionReportResultDto>({
      method: 'GET',
      url: '/api/app/shop-report/supplier-transaction-report',
      params: { supplierId: input.supplierId, transactionType: input.transactionType, referenceNumber: input.referenceNumber, period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getTaxSummaryReport = (input: GetShopTaxSummaryReportInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ShopTaxSummaryReportDto>({
      method: 'GET',
      url: '/api/app/shop-report/tax-summary-report',
      params: { period: input.period, dateFrom: input.dateFrom, dateTo: input.dateTo },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
