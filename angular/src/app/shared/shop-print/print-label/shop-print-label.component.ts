import { AfterViewInit, Component, ElementRef, Input, OnChanges, QueryList, ViewChildren } from '@angular/core';
import * as JsBarcode from 'jsbarcode';
import { ShopPrintDocumentDto } from '../../../proxy/shop-management/print-templates/models';
import { ShopPrintPaperSize } from '../../../proxy/shop-management/print-settings';

// Product Barcode Label / Batch and Expiry Label - a fundamentally different shape than a
// receipt (small fixed label, possibly repeated many times), so it does NOT reuse
// ShopThermalReceiptComponent/ShopA4DocumentComponent. Supports 58mm single-label printing and a
// simple multi-label grid for A4 sheets (per spec: "58mm single label, Multiple labels per A4
// page" + "Allow user to choose label quantity").
@Component({
  selector: 'shop-print-label',
  standalone: false,
  templateUrl: './shop-print-label.component.html',
  styleUrls: ['./shop-print-label.component.scss'],
})
export class ShopPrintLabelComponent implements AfterViewInit, OnChanges {
  @Input() document!: ShopPrintDocumentDto;
  @Input() copies = 1;

  @ViewChildren('barcodeEl') barcodeElements?: QueryList<ElementRef<SVGElement>>;

  readonly ShopPrintPaperSize = ShopPrintPaperSize;

  get copyIndexes(): number[] {
    const count = Math.max(1, Math.min(this.copies || 1, 200));
    return Array.from({ length: count }, (_, i) => i);
  }

  ngAfterViewInit(): void {
    this.renderBarcodes();
  }

  ngOnChanges(): void {
    setTimeout(() => this.renderBarcodes());
  }

  private renderBarcodes(): void {
    if (!this.document?.barcodeValue || !this.barcodeElements) return;
    this.barcodeElements.forEach((ref) => {
      JsBarcode(ref.nativeElement, this.document.barcodeValue!, {
        format: 'CODE128',
        displayValue: false,
        height: 30,
        margin: 0,
      });
    });
  }
}
