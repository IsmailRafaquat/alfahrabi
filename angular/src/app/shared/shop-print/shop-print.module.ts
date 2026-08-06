import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../shared.module';
import { ShopPrintCurrencyPipe } from './models/shop-print-currency.pipe';
import { ShopPrintHeaderComponent } from './print-header/shop-print-header.component';
import { ShopPrintFooterComponent } from './print-footer/shop-print-footer.component';
import { ShopPrintItemTableComponent } from './print-lines/shop-print-item-table.component';
import { ShopPrintTotalsComponent } from './print-totals/shop-print-totals.component';
import { ShopPrintPaymentSummaryComponent } from './payment-summary/shop-print-payment-summary.component';
import { ShopPrintLabelComponent } from './print-label/shop-print-label.component';
import { ShopPrintReportSummaryComponent } from './report-summary/shop-print-report-summary.component';
import { ShopThermalReceiptComponent } from './thermal-receipt/shop-thermal-receipt.component';
import { ShopA4DocumentComponent } from './a4-document/shop-a4-document.component';
import { ShopPrintLayoutComponent } from './print-layout/shop-print-layout.component';
import { ShopPrintPreviewComponent } from './print-preview/shop-print-preview.component';
import { ShopPrintPreviewPageComponent } from './print-preview/shop-print-preview-page.component';

const COMPONENTS = [
  ShopPrintCurrencyPipe,
  ShopPrintHeaderComponent,
  ShopPrintFooterComponent,
  ShopPrintItemTableComponent,
  ShopPrintTotalsComponent,
  ShopPrintPaymentSummaryComponent,
  ShopPrintLabelComponent,
  ShopPrintReportSummaryComponent,
  ShopThermalReceiptComponent,
  ShopA4DocumentComponent,
  ShopPrintLayoutComponent,
  ShopPrintPreviewComponent,
  ShopPrintPreviewPageComponent,
];

@NgModule({
  declarations: [...COMPONENTS],
  imports: [CommonModule, FormsModule, SharedModule, RouterModule],
  exports: [CommonModule, FormsModule, SharedModule, ...COMPONENTS],
})
export class ShopPrintModule {}
