import type { ShopExpensePaymentMethod } from './shop-expense-payment-method.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopExpenseStatus } from './shop-expense-status.enum';

export interface CancelShopExpenseDto {
  cancellationReason: string;
}

export interface CreateUpdateShopExpenseDto {
  expenseCategoryId: string;
  expenseDate: string;
  amount: number;
  paymentMethod: ShopExpensePaymentMethod;
  paidTo?: string;
  referenceNumber?: string;
  chequeNumber?: string;
  bankName?: string;
  bankAccountId?: string;
  description?: string;
  notes?: string;
}

export interface GetShopExpensesInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  expenseCategoryId?: string;
  status?: ShopExpenseStatus;
  paymentMethod?: ShopExpensePaymentMethod;
  expenseDateFrom?: string;
  expenseDateTo?: string;
  minimumAmount?: number;
  maximumAmount?: number;
}

export interface ShopExpenseDto extends EntityDto<string> {
  expenseNumber?: string;
  expenseCategoryId?: string;
  expenseCategoryCode?: string;
  expenseCategoryName?: string;
  expenseDate?: string;
  amount?: number;
  paymentMethod?: ShopExpensePaymentMethod;
  paidTo?: string;
  referenceNumber?: string;
  chequeNumber?: string;
  bankName?: string;
  bankAccountId?: string;
  bankAccountCode?: string;
  bankAccountName?: string;
  description?: string;
  notes?: string;
  status?: ShopExpenseStatus;
  postedDate?: string;
  cancelledDate?: string;
  cancellationReason?: string;
  creationTime?: string;
}

export interface ShopExpenseSummaryDto {
  totalPostedExpenses?: number;
  totalDraftExpenses?: number;
  totalCancelledExpenses?: number;
  postedExpenseCount: number;
  draftExpenseCount: number;
  cancelledExpenseCount: number;
}
