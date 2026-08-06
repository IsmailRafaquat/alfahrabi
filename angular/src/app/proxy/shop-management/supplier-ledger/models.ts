import type { ShopSupplierLedgerReferenceType } from './shop-supplier-ledger-reference-type.enum';

export interface GetShopSupplierLedgerInput {
  supplierId: string;
  dateFrom?: string;
  dateTo?: string;
  referenceType?: ShopSupplierLedgerReferenceType;
  filter?: string;
}

export interface ShopSupplierBalanceSummaryDto {
  supplierId?: string;
  supplierCode?: string;
  supplierName?: string;
  openingBalance?: number;
  totalCompletedPurchases?: number;
  totalCompletedReturns?: number;
  totalPostedPayments?: number;
  currentBalance?: number;
  payableAmount?: number;
  advanceAmount?: number;
  lastTransactionDate?: string;
}

export interface ShopSupplierLedgerDto {
  supplierId?: string;
  supplierCode?: string;
  supplierName?: string;
  dateFrom?: string;
  dateTo?: string;
  openingBalance?: number;
  totalDebit?: number;
  totalCredit?: number;
  closingBalance?: number;
  payableAmount?: number;
  advanceAmount?: number;
  totalPurchases?: number;
  totalPayments?: number;
  totalReturns?: number;
  entries: ShopSupplierLedgerEntryDto[];
}

export interface ShopSupplierLedgerEntryDto {
  transactionDate?: string;
  referenceType?: ShopSupplierLedgerReferenceType;
  referenceId?: string;
  referenceNumber?: string;
  description?: string;
  debitAmount?: number;
  creditAmount?: number;
  runningBalance?: number;
  transactionStatus?: string;
}

export interface ShopSupplierStatementDto {
  shopName?: string;
  shopAddress?: string;
  shopPhone?: string;
  shopEmail?: string;
  supplierId?: string;
  supplierCode?: string;
  supplierName?: string;
  supplierAddress?: string;
  supplierPhone?: string;
  supplierEmail?: string;
  dateFrom?: string;
  dateTo?: string;
  generatedDate?: string;
  openingBalance?: number;
  totalDebit?: number;
  totalCredit?: number;
  closingBalance?: number;
  payableAmount?: number;
  advanceAmount?: number;
  entries: ShopSupplierLedgerEntryDto[];
}
