import { mapEnumToOptions } from '@abp/ng.core';

export enum ShopAiResponseType {
  TextAnswer = 0,
  ModuleExplanation = 1,
  FieldList = 2,
  FieldExplanation = 3,
  MissingInformation = 4,
  LookupChoices = 5,
  ActionPreview = 6,
  ExecutionResult = 7,
  Error = 8,
  DataList = 9,
}

export const shopAiResponseTypeOptions = mapEnumToOptions(ShopAiResponseType);
