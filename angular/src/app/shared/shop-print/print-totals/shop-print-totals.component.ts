import { Component, Input } from '@angular/core';
import { ShopPrintTotalsDto } from '../../../proxy/shop-management/print-templates/models';
import { ShopPrintLabelSet } from '../models/shop-print-labels';

@Component({
  selector: 'shop-print-totals',
  standalone: false,
  templateUrl: './shop-print-totals.component.html',
  styleUrls: ['./shop-print-totals.component.scss'],
})
export class ShopPrintTotalsComponent {
  @Input() totals!: ShopPrintTotalsDto;
  @Input() labels!: ShopPrintLabelSet;
  @Input() currencySymbol = '';
  @Input() decimalPlaces = 2;
}
