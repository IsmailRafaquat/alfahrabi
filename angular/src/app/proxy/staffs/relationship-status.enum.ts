import { mapEnumToOptions } from '@abp/ng.core';

export enum RelationshipStatus {
  Single = 1,
  Married = 2,
  Engaged = 3,
  Other = 4,
}

export const relationshipStatusOptions = mapEnumToOptions(RelationshipStatus);
