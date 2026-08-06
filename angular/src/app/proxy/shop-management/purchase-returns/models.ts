import type { ShopPurchaseReturnReason } from './shop-purchase-return-reason.enum';
import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopPurchaseReturnStatus } from './shop-purchase-return-status.enum';

export interface CancelShopPurchaseReturnDto {
  cancellationReason: string;
}

export interface CreateShopPurchaseReturnDto {
  goodsReceiptId: string;
  returnDate?: string;
  reason?: ShopPurchaseReturnReason;
  reasonDetails?: string;
  otherCharges: number;
  notes?: string;
  items: CreateShopPurchaseReturnItemDto[];
}

export interface CreateShopPurchaseReturnItemDto {
  goodsReceiptItemId: string;
  returnQuantity: number;
  reason?: ShopPurchaseReturnReason;
  notes?: string;
}

export interface GetShopPurchaseReturnsInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  supplierId?: string;
  goodsReceiptId?: string;
  status?: ShopPurchaseReturnStatus;
  fromDate?: string;
  toDate?: string;
}

export interface ShopGoodsReceiptReturnableDto extends EntityDto<string> {
  goodsReceiptNumber?: string;
  supplierId?: string;
  supplierName?: string;
  receiptDate?: string;
  items: ShopGoodsReceiptReturnableItemDto[];
}

export interface ShopGoodsReceiptReturnableItemDto extends EntityDto<string> {
  productId?: string;
  productName?: string;
  productCode?: string;
  unitName?: string;
  unitShortName?: string;
  unitAllowDecimal: boolean;
  receivedQuantity: number;
  bonusQuantity: number;
  previouslyReturnedQuantity: number;
  returnableQuantity: number;
  purchasePrice?: number;
  taxPercentage: number;
  batchNumber?: string;
  expiryDate?: string;
}

export interface ShopPurchaseReturnDto extends FullAuditedEntityDto<string> {
  purchaseReturnNumber?: string;
  supplierId?: string;
  supplierName?: string;
  goodsReceiptId?: string;
  goodsReceiptNumber?: string;
  returnDate?: string;
  status?: ShopPurchaseReturnStatus;
  reason?: ShopPurchaseReturnReason;
  reasonDetails?: string;
  subTotal?: number;
  taxAmount?: number;
  otherCharges: number;
  grandTotal?: number;
  notes?: string;
  completedDate?: string;
  cancelledDate?: string;
  cancellationReason?: string;
  items: ShopPurchaseReturnItemDto[];
}

export interface ShopPurchaseReturnItemDto extends EntityDto<string> {
  goodsReceiptItemId?: string;
  productId?: string;
  productNameSnapshot?: string;
  productCodeSnapshot?: string;
  unitNameSnapshot?: string;
  unitShortNameSnapshot?: string;
  batchNumber?: string;
  expiryDate?: string;
  receivedQuantitySnapshot: number;
  previouslyReturnedQuantity: number;
  returnQuantity: number;
  unitPurchasePrice?: number;
  taxPercentage: number;
  taxAmount?: number;
  lineSubTotal?: number;
  lineTotal?: number;
  reason?: ShopPurchaseReturnReason;
  notes?: string;
}

export interface UpdateShopPurchaseReturnDto {
  returnDate?: string;
  reason?: ShopPurchaseReturnReason;
  reasonDetails?: string;
  otherCharges: number;
  notes?: string;
  items: UpdateShopPurchaseReturnItemDto[];
}

export interface UpdateShopPurchaseReturnItemDto extends CreateShopPurchaseReturnItemDto {
}
