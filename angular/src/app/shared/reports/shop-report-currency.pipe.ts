import { Pipe, PipeTransform } from '@angular/core';
import { formatNumber } from '@angular/common';

@Pipe({ name: 'shopReportCurrency', standalone: false })
export class ShopReportCurrencyPipe implements PipeTransform {
  transform(value: number | null | undefined): string {
    if (value === null || value === undefined) {
      return '-';
    }
    return formatNumber(value, 'en-US', '1.2-2');
  }
}
