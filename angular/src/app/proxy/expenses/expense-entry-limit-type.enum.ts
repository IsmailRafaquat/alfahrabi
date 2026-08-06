import { mapEnumToOptions } from '@abp/ng.core';

export enum ExpenseEntryLimitType {
  MultipleTimesInMonth = 1,
  OneTimePerMonth = 2,
}

export const expenseEntryLimitTypeOptions = mapEnumToOptions(ExpenseEntryLimitType);
