import { Component, Input } from '@angular/core';
import { ShopPrintDocumentDto } from '../../../proxy/shop-management/print-templates/models';
import { ShopPrintPaperSize } from '../../../proxy/shop-management/print-settings';
import { ShopPrintSettingsDto } from '../../../proxy/shop-management/print-settings/models';
import { SHOP_PRINT_LABEL_DOCUMENT_TYPES } from '../models/shop-print-document-types';
import { ShopPrintLang, SHOP_PRINT_LABELS } from '../models/shop-print-labels';

// Single print root every document type renders through - keeps using the existing global
// .print-only isolation rule in angular/src/styles.scss unchanged, just adds the paper-size
// class alongside it (thermal-print-58 / thermal-print-80 / a4-print).
@Component({
  selector: 'shop-print-layout',
  standalone: false,
  templateUrl: './shop-print-layout.component.html',
  styleUrls: ['./shop-print-layout.component.scss'],
})
export class ShopPrintLayoutComponent {
  @Input() document!: ShopPrintDocumentDto;
  @Input() settings?: ShopPrintSettingsDto;
  @Input() lang: ShopPrintLang = 'en';
  @Input() cashierName?: string;
  @Input() copies = 1;
  @Input() isDuplicateCopy = false;

  readonly ShopPrintPaperSize = ShopPrintPaperSize;

  get labels() {
    return SHOP_PRINT_LABELS[this.lang];
  }

  get isLabelDocument(): boolean {
    return SHOP_PRINT_LABEL_DOCUMENT_TYPES.includes(this.document?.documentType);
  }

  get paperSizeClass(): string {
    switch (this.document?.paperSize) {
      case ShopPrintPaperSize.Thermal58Mm:
        return 'thermal-print-58';
      case ShopPrintPaperSize.A4:
        return 'a4-print';
      default:
        return 'thermal-print-80';
    }
  }

  get fontSizeClass(): string {
    switch (this.settings?.thermalFontSize) {
      case 0:
        return 'thermal-font-small';
      case 2:
        return 'thermal-font-large';
      default:
        return '';
    }
  }

  get densityClass(): string {
    switch (this.settings?.thermalPrintDensity) {
      case 0:
        return 'thermal-density-light';
      case 2:
        return 'thermal-density-dark';
      default:
        return '';
    }
  }

  get copyLabel(): string | undefined {
    if (!this.settings) return undefined;
    return this.isDuplicateCopy ? this.settings.printDuplicateCopyLabel : this.settings.printCustomerCopyLabel;
  }
}
