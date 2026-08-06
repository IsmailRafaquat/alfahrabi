import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopGoodsReceiptStatus } from './shop-goods-receipt-status.enum';
import type { ShopGoodsReceiptPaymentStatus } from './shop-goods-receipt-payment-status.enum';
import type { ShopPurchaseOrderStatus } from '../purchase-orders/shop-purchase-order-status.enum';

export interface CancelShopGoodsReceiptDto {
  cancellationReason: string;
}

export interface CreateShopGoodsReceiptDto extends ShopGoodsReceiptEditDtoBase<CreateShopGoodsReceiptItemDto> {
}

export interface CreateShopGoodsReceiptItemDto extends ShopGoodsReceiptItemEditDtoBase {
}

export interface GetShopGoodsReceiptsInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  purchaseOrderId?: string;
  supplierId?: string;
  status?: ShopGoodsReceiptStatus;
  receiptDateFrom?: string;
  receiptDateTo?: string;
  minimumGrandTotal?: number;
  maximumGrandTotal?: number;
}

export interface ShopGoodsReceiptDto extends EntityDto<string> {
  goodsReceiptNumber?: string;
  purchaseOrderId?: string;
  purchaseOrderNumber?: string;
  supplierId?: string;
  supplierCode?: string;
  supplierName?: string;
  supplierInvoiceNumber?: string;
  receiptDate?: string;
  status?: ShopGoodsReceiptStatus;
  subTotal?: number;
  discountAmount?: number;
  taxAmount?: number;
  shippingCharges?: number;
  otherCharges?: number;
  grandTotal?: number;
  paidAmount?: number;
  returnAmount?: number;
  pendingAmount?: number;
  paymentStatus?: ShopGoodsReceiptPaymentStatus;
  notes?: string;
  receivedByUserId?: string;
  completedByUserId?: string;
  completedDate?: string;
  cancelledByUserId?: string;
  cancelledDate?: string;
  cancellationReason?: string;
  creationTime?: string;
  items: ShopGoodsReceiptItemDto[];
}

export interface ShopGoodsReceiptEditDtoBase<TItem> {
  purchaseOrderId: string;
  supplierInvoiceNumber?: string;
  receiptDate: string;
  shippingCharges: number;
  otherCharges: number;
  notes?: string;
  items: TItem[];
}

export interface ShopGoodsReceiptItemDto extends EntityDto<string> {
  purchaseOrderItemId?: string;
  productId?: string;
  productName?: string;
  productCode?: string;
  unitName?: string;
  unitShortName?: string;
  unitAllowDecimal: boolean;
  trackBatch: boolean;
  trackExpiry: boolean;
  trackSerialNumber: boolean;
  orderedQuantity: number;
  previouslyReceivedQuantity: number;
  remainingQuantity: number;
  receivedQuantity: number;
  bonusQuantity: number;
  purchasePrice?: number;
  salePrice: number;
  batchNumber?: string;
  manufacturingDate?: string;
  expiryDate?: string;
  discountPercentage: number;
  discountAmount?: number;
  taxPercentage: number;
  taxAmount?: number;
  lineSubTotal?: number;
  lineTotal?: number;
}

export interface ShopGoodsReceiptItemEditDtoBase {
  purchaseOrderItemId: string;
  receivedQuantity: number;
  bonusQuantity: number;
  purchasePrice: number;
  salePrice: number;
  batchNumber?: string;
  manufacturingDate?: string;
  expiryDate?: string;
  discountPercentage: number;
  taxPercentage: number;
}

export interface ShopPurchaseOrderReceivingDto {
  purchaseOrderId?: string;
  purchaseOrderNumber?: string;
  supplierId?: string;
  supplierCode?: string;
  supplierName?: string;
  orderDate?: string;
  expectedDeliveryDate?: string;
  status?: ShopPurchaseOrderStatus;
  items: ShopPurchaseOrderReceivingItemDto[];
}

export interface ShopPurchaseOrderReceivingItemDto {
  purchaseOrderItemId?: string;
  productId?: string;
  productName?: string;
  productCode?: string;
  unitName?: string;
  unitShortName?: string;
  unitAllowDecimal: boolean;
  trackBatch: boolean;
  trackExpiry: boolean;
  trackSerialNumber: boolean;
  orderedQuantity: number;
  previouslyReceivedQuantity: number;
  remainingQuantity: number;
  defaultPurchasePrice?: number;
  defaultSalePrice?: number;
}

export interface UpdateShopGoodsReceiptDto extends ShopGoodsReceiptEditDtoBase<UpdateShopGoodsReceiptItemDto> {
}

export interface UpdateShopGoodsReceiptItemDto extends ShopGoodsReceiptItemEditDtoBase {
}
