import type { ShopBankDirection } from './shop-bank-direction.enum';
import type { ShopBankTransferType } from './shop-bank-transfer-type.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopBankTransactionType } from './shop-bank-transaction-type.enum';
import type { ShopBankReferenceType } from './shop-bank-reference-type.enum';
import type { ShopBankTransferStatus } from './shop-bank-transfer-status.enum';

export interface CancelShopBankTransferDto {
  cancellationReason: string;
}

export interface CreateManualBankMovementDto {
  bankAccountId: string;
  transactionDate: string;
  direction: ShopBankDirection;
  amount: number;
  referenceNumber?: string;
  description?: string;
}

export interface CreateUpdateShopBankAccountDto {
  code: string;
  accountName: string;
  bankName: string;
  accountNumber?: string;
  iban?: string;
  branchName?: string;
  openingBalance: number;
  isDefault: boolean;
  isActive: boolean;
  notes?: string;
}

export interface CreateUpdateShopBankTransferDto {
  transferDate: string;
  transferType: ShopBankTransferType;
  fromBankAccountId?: string;
  toBankAccountId?: string;
  cashRegisterId?: string;
  amount: number;
  referenceNumber?: string;
  notes?: string;
}

export interface GetShopBankAccountsInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  isActive?: boolean;
  isDefault?: boolean;
  bankName?: string;
}

export interface GetShopBankTransactionsInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  bankAccountId?: string;
  transactionType?: ShopBankTransactionType;
  direction?: ShopBankDirection;
  referenceType?: ShopBankReferenceType;
  referenceNumber?: string;
  dateFrom?: string;
  dateTo?: string;
  minimumAmount?: number;
  maximumAmount?: number;
}

export interface GetShopBankTransfersInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  transferType?: ShopBankTransferType;
  status?: ShopBankTransferStatus;
  fromBankAccountId?: string;
  toBankAccountId?: string;
  dateFrom?: string;
  dateTo?: string;
}

export interface ShopBankAccountDto extends EntityDto<string> {
  code?: string;
  accountName?: string;
  bankName?: string;
  accountNumber?: string;
  iban?: string;
  branchName?: string;
  openingBalance?: number;
  currentBalance?: number;
  isDefault: boolean;
  isActive: boolean;
  notes?: string;
  creationTime?: string;
}

export interface ShopBankAccountLookupDto extends EntityDto<string> {
  code?: string;
  accountName?: string;
  bankName?: string;
  isDefault: boolean;
}

export interface ShopBankAccountSummaryDto {
  openingBalance?: number;
  totalMoneyIn?: number;
  totalMoneyOut?: number;
  currentBalance?: number;
  customerPayments?: number;
  supplierPayments?: number;
  expenses?: number;
  customerRefunds?: number;
  cashDeposits?: number;
  cashWithdrawals?: number;
  bankTransfersIn?: number;
  bankTransfersOut?: number;
  manualDeposits?: number;
  manualWithdrawals?: number;
}

export interface ShopBankTransactionDto extends EntityDto<string> {
  bankAccountId?: string;
  bankAccountCode?: string;
  bankAccountName?: string;
  transactionDate?: string;
  transactionType?: ShopBankTransactionType;
  direction?: ShopBankDirection;
  amount?: number;
  referenceType?: ShopBankReferenceType;
  referenceId?: string;
  referenceNumber?: string;
  description?: string;
  balanceAfterTransaction?: number;
  isReversal: boolean;
  creationTime?: string;
}

export interface ShopBankTransferDto extends EntityDto<string> {
  transferNumber?: string;
  transferDate?: string;
  transferType?: ShopBankTransferType;
  fromBankAccountId?: string;
  fromBankAccountName?: string;
  toBankAccountId?: string;
  toBankAccountName?: string;
  cashRegisterId?: string;
  cashRegisterName?: string;
  amount?: number;
  referenceNumber?: string;
  notes?: string;
  status?: ShopBankTransferStatus;
  postedByUserId?: string;
  postedDate?: string;
  cancelledByUserId?: string;
  cancelledDate?: string;
  cancellationReason?: string;
  creationTime?: string;
}
