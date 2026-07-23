import type { ShopCustomerLedgerReferenceType } from './shop-customer-ledger-reference-type.enum';

export interface GetShopCustomerLedgerInput {
  customerId: string;
  dateFrom?: string;
  dateTo?: string;
  referenceType?: ShopCustomerLedgerReferenceType;
  filter?: string;
}

export interface ShopCustomerBalanceSummaryDto {
  customerId?: string;
  customerCode?: string;
  customerName?: string;
  openingBalance?: number;
  totalCompletedSales?: number;
  totalInitialPaidAtSale?: number;
  totalPostedCustomerPayments?: number;
  currentBalance?: number;
  receivableAmount?: number;
  advanceAmount?: number;
  lastTransactionDate?: string;
}

export interface ShopCustomerLedgerDto {
  customerId?: string;
  customerCode?: string;
  customerName?: string;
  dateFrom?: string;
  dateTo?: string;
  openingBalance?: number;
  totalSales?: number;
  totalInitialPaid?: number;
  totalAdditionalPayments?: number;
  totalDebit?: number;
  totalCredit?: number;
  closingBalance?: number;
  receivableAmount?: number;
  advanceAmount?: number;
  entries: ShopCustomerLedgerEntryDto[];
}

export interface ShopCustomerLedgerEntryDto {
  transactionDate?: string;
  creationTime?: string;
  referenceType?: ShopCustomerLedgerReferenceType;
  referenceId?: string;
  referenceNumber?: string;
  description?: string;
  debitAmount?: number;
  creditAmount?: number;
  runningBalance?: number;
  transactionStatus?: string;
}

export interface ShopCustomerStatementDto {
  shopName?: string;
  shopAddress?: string;
  shopPhone?: string;
  shopEmail?: string;
  customerId?: string;
  customerCode?: string;
  customerName?: string;
  customerAddress?: string;
  customerPhone?: string;
  customerEmail?: string;
  dateFrom?: string;
  dateTo?: string;
  generatedDate?: string;
  openingBalance?: number;
  totalDebit?: number;
  totalCredit?: number;
  closingBalance?: number;
  receivableAmount?: number;
  advanceAmount?: number;
  entries: ShopCustomerLedgerEntryDto[];
}
