import { Component, Input } from '@angular/core';
import { ShopPrintPaymentDto } from '../../../proxy/shop-management/print-templates/models';
import { ShopPrintLabelSet } from '../models/shop-print-labels';

@Component({
  selector: 'shop-print-payment-summary',
  standalone: false,
  templateUrl: './shop-print-payment-summary.component.html',
  styleUrls: ['./shop-print-payment-summary.component.scss'],
})
export class ShopPrintPaymentSummaryComponent {
  @Input() payment?: ShopPrintPaymentDto;
  @Input() labels!: ShopPrintLabelSet;
  @Input() currencySymbol = '';
  @Input() decimalPlaces = 2;
}
