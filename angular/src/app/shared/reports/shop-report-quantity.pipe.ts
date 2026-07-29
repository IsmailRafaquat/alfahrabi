import { Pipe, PipeTransform } from '@angular/core';
import { formatNumber } from '@angular/common';

@Pipe({ name: 'shopReportQuantity', standalone: false })
export class ShopReportQuantityPipe implements PipeTransform {
  transform(value: number | null | undefined): string {
    if (value === null || value === undefined) {
      return '-';
    }
    return formatNumber(value, 'en-US', '1.0-4');
  }
}
