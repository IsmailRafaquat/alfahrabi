import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopSaleStatus } from './shop-sale-status.enum';
import type { ShopSaleType } from './shop-sale-type.enum';
import type { ShopSalePaymentMethod } from './shop-sale-payment-method.enum';

export interface CancelShopSaleDto {
  cancellationReason: string;
}

export interface CompleteShopSaleDto {
  itemBatchAllocations: CompleteShopSaleItemBatchAllocationDto[];
}

export interface CompleteShopSaleItemBatchAllocationDto {
  saleItemId?: string;
  allocations: ShopBatchAllocationLineDto[];
}

export interface CreateShopSaleDto extends ShopSaleEditDtoBase<CreateShopSaleItemDto> {
}

export interface CreateShopSaleItemDto extends ShopSaleItemEditDtoBase {
}

export interface GetShopSalesInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  customerId?: string;
  status?: ShopSaleStatus;
  saleType?: ShopSaleType;
  saleDateFrom?: string;
  saleDateTo?: string;
  minimumGrandTotal?: number;
  maximumGrandTotal?: number;
  hasPendingAmount?: boolean;
}

export interface ShopBatchAllocationLineDto {
  productBatchId?: string;
  quantity: number;
}

export interface ShopSaleDto extends EntityDto<string> {
  saleNumber?: string;
  customerId?: string;
  customerCode?: string;
  customerName?: string;
  saleDate?: string;
  dueDate?: string;
  saleType?: ShopSaleType;
  paymentMethod?: ShopSalePaymentMethod;
  status?: ShopSaleStatus;
  subTotal?: number;
  discountAmount?: number;
  taxAmount?: number;
  otherCharges?: number;
  grandTotal?: number;
  paidAmount?: number;
  pendingAmount?: number;
  referenceNumber?: string;
  notes?: string;
  completedByUserId?: string;
  completedDate?: string;
  cancelledByUserId?: string;
  cancelledDate?: string;
  cancellationReason?: string;
  creationTime?: string;
  items: ShopSaleItemDto[];
}

export interface ShopSaleEditDtoBase<TItem> {
  customerId: string;
  saleDate: string;
  dueDate?: string;
  saleType?: ShopSaleType;
  paymentMethod?: ShopSalePaymentMethod;
  paidAmount: number;
  referenceNumber?: string;
  otherCharges: number;
  notes?: string;
  items: TItem[];
}

export interface ShopSaleItemBatchAllocationDto {
  productBatchId?: string;
  batchNumber?: string;
  expiryDate?: string;
  quantity: number;
  unitCostSnapshot?: number;
}

export interface ShopSaleItemDto extends EntityDto<string> {
  productId?: string;
  productName?: string;
  productCode?: string;
  unitName?: string;
  unitShortName?: string;
  unitAllowDecimal: boolean;
  quantity: number;
  unitSalePrice?: number;
  unitCostSnapshot?: number;
  discountPercentage: number;
  discountAmount?: number;
  taxPercentage: number;
  taxAmount?: number;
  lineSubTotal?: number;
  lineTotal?: number;
  batchNumber?: string;
  expiryDate?: string;
  batchAllocations: ShopSaleItemBatchAllocationDto[];
}

export interface ShopSaleItemEditDtoBase {
  productId: string;
  quantity: number;
  unitSalePrice: number;
  discountPercentage: number;
  taxPercentage: number;
  batchNumber?: string;
  expiryDate?: string;
}

export interface ShopSaleProductLookupDto extends EntityDto<string> {
  code?: string;
  name?: string;
  unitId?: string;
  unitName?: string;
  unitShortName?: string;
  unitAllowDecimal: boolean;
  currentStock: number;
  salePrice?: number;
  taxPercentage: number;
  trackBatch: boolean;
  trackExpiry: boolean;
}

export interface UpdateShopSaleDto extends ShopSaleEditDtoBase<UpdateShopSaleItemDto> {
}

export interface UpdateShopSaleItemDto extends ShopSaleItemEditDtoBase {
}
