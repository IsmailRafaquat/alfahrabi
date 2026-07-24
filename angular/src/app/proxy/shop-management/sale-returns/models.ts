import type { ShopSaleReturnReason } from './shop-sale-return-reason.enum';
import type { ShopSaleReturnSettlementType } from './shop-sale-return-settlement-type.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopSaleReturnStatus } from './shop-sale-return-status.enum';

export interface CancelShopSaleReturnDto {
  cancellationReason: string;
}

export interface CreateShopSaleReturnDto {
  saleId: string;
  returnDate?: string;
  reason?: ShopSaleReturnReason;
  reasonDetails?: string;
  settlementType?: ShopSaleReturnSettlementType;
  otherCharges: number;
  notes?: string;
  items: CreateShopSaleReturnItemDto[];
}

export interface CreateShopSaleReturnItemDto {
  saleItemId: string;
  returnQuantity: number;
  reason?: ShopSaleReturnReason;
  notes?: string;
}

export interface GetShopSaleReturnsInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  saleId?: string;
  customerId?: string;
  status?: ShopSaleReturnStatus;
  reason?: ShopSaleReturnReason;
  settlementType?: ShopSaleReturnSettlementType;
  returnDateFrom?: string;
  returnDateTo?: string;
}

export interface ShopSaleReturnDto extends EntityDto<string> {
  saleReturnNumber?: string;
  saleId?: string;
  saleNumber?: string;
  customerId?: string;
  customerCode?: string;
  customerName?: string;
  returnDate?: string;
  status?: ShopSaleReturnStatus;
  reason?: ShopSaleReturnReason;
  reasonDetails?: string;
  settlementType?: ShopSaleReturnSettlementType;
  subTotal?: number;
  discountAmount?: number;
  taxAmount?: number;
  otherCharges?: number;
  grandTotal?: number;
  refundAmount?: number;
  customerCreditAmount?: number;
  notes?: string;
  completedDate?: string;
  cancelledDate?: string;
  cancellationReason?: string;
  creationTime?: string;
  items: ShopSaleReturnItemDto[];
}

export interface ShopSaleReturnItemDto extends EntityDto<string> {
  saleItemId?: string;
  productId?: string;
  productCode?: string;
  productName?: string;
  unitName?: string;
  unitShortName?: string;
  soldQuantity: number;
  previouslyReturnedQuantity: number;
  returnableQuantity: number;
  returnQuantity: number;
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
  reason?: ShopSaleReturnReason;
  notes?: string;
}

export interface ShopSaleReturnableDto extends EntityDto<string> {
  saleNumber?: string;
  customerId?: string;
  customerCode?: string;
  customerName?: string;
  saleDate?: string;
  grandTotal?: number;
  paidAmount?: number;
  pendingAmount?: number;
  items: ShopSaleReturnableItemDto[];
}

export interface ShopSaleReturnableItemDto extends EntityDto<string> {
  saleItemId?: string;
  productId?: string;
  productCode?: string;
  productName?: string;
  unitName?: string;
  unitShortName?: string;
  unitAllowDecimal: boolean;
  soldQuantity: number;
  previouslyReturnedQuantity: number;
  returnableQuantity: number;
  unitSalePrice?: number;
  discountPercentage: number;
  taxPercentage: number;
  batchNumber?: string;
  expiryDate?: string;
}

export interface UpdateShopSaleReturnDto {
  returnDate?: string;
  reason?: ShopSaleReturnReason;
  reasonDetails?: string;
  settlementType?: ShopSaleReturnSettlementType;
  otherCharges: number;
  notes?: string;
  items: UpdateShopSaleReturnItemDto[];
}

export interface UpdateShopSaleReturnItemDto extends CreateShopSaleReturnItemDto {
}
