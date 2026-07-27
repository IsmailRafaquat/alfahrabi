import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopStockTransactionType } from './shop-stock-transaction-type.enum';
import type { ShopStockReferenceType } from './shop-stock-reference-type.enum';

export interface GetShopStockTransactionsInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  productId?: string;
  transactionType?: ShopStockTransactionType;
  referenceType?: ShopStockReferenceType;
  dateFrom?: string;
  dateTo?: string;
  referenceNumber?: string;
}

export interface ShopStockTransactionDto extends EntityDto<string> {
  productId?: string;
  productName?: string;
  productCode?: string;
  transactionType?: ShopStockTransactionType;
  referenceType?: ShopStockReferenceType;
  referenceId?: string;
  referenceNumber?: string;
  transactionDate?: string;
  quantityIn: number;
  quantityOut: number;
  balanceQuantity: number;
  unitCost?: number;
  totalCost?: number;
  batchNumber?: string;
  expiryDate?: string;
  productBatchId?: string;
  batchBalanceQuantity?: number;
  notes?: string;
  creationTime?: string;
}
