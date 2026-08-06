import type { ShopStockAdjustmentReason } from './shop-stock-adjustment-reason.enum';
import type { ShopStockAdjustmentType } from './shop-stock-adjustment-type.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopStockAdjustmentStatus } from './shop-stock-adjustment-status.enum';

export interface CancelShopStockAdjustmentDto {
  cancellationReason: string;
}

export interface CreateShopStockAdjustmentDto {
  adjustmentDate: string;
  reason: ShopStockAdjustmentReason;
  reasonDetails?: string;
  notes?: string;
  items: CreateShopStockAdjustmentItemDto[];
}

export interface CreateShopStockAdjustmentItemDto {
  productId: string;
  adjustmentType: ShopStockAdjustmentType;
  adjustmentQuantity: number;
  batchNumber?: string;
  manufacturingDate?: string;
  expiryDate?: string;
  productBatchId?: string;
  reason?: ShopStockAdjustmentReason;
  notes?: string;
}

export interface GetShopStockAdjustmentsInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  status?: ShopStockAdjustmentStatus;
  reason?: ShopStockAdjustmentReason;
  adjustmentDateFrom?: string;
  adjustmentDateTo?: string;
  productId?: string;
  adjustmentType?: ShopStockAdjustmentType;
}

export interface ShopStockAdjustmentDto extends EntityDto<string> {
  adjustmentNumber?: string;
  adjustmentDate?: string;
  status?: ShopStockAdjustmentStatus;
  reason?: ShopStockAdjustmentReason;
  reasonDetails?: string;
  notes?: string;
  postedByUserId?: string;
  postedDate?: string;
  cancelledByUserId?: string;
  cancelledDate?: string;
  cancellationReason?: string;
  creationTime?: string;
  items: ShopStockAdjustmentItemDto[];
}

export interface ShopStockAdjustmentItemDto extends EntityDto<string> {
  productId?: string;
  productCode?: string;
  productName?: string;
  unitName?: string;
  unitShortName?: string;
  unitAllowDecimal: boolean;
  adjustmentType?: ShopStockAdjustmentType;
  systemQuantity: number;
  adjustmentQuantity: number;
  finalQuantity: number;
  unitCostSnapshot?: number;
  batchNumber?: string;
  manufacturingDate?: string;
  expiryDate?: string;
  productBatchId?: string;
  reason?: ShopStockAdjustmentReason;
  notes?: string;
}

export interface ShopStockAdjustmentProductLookupDto extends EntityDto<string> {
  code?: string;
  name?: string;
  unitId?: string;
  unitName?: string;
  unitShortName?: string;
  unitAllowDecimal: boolean;
  currentStock: number;
  trackBatch: boolean;
  trackExpiry: boolean;
  trackSerialNumber: boolean;
}

export interface UpdateShopStockAdjustmentDto {
  adjustmentDate: string;
  reason: ShopStockAdjustmentReason;
  reasonDetails?: string;
  notes?: string;
  items: UpdateShopStockAdjustmentItemDto[];
}

export interface UpdateShopStockAdjustmentItemDto extends CreateShopStockAdjustmentItemDto {
}
