import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateShopSupplierDto {
  code: string;
  name: string;
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
  isActive: boolean;
}

export interface GetShopSuppliersInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  city?: string;
  country?: string;
  isActive?: boolean;
  hasOpeningBalance?: boolean;
  hasCreditLimit?: boolean;
}

export interface ShopSupplierDto extends EntityDto<string> {
  code?: string;
  name?: string;
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
  isActive: boolean;
  creationTime?: string;
}

export interface ShopSupplierLookupDto extends EntityDto<string> {
  code?: string;
  name?: string;
  contactPerson?: string;
  phone?: string;
  paymentTermsDays: number;
  displayName?: string;
}
