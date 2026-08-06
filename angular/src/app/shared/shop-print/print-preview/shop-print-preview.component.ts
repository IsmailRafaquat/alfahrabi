import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ConfigStateService } from '@abp/ng.core';
import { catchError, forkJoin, map, Observable, of, switchMap } from 'rxjs';
import { ShopCustomerPaymentService } from '../../../proxy/shop-management/customer-payments';
import { ShopPrintTemplateService } from '../../../proxy/shop-management/print-templates/shop-print-template.service';
import { ShopPrintSettingsService } from '../../../proxy/shop-management/print-settings/shop-print-settings.service';
import { ShopPrintDocumentDto } from '../../../proxy/shop-management/print-templates/models';
import { ShopPrintPaperSize } from '../../../proxy/shop-management/print-settings';
import { ShopPrintSettingsDto } from '../../../proxy/shop-management/print-settings/models';
import { ShopPrintLang } from '../models/shop-print-labels';
import { SHOP_PRINT_DOCUMENT_TYPES, SHOP_PRINT_LABEL_DOCUMENT_TYPES } from '../models/shop-print-document-types';
import { ShopPrintService } from '../services/shop-print.service';
import { shareOrOpenWhatsApp } from '../services/shop-whatsapp-share.util';

// Dynamically mounted by ShopPrintService.openPreview() - not declared in any page template. See
// that service for why (avoids per-page modal boilerplate + avoids an Angular CDK dependency).
@Component({
  selector: 'shop-print-preview',
  standalone: false,
  templateUrl: './shop-print-preview.component.html',
  styleUrls: ['./shop-print-preview.component.scss'],
})
export class ShopPrintPreviewComponent implements OnInit {
  @Input() documentType!: string;
  @Input() documentId!: string;
  @Input() initialPaperSize?: ShopPrintPaperSize;
  @Input() initialCopies = 1;
  // Only Cash Closing/Opening Slip, Customer Ledger Statement, and Supplier Ledger Statement set
  // this (Sale Invoice never did, and still doesn't) - see shareOnWhatsApp() below.
  @Input() whatsAppShareMessage?: string;

  @Output() closed = new EventEmitter<void>();

  @ViewChild('printRoot', { read: ElementRef }) printRoot?: ElementRef<HTMLElement>;

  readonly ShopPrintPaperSize = ShopPrintPaperSize;

  document?: ShopPrintDocumentDto;
  settings?: ShopPrintSettingsDto;
  loading = true;
  errorMessage?: string;
  printing = false;
  downloading = false;
  sharing = false;

  lang: ShopPrintLang = 'en';
  paperSize: ShopPrintPaperSize = ShopPrintPaperSize.Thermal80Mm;
  copies = 1;
  showLogo = false;
  showQrCode = false;
  showBatch = true;
  showExpiry = true;
  cashierName?: string;

  get isLabelDocument(): boolean {
    return SHOP_PRINT_LABEL_DOCUMENT_TYPES.includes(this.documentType);
  }

  // Overlays the preview modal's Show Logo/QR/Batch/Expiry toggles onto the loaded settings
  // without mutating the original ShopPrintSettingsDto - shop-print-layout and its children only
  // ever read from this, never from `settings` directly.
  get effectiveSettings(): ShopPrintSettingsDto | undefined {
    if (!this.settings) return undefined;
    return {
      ...this.settings,
      printHeaderLogo: this.showLogo,
      printQrCode: this.showQrCode,
      printBatchNumber: this.showBatch,
      printExpiryDate: this.showExpiry,
    };
  }

  constructor(
    private templateService: ShopPrintTemplateService,
    private printSettingsService: ShopPrintSettingsService,
    private customerPaymentService: ShopCustomerPaymentService,
    private printService: ShopPrintService,
    private configState: ConfigStateService,
  ) {}

