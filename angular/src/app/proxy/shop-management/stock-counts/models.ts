import type { ShopStockCountScope } from './shop-stock-count-scope.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopStockCountStatus } from './shop-stock-count-status.enum';
import type { ShopStockAdjustmentType } from '../stock-adjustments/shop-stock-adjustment-type.enum';

export interface CancelShopStockCountDto {
  cancellationReason: string;
}

export interface CreateShopStockCountDto {
  countDate: string;
  scope: ShopStockCountScope;
  productCategoryId?: string;
  selectedProductIds: string[];
  notes?: string;
}

export interface GetShopStockCountsInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  status?: ShopStockCountStatus;
  scope?: ShopStockCountScope;
  productCategoryId?: string;
  productId?: string;
  countDateFrom?: string;
  countDateTo?: string;
  hasDifferences?: boolean;
}

export interface ShopStockCountDto extends EntityDto<string> {
  stockCountNumber?: string;
  countDate?: string;
  scope?: ShopStockCountScope;
  productCategoryId?: string;
  productCategoryName?: string;
  status?: ShopStockCountStatus;
  notes?: string;
  generatedStockAdjustmentId?: string;
  generatedStockAdjustmentNumber?: string;
  startedDate?: string;
  countedDate?: string;
  postedDate?: string;
  cancelledDate?: string;
  cancellationReason?: string;
  creationTime?: string;
  totalItems: number;
  countedItems: number;
  differenceItems: number;
  totalIncreaseQuantity: number;
  totalDecreaseQuantity: number;
  items: ShopStockCountItemDto[];
}

export interface ShopStockCountItemDto extends EntityDto<string> {
  productId?: string;
  productCode?: string;
  productName?: string;
  unitName?: string;
  unitShortName?: string;
  unitAllowDecimal: boolean;
  systemQuantity: number;
  physicalQuantity?: number;
  differenceQuantity: number;
  adjustmentType?: ShopStockAdjustmentType;
  isCounted: boolean;
  notes?: string;
}

export interface ShopStockCountPostingPreviewDto {
  totalProducts: number;
  noDifferenceCount: number;
  increaseCount: number;
  decreaseCount: number;
  totalIncreaseQuantity: number;
  totalDecreaseQuantity: number;
  canPost: boolean;
  changedProducts: string[];
}

export interface ShopStockCountProductLookupDto extends EntityDto<string> {
  code?: string;
  name?: string;
  categoryId?: string;
  unitName?: string;
  unitShortName?: string;
  unitAllowDecimal: boolean;
  currentStock: number;
  trackBatch: boolean;
  trackExpiry: boolean;
  trackSerialNumber: boolean;
}

export interface UpdateShopStockCountDto {
  countDate: string;
  scope: ShopStockCountScope;
  productCategoryId?: string;
  selectedProductIds: string[];
  notes?: string;
}

export interface UpdateShopStockCountItemQuantityDto {
  stockCountItemId: string;
  physicalQuantity: number;
  notes?: string;
}
