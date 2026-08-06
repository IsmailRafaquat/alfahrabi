import type { ShopCashDirection } from './shop-cash-direction.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopCashTransactionType } from './shop-cash-transaction-type.enum';
import type { ShopCashClosingStatus } from './shop-cash-closing-status.enum';
import type { ShopCashReferenceType } from './shop-cash-reference-type.enum';

export interface CancelShopCashClosingDto {
  cancellationReason: string;
}

export interface CloseShopCashRegisterDto {
  actualClosingCash: number;
  notes?: string;
}

export interface CreateManualCashMovementDto {
  cashRegisterId: string;
  transactionDate: string;
  direction: ShopCashDirection;
  amount: number;
  referenceNumber?: string;
  description?: string;
}

export interface CreateUpdateShopCashRegisterDto {
  code: string;
  name: string;
  description?: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface GetShopCashRegistersInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  isActive?: boolean;
}

export interface GetShopCashTransactionsInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  cashRegisterId?: string;
  cashClosingId?: string;
  transactionType?: ShopCashTransactionType;
  direction?: ShopCashDirection;
  transactionDateFrom?: string;
  transactionDateTo?: string;
}

export interface OpenShopCashRegisterDto {
  businessDate: string;
  openingCash: number;
  notes?: string;
}

export interface ShopCashClosingDto extends EntityDto<string> {
  cashRegisterId?: string;
  cashRegisterCode?: string;
  cashRegisterName?: string;
  businessDate?: string;
  status?: ShopCashClosingStatus;
  openingCash?: number;
  cashSales?: number;
  customerCashPayments?: number;
  supplierCashPayments?: number;
  cashExpenses?: number;
  customerRefunds?: number;
  manualCashIn?: number;
  manualCashOut?: number;
  expectedClosingCash?: number;
  actualClosingCash?: number;
  differenceAmount?: number;
  notes?: string;
  cancellationReason?: string;
  openedByUserId?: string;
  openedDate?: string;
  closedByUserId?: string;
  closedDate?: string;
  creationTime?: string;
}

export interface ShopCashRegisterDto extends EntityDto<string> {
  code?: string;
  name?: string;
  description?: string;
  isDefault: boolean;
  isActive: boolean;
  creationTime?: string;
}

export interface ShopCashRegisterLookupDto extends EntityDto<string> {
  code?: string;
  name?: string;
  isDefault: boolean;
}

export interface ShopCashRegisterSummaryDto {
  openingCash?: number;
  cashSales?: number;
  customerCashPayments?: number;
  supplierCashPayments?: number;
  cashExpenses?: number;
  customerRefunds?: number;
  manualCashIn?: number;
  manualCashOut?: number;
  expectedClosingCash?: number;
  actualClosingCash?: number;
  differenceAmount?: number;
  isShort: boolean;
  isExcess: boolean;
}

export interface ShopCashRegisterTransactionDto extends EntityDto<string> {
  cashRegisterId?: string;
  cashRegisterCode?: string;
  cashRegisterName?: string;
  cashClosingId?: string;
  transactionDate?: string;
  transactionType?: ShopCashTransactionType;
  direction?: ShopCashDirection;
  amount?: number;
  referenceType?: ShopCashReferenceType;
  referenceId?: string;
  referenceNumber?: string;
  description?: string;
  runningBalance?: number;
  creationTime?: string;
}
