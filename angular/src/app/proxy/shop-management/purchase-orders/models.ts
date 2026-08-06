import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopPurchaseOrderStatus } from './shop-purchase-order-status.enum';

export interface CancelShopPurchaseOrderDto {
  cancellationReason: string;
}

export interface CreateShopPurchaseOrderDto extends ShopPurchaseOrderEditDtoBase<CreateShopPurchaseOrderItemDto> {
}

export interface CreateShopPurchaseOrderItemDto extends ShopPurchaseOrderItemEditDtoBase {
}

export interface GetShopPurchaseOrdersInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  supplierId?: string;
  status?: ShopPurchaseOrderStatus;
  orderDateFrom?: string;
  orderDateTo?: string;
  expectedDeliveryDateFrom?: string;
  expectedDeliveryDateTo?: string;
  minimumGrandTotal?: number;
  maximumGrandTotal?: number;
}

export interface RejectShopPurchaseOrderDto {
  rejectionReason: string;
}

export interface ShopPurchaseOrderDto extends EntityDto<string> {
  purchaseOrderNumber?: string;
  supplierId?: string;
  supplierCode?: string;
  supplierName?: string;
  orderDate?: string;
  expectedDeliveryDate?: string;
  status?: ShopPurchaseOrderStatus;
  supplierReference?: string;
  subTotal?: number;
  discountAmount?: number;
  taxAmount?: number;
  shippingCharges?: number;
  otherCharges?: number;
  grandTotal?: number;
  notes?: string;
  approvedByUserId?: string;
  approvedDate?: string;
  rejectedByUserId?: string;
  rejectedDate?: string;
  rejectionReason?: string;
  cancelledByUserId?: string;
  cancelledDate?: string;
  cancellationReason?: string;
  creationTime?: string;
  items: ShopPurchaseOrderItemDto[];
}

export interface ShopPurchaseOrderEditDtoBase<TItem> {
  supplierId: string;
  orderDate: string;
  expectedDeliveryDate?: string;
  supplierReference?: string;
  shippingCharges: number;
  otherCharges: number;
  notes?: string;
  items: TItem[];
}

export interface ShopPurchaseOrderItemDto extends EntityDto<string> {
  productId?: string;
  productName?: string;
  productCode?: string;
  unitName?: string;
  unitShortName?: string;
  description?: string;
  orderedQuantity: number;
  receivedQuantity: number;
  unitPurchasePrice?: number;
  discountPercentage: number;
  discountAmount?: number;
  taxPercentage: number;
  taxAmount?: number;
  lineSubTotal?: number;
  lineTotal?: number;
}

export interface ShopPurchaseOrderItemEditDtoBase {
  productId: string;
  description?: string;
  orderedQuantity: number;
  unitPurchasePrice: number;
  discountPercentage: number;
  taxPercentage: number;
}

export interface UpdateShopPurchaseOrderDto extends ShopPurchaseOrderEditDtoBase<UpdateShopPurchaseOrderItemDto> {
}

export interface UpdateShopPurchaseOrderItemDto extends ShopPurchaseOrderItemEditDtoBase {
}
