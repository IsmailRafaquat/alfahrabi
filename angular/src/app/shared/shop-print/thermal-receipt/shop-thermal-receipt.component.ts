import { Component, Input } from '@angular/core';
import { ShopPrintDocumentDto } from '../../../proxy/shop-management/print-templates/models';
import { ShopPrintPaperSize } from '../../../proxy/shop-management/print-settings';
import { ShopPrintLabelSet } from '../models/shop-print-labels';
import { ShopPrintSettingsDto } from '../../../proxy/shop-management/print-settings/models';

@Component({
  selector: 'shop-thermal-receipt',
  standalone: false,
  templateUrl: './shop-thermal-receipt.component.html',
  styleUrls: ['./shop-thermal-receipt.component.scss'],
})
export class ShopThermalReceiptComponent {
  @Input() document!: ShopPrintDocumentDto;
  @Input() labels!: ShopPrintLabelSet;
  @Input() settings?: ShopPrintSettingsDto;
  @Input() cashierName?: string;
  @Input() copyLabel?: string;

  readonly ShopPrintPaperSize = ShopPrintPaperSize;
}