  ngOnInit(): void {
    this.copies = this.initialCopies || 1;
    const currentUser = this.configState.getOne('currentUser') as { name?: string; userName?: string } | undefined;
    this.cashierName = currentUser?.name || currentUser?.userName;

    forkJoin({
      document: this.templateService.getPrintDocument(this.documentType, this.documentId),
      settings: this.printSettingsService.get(),
      customerPaymentTotals: this.getCustomerPaymentTotals(),
    }).subscribe({
      next: ({ document, settings, customerPaymentTotals }) => {
        this.document = customerPaymentTotals == null
          ? document
          : {
              ...document,
              totals: {
                ...document.totals,
                netAmount: customerPaymentTotals.totalAmount,
                pendingAmount: customerPaymentTotals.pendingAmount,
              },
            };
        this.settings = settings;
        this.paperSize = this.initialPaperSize ?? document.paperSize ?? settings.defaultPrintPaperSize;
        this.document = { ...this.document, paperSize: this.paperSize };
        this.showLogo = settings.printHeaderLogo;
        this.showQrCode = settings.printQrCode;
        this.showBatch = settings.printBatchNumber;
        this.showExpiry = settings.printExpiryDate;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load print data. You may not have permission to print this document.';
        this.loading = false;
      },
    });
  }

  /**
   * Customer-payment print documents do not currently include the allocated sales' full total or
   * remaining balance. Get the full total from the payment allocations and reuse the outstanding-
   * sale source shown on the detail page for the pending amount. A failure must not block printing.
   */
  private getCustomerPaymentTotals(): Observable<{ totalAmount: number; pendingAmount: number } | undefined> {
    if (this.documentType !== SHOP_PRINT_DOCUMENT_TYPES.CustomerPayment) return of(undefined);

    return this.customerPaymentService.get(this.documentId).pipe(
      switchMap(payment => {
        if (!payment.customerId) return of(undefined);
        const totalAmount = (payment.allocations ?? []).reduce(
          (sum, allocation) => sum + (allocation.grandTotal ?? 0),
          0,
        );
        const allocatedSaleIds = new Set(
          (payment.allocations ?? []).map(allocation => allocation.saleId).filter((id): id is string => !!id),
        );

        return this.customerPaymentService.getOutstandingSales(payment.customerId).pipe(
          map(result => ({
            totalAmount,
            pendingAmount: (result.items ?? [])
              .filter(sale => !!sale.saleId && allocatedSaleIds.has(sale.saleId))
              .reduce((sum, sale) => sum + (sale.pendingAmount ?? 0), 0),
          })),
        );
      }),
      catchError(() => of(undefined)),
    );
  }

  onPaperSizeChange(size: ShopPrintPaperSize): void {
    this.paperSize = size;
    if (this.document) this.document = { ...this.document, paperSize: size };
  }

  toggleLang(lang: ShopPrintLang): void {
    this.lang = lang;
  }

  async print(): Promise<void> {
    if (!this.document) return;
    this.printing = true;
    try {
      await this.printService.print();
    } finally {
      this.printing = false;
    }
  }

  async downloadPdf(): Promise<void> {
    if (!this.document || !this.printRoot) return;
    this.downloading = true;
    try {
      const fileName = `${this.document.documentType}-${this.document.documentNumber}.pdf`;
      await this.printService.downloadPdf(this.printRoot.nativeElement, this.document, fileName);
    } finally {
      this.downloading = false;
    }
  }

  async shareOnWhatsApp(): Promise<void> {
    if (!this.document || !this.printRoot || !this.whatsAppShareMessage) return;
    this.sharing = true;
    try {
      const fileName = `${this.document.documentType}-${this.document.documentNumber}.pdf`;
      const blob = await this.printService.downloadPdf(this.printRoot.nativeElement, this.document, fileName);
      await shareOrOpenWhatsApp(blob, fileName, this.whatsAppShareMessage);
    } finally {
      this.sharing = false;
    }
  }

  close(): void {
    this.closed.emit();
  }
}
