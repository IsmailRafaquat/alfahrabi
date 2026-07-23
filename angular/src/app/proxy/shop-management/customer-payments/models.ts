import type { ShopCustomerPaymentType } from './shop-customer-payment-type.enum';
import type { ShopCustomerPaymentMethod } from './shop-customer-payment-method.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ShopCustomerPaymentStatus } from './shop-customer-payment-status.enum';

export interface CancelShopCustomerPaymentDto {
  cancellationReason: string;
}

export interface CreateUpdateShopCustomerPaymentAllocationDto {
  saleId: string;
  allocatedAmount: number;
}

export interface CreateUpdateShopCustomerPaymentDto {
  customerId: string;
  paymentDate: string;
  paymentType: ShopCustomerPaymentType;
  paymentMethod: ShopCustomerPaymentMethod;
  amount: number;
  referenceNumber?: string;
  chequeNumber?: string;
  bankName?: string;
  notes?: string;
  allocations: CreateUpdateShopCustomerPaymentAllocationDto[];
}

export interface GetShopCustomerPaymentsInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  customerId?: string;
  paymentType?: ShopCustomerPaymentType;
  paymentMethod?: ShopCustomerPaymentMethod;
  status?: ShopCustomerPaymentStatus;
  paymentDateFrom?: string;
  paymentDateTo?: string;
}

export interface ShopCustomerOutstandingSaleDto {
  saleId?: string;
  saleNumber?: string;
  saleDate?: string;
  dueDate?: string;
  grandTotal?: number;
  initialPaidAmount?: number;
  additionalPaidAmount?: number;
  totalPaidAmount?: number;
  pendingAmount?: number;
}

export interface ShopCustomerPaymentAllocationDto extends EntityDto<string> {
  saleId?: string;
  saleNumber?: string;
  saleDate?: string;
  grandTotal?: number;
  allocatedAmount?: number;
  creationTime?: string;
}

export interface ShopCustomerPaymentDto extends EntityDto<string> {
  paymentNumber?: string;
  customerId?: string;
  customerCode?: string;
  customerName?: string;
  paymentDate?: string;
  paymentType?: ShopCustomerPaymentType;
  paymentMethod?: ShopCustomerPaymentMethod;
  amount?: number;
  referenceNumber?: string;
  chequeNumber?: string;
  bankName?: string;
  notes?: string;
  status?: ShopCustomerPaymentStatus;
  allocatedAmount?: number;
  unallocatedAmount?: number;
  postedByUserId?: string;
  postedDate?: string;
  cancelledByUserId?: string;
  cancelledDate?: string;
  cancellationReason?: string;
  creationTime?: string;
  allocations: ShopCustomerPaymentAllocationDto[];
}
