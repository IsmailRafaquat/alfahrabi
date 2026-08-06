import { Component, Input } from '@angular/core';
import { ShopPrintLineDto } from '../../../proxy/shop-management/print-templates/models';
import { ShopPrintPaperSize } from '../../../proxy/shop-management/print-settings';
import { ShopPrintLabelSet } from '../models/shop-print-labels';

@Component({
  selector: 'shop-print-item-table',
  standalone: false,
  templateUrl: './shop-print-item-table.component.html',
  styleUrls: ['./shop-print-item-table.component.scss'],
})
export class ShopPrintItemTableComponent {
  @Input() lines: ShopPrintLineDto[] = [];
  @Input() labels!: ShopPrintLabelSet;
  @Input() paperSize: ShopPrintPaperSize = ShopPrintPaperSize.Thermal80Mm;
  @Input() currencySymbol = '';
  @Input() decimalPlaces = 2;
  @Input() showUnit = true;
  @Input() showItemCode = false;
  @Input() showBatch = true;
  @Input() showExpiry = true;

  readonly ShopPrintPaperSize = ShopPrintPaperSize;

  get isStacked(): boolean {
    return this.paperSize === ShopPrintPaperSize.Thermal58Mm;
  }

  hasBatchOrExpiry(line: ShopPrintLineDto): boolean {
    return !!(this.showBatch && line.batchNumber) || !!(this.showExpiry && line.expiryDate);
  }
}
