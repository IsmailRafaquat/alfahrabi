import { Component, Input } from '@angular/core';
import { ShopPrintDocumentDto } from '../../../proxy/shop-management/print-templates/models';
import { ShopPrintLabelSet } from '../models/shop-print-labels';
import { ShopPrintSettingsDto } from '../../../proxy/shop-management/print-settings/models';

// Clean, simple A4 letterhead layout - deliberately NOT the thermal layout (per spec: "Do not use
// the thermal layout for A4"). Reuses the same building-block components as the thermal receipt
// (header/item-table/totals/payment/footer) so both paper sizes stay visually consistent and
// neither duplicates markup, but composed inside an A4-styled shell instead of a narrow column.
@Component({
  selector: 'shop-a4-document',
  standalone: false,
  templateUrl: './shop-a4-document.component.html',
  styleUrls: ['./shop-a4-document.component.scss'],
})
export class ShopA4DocumentComponent {
  @Input() document!: ShopPrintDocumentDto;
  @Input() labels!: ShopPrintLabelSet;
  @Input() settings?: ShopPrintSettingsDto;
  @Input() cashierName?: string;
  @Input() copyLabel?: string;
}
