import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateShopUnitDto {
  name: string;
  shortName: string;
  allowDecimal: boolean;
  isActive: boolean;
}

export interface GetShopUnitsInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  allowDecimal?: boolean;
  isActive?: boolean;
}

export interface ShopUnitDto extends EntityDto<string> {
  name?: string;
  shortName?: string;
  allowDecimal: boolean;
  isActive: boolean;
  creationTime?: string;
}

export interface ShopUnitLookupDto extends EntityDto<string> {
  name?: string;
  shortName?: string;
  allowDecimal: boolean;
}
