import { AfterViewInit, Component, ElementRef, Input, OnChanges, ViewChild } from '@angular/core';
import { toCanvas } from 'qrcode';
import * as JsBarcode from 'jsbarcode';
import { ShopPrintLabelSet } from '../models/shop-print-labels';

// Renders the optional QR code / barcode from a plain reference-string value (never a secret,
// never an image from the backend - see ShopPrintDocumentDto.QrCodeValue/BarcodeValue). Uses the
// 'qrcode' and 'jsbarcode' npm packages, the only QR/barcode libraries in this codebase.
@Component({
  selector: 'shop-print-footer',
  standalone: false,
  templateUrl: './shop-print-footer.component.html',
  styleUrls: ['./shop-print-footer.component.scss'],
})
export class ShopPrintFooterComponent implements AfterViewInit, OnChanges {
  @Input() footerMessage?: string;
  @Input() termsAndConditions?: string;
  @Input() qrCodeValue?: string;
  @Input() barcodeValue?: string;
  @Input() labels!: ShopPrintLabelSet;

  @ViewChild('qrCanvas') qrCanvas?: ElementRef<HTMLCanvasElement>;
  @ViewChild('barcodeSvg') barcodeSvg?: ElementRef<SVGElement>;

  private rendered = false;

  ngAfterViewInit(): void {
    this.render();
  }

  ngOnChanges(): void {
    this.render();
  }

  private render(): void {
    // Deferred one tick so *ngIf-gated canvas/svg elements exist in the DOM before rendering.
    setTimeout(() => {
      this.renderQrCode();
      this.renderBarcode();
    });
  }

  private renderQrCode(): void {
    if (!this.qrCodeValue || !this.qrCanvas) return;
    toCanvas(this.qrCanvas.nativeElement, this.qrCodeValue, { width: 96, margin: 0 }, () => {});
  }

  private renderBarcode(): void {
    if (!this.barcodeValue || !this.barcodeSvg) return;
    JsBarcode(this.barcodeSvg.nativeElement, this.barcodeValue, {
      format: 'CODE128',
      displayValue: true,
      fontSize: 12,
      height: 40,
      margin: 0,
    });
  }
}
