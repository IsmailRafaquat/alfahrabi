import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateShopExpenseCategoryDto {
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface GetShopExpenseCategoriesInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  isActive?: boolean;
}

export interface ShopExpenseCategoryDto extends EntityDto<string> {
  code?: string;
  name?: string;
  description?: string;
  isActive: boolean;
  creationTime?: string;
}

export interface ShopExpenseCategoryLookupDto extends EntityDto<string> {
  code?: string;
  name?: string;
}
