import type { FeeHeadChargeType } from './fee-head-charge-type.enum';
import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateFeeHeadDto {
  name: string;
  isActive: boolean;
  chargeType?: FeeHeadChargeType;
}

export interface FeeHeadDto extends FullAuditedEntityDto<string> {
  tenantId?: string;
  name?: string;
  isActive: boolean;
  chargeType?: FeeHeadChargeType;
}

export interface FeeHeadLookupDto {
  id?: string;
  name?: string;
}

export interface GetFeeHeadListInput extends PagedAndSortedResultRequestDto {
  filter?: string;
  isActive?: boolean;
}
