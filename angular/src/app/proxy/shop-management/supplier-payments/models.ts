import type { ShopSupplierPaymentType } from './shop-supplier-payment-type.enum';
import type { ShopSupplierPaymentMethod } from './shop-supplier-payment-method.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopSupplierPaymentStatus } from './shop-supplier-payment-status.enum';

export interface CancelShopSupplierPaymentDto {
  cancellationReason: string;
}

export interface CreateUpdateShopSupplierPaymentAllocationDto {
  goodsReceiptId: string;
  allocatedAmount: number;
}

export interface CreateUpdateShopSupplierPaymentDto {
  supplierId: string;
  paymentDate: string;
  paymentType: ShopSupplierPaymentType;
  paymentMethod: ShopSupplierPaymentMethod;
  amount: number;
  referenceNumber?: string;
  chequeNumber?: string;
  bankName?: string;
  notes?: string;
  allocations: CreateUpdateShopSupplierPaymentAllocationDto[];
}

export interface GetShopSupplierPaymentsInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  supplierId?: string;
  paymentType?: ShopSupplierPaymentType;
  paymentMethod?: ShopSupplierPaymentMethod;
  status?: ShopSupplierPaymentStatus;
  paymentDateFrom?: string;
  paymentDateTo?: string;
}

export interface ShopSupplierOutstandingReceiptDto {
  goodsReceiptId?: string;
  goodsReceiptNumber?: string;
  supplierInvoiceNumber?: string;
  receiptDate?: string;
  grandTotal?: number;
  paidAmount?: number;
  pendingAmount?: number;
}

export interface ShopSupplierPaymentAllocationDto extends EntityDto<string> {
  goodsReceiptId?: string;
  goodsReceiptNumber?: string;
  supplierInvoiceNumber?: string;
  receiptDate?: string;
  grandTotal?: number;
  allocatedAmount?: number;
  creationTime?: string;
}

export interface ShopSupplierPaymentDto extends EntityDto<string> {
  paymentNumber?: string;
  supplierId?: string;
  supplierCode?: string;
  supplierName?: string;
  paymentDate?: string;
  paymentType?: ShopSupplierPaymentType;
  paymentMethod?: ShopSupplierPaymentMethod;
  amount?: number;
  referenceNumber?: string;
  chequeNumber?: string;
  bankName?: string;
  notes?: string;
  status?: ShopSupplierPaymentStatus;
  allocatedAmount?: number;
  unallocatedAmount?: number;
  postedByUserId?: string;
  postedDate?: string;
  cancelledByUserId?: string;
  cancelledDate?: string;
  cancellationReason?: string;
  creationTime?: string;
  allocations: ShopSupplierPaymentAllocationDto[];
}
