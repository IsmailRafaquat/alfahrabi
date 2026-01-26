import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface CreateUpdateFeeStructureItemDto {
  feeStructureId: string;
  feeHeadId: string;
  monthlyAmount: number;
  isMandatory: boolean;
}

export interface FeeStructureItemDto extends FullAuditedEntityDto<string> {
  tenantId?: string;
  feeStructureId?: string;
  feeHeadId?: string;
  monthlyAmount: number;
  isMandatory: boolean;
  feeHeadName?: string;
  feeStructureName?: string;
}

export interface GetFeeStructureItemListInput extends PagedAndSortedResultRequestDto {
  feeStructureId?: string;
  feeHeadId?: string;
  isMandatory?: boolean;
}
