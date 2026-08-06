import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateShopProductCategoryDto {
  name: string;
  code: string;
  parentCategoryId?: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
}

export interface GetShopProductCategoriesInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  parentCategoryId?: string;
  rootCategoriesOnly: boolean;
  isActive?: boolean;
}

export interface ShopProductCategoryDto extends EntityDto<string> {
  name?: string;
  code?: string;
  parentCategoryId?: string;
  parentCategoryName?: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  creationTime?: string;
}

export interface ShopProductCategoryLookupDto extends EntityDto<string> {
  name?: string;
  code?: string;
  parentCategoryId?: string;
  displayName?: string;
}

export interface ShopProductCategoryTreeDto {
  id?: string;
  name?: string;
  code?: string;
  parentCategoryId?: string;
  displayOrder: number;
  isActive: boolean;
  children: ShopProductCategoryTreeDto[];
}
