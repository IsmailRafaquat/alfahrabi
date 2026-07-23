import type { ShopCustomerType } from './shop-customer-type.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateShopCustomerDto {
  code: string;
  name: string;
  customerType?: ShopCustomerType;
  contactPerson?: string;
  phone?: string;
  alternatePhone?: string;
  email?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  stateOrProvince?: string;
  postalCode?: string;
  country?: string;
  taxNumber?: string;
  openingBalance: number;
  creditLimit: number;
  paymentTermsDays: number;
  notes?: string;
  isWalkInCustomer: boolean;
  isActive: boolean;
}

export interface GetShopCustomersInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  customerType?: ShopCustomerType;
  city?: string;
  country?: string;
  isActive?: boolean;
  isWalkInCustomer?: boolean;
  hasOpeningBalance?: boolean;
  hasCreditLimit?: boolean;
}

export interface ShopCustomerDto extends EntityDto<string> {
  code?: string;
  name?: string;
  customerType?: ShopCustomerType;
  contactPerson?: string;
  phone?: string;
  alternatePhone?: string;
  email?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  stateOrProvince?: string;
  postalCode?: string;
  country?: string;
  taxNumber?: string;
  openingBalance?: number;
  creditLimit?: number;
  paymentTermsDays: number;
  notes?: string;
  isWalkInCustomer: boolean;
  isActive: boolean;
  creationTime?: string;
}

export interface ShopCustomerLookupDto extends EntityDto<string> {
  code?: string;
  name?: string;
  customerType?: ShopCustomerType;
  phone?: string;
  creditLimit?: number;
  paymentTermsDays: number;
  isWalkInCustomer: boolean;
}
