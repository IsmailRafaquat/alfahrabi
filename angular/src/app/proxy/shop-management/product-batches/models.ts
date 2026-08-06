import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopProductBatchStatus } from './shop-product-batch-status.enum';

export interface BlockShopProductBatchDto {
  blockReason: string;
}

export interface GetShopProductBatchesInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  productId?: string;
  productCategoryId?: string;
  supplierId?: string;
  status?: ShopProductBatchStatus;
  batchNumber?: string;
  expiryFrom?: string;
  expiryTo?: string;
  nearExpiryOnly?: boolean;
  expiredOnly?: boolean;
  hasAvailableStock?: boolean;
}

export interface ShopBatchAvailabilityDto {
  productId?: string;
  requiredQuantity: number;
  totalAvailableQuantity: number;
  isSufficient: boolean;
  batches: ShopProductBatchLookupDto[];
}

export interface ShopBatchSummaryDto {
  productId?: string;
  totalBatches: number;
  activeBatches: number;
  nearExpiryBatches: number;
  expiredBatches: number;
  exhaustedBatches: number;
  blockedBatches: number;
  totalReceivedQuantity: number;
  totalIssuedQuantity: number;
  totalAvailableQuantity: number;
  nearestExpiryDate?: string;
}

export interface ShopProductBatchDto extends EntityDto<string> {
  productId?: string;
  productCode?: string;
  productName?: string;
  unitName?: string;
  unitShortName?: string;
  batchNumber?: string;
  manufacturingDate?: string;
  expiryDate?: string;
  receivedQuantity: number;
  issuedQuantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  unitCost?: number;
  status?: ShopProductBatchStatus;
  daysToExpiry?: number;
  firstReceivedDate?: string;
  lastMovementDate?: string;
  supplierId?: string;
  supplierName?: string;
  goodsReceiptId?: string;
  goodsReceiptNumber?: string;
  notes?: string;
  isBlocked: boolean;
  blockReason?: string;
  creationTime?: string;
}

export interface ShopProductBatchLookupDto extends EntityDto<string> {
  productId?: string;
  batchNumber?: string;
  expiryDate?: string;
  daysToExpiry?: number;
  availableQuantity: number;
  status?: ShopProductBatchStatus;
  isBlocked: boolean;
}

export interface UpdateShopProductBatchDto {
  manufacturingDate?: string;
  expiryDate?: string;
  notes?: string;
}
