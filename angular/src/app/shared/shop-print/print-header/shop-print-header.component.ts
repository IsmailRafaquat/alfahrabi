import { Component, Input } from '@angular/core';
import { ShopPrintDocumentDto } from '../../../proxy/shop-management/print-templates/models';
import { ShopPrintLabelSet } from '../models/shop-print-labels';

@Component({
  selector: 'shop-print-header',
  standalone: false,
  templateUrl: './shop-print-header.component.html',
  styleUrls: ['./shop-print-header.component.scss'],
})
export class ShopPrintHeaderComponent {
  @Input() document!: ShopPrintDocumentDto;
  @Input() labels!: ShopPrintLabelSet;
  @Input() cashierName?: string;
  @Input() showCashierName = true;
  @Input() showDateTime = true;
  @Input() copyLabel?: string;
}
