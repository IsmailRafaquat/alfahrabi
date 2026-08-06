import { Pipe, PipeTransform } from '@angular/core';

// Formats using the printing shop's own CurrencySymbol/DecimalPlaces (from ShopSetting via
// ShopPrintBusinessDto) - unlike ShopReportCurrencyPipe (shared/reports), which hardcodes
// en-US/2-decimal formatting, this pipe honors per-tenant currency settings as required by the
// print spec ("Rs. 1,420.00" / "PKR 1,420.00" per existing currency formatting settings").
@Pipe({ name: 'shopPrintCurrency', standalone: false })
export class ShopPrintCurrencyPipe implements PipeTransform {
  transform(value: number | null | undefined, symbol = '', decimalPlaces = 2): string {
    const amount = value ?? 0;
    const formatted = amount.toLocaleString('en-US', {
      minimumFractionDigits: decimalPlaces,
      maximumFractionDigits: decimalPlaces,
    });
    return symbol ? `${symbol} ${formatted}` : formatted;
  }
}
