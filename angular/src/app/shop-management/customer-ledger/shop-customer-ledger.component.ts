import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { saveAs } from 'file-saver';
import html2canvas from 'html2canvas';
import jsPDF from 'jspdf';
import {
  ShopCustomerLedgerDto,
  ShopCustomerLedgerReferenceType,
  ShopCustomerLedgerService,
  ShopCustomerStatementDto,
  shopCustomerLedgerReferenceTypeOptions,
} from '../../proxy/shop-management/customer-ledger';
import { ShopCustomerLookupDto, ShopCustomerService } from '../../proxy/shop-management/customers';

@Component({ selector: 'app-shop-customer-ledger', standalone: false, templateUrl: './shop-customer-ledger.component.html', styleUrl: './shop-customer-ledger.component.scss' })
export class ShopCustomerLedgerComponent implements OnInit {
  private readonly service = inject(ShopCustomerLedgerService);
  private readonly customerService = inject(ShopCustomerService);
  private readonly route = inject(ActivatedRoute);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly ShopCustomerLedgerReferenceType = ShopCustomerLedgerReferenceType;
  readonly referenceTypeOptions = shopCustomerLedgerReferenceTypeOptions;
  readonly canViewAmounts = this.permissions.getGrantedPolicy('ShopManagement.CustomerLedger.ViewAmounts');
  readonly canPrint = this.permissions.getGrantedPolicy('ShopManagement.CustomerLedger.Print');

  customers: ShopCustomerLookupDto[] = [];
  customerId = '';
  dateFrom: string | null = null;
  dateTo: string | null = null;
  referenceType: ShopCustomerLedgerReferenceType | '' = '';
  filter = '';

  @ViewChild('statementRef') statementRef?: ElementRef<HTMLElement>;

  ledger?: ShopCustomerLedgerDto;
  statement?: ShopCustomerStatementDto;
  loading = false;
  printing = false;
  sharing = false;
  capturing = false;
  tooltipLang: 'en' | 'ur' = 'en';

  printLangModalOpen = false;
  printLang: 'en' | 'ur' = 'en';
  private pendingAction: 'print' | 'share' = 'print';

  private readonly statementLabelsByLang = {
    en: {
      statementTitle: 'Customer Statement',
      date: 'Date',
      referenceNumber: 'Reference Number',
      description: 'Description',
      debit: 'Debit',
      credit: 'Credit',
      runningBalance: 'Running Balance',
      totalDebit: 'Total Debit',
      totalCredit: 'Total Credit',
      closingBalance: 'Closing Balance',
      receivableAmount: 'Receivable Amount',
      advanceAmount: 'Advance Amount',
      totalSaleReturns: 'Total Sale Returns',
      totalRefunds: 'Total Refunds',
      totalCustomerCredits: 'Total Customer Credits',
    },
    ur: {
      statementTitle: 'کسٹمر اسٹیٹمنٹ',
      date: 'تاریخ',
      referenceNumber: 'حوالہ نمبر',
      description: 'تفصیل',
      debit: 'ڈیبٹ',
      credit: 'کریڈٹ',
      runningBalance: 'چلتا بیلنس',
      totalDebit: 'کل ڈیبٹ',
      totalCredit: 'کل کریڈٹ',
      closingBalance: 'اختتامی بیلنس',
      receivableAmount: 'وصولی رقم',
      advanceAmount: 'ایڈوانس رقم',
      totalSaleReturns: 'کل سیل ریٹرن',
      totalRefunds: 'کل ریفنڈ',
      totalCustomerCredits: 'کل کسٹمر کریڈٹ',
    },
  };

  get statementLabels() {
    return this.statementLabelsByLang[this.printLang];
  }

  ngOnInit(): void {
    this.customerService.getLookup().subscribe(result => (this.customers = result.items || []));

    const routeCustomerId = this.route.snapshot.paramMap.get('customerId');
    if (routeCustomerId) {
      this.customerId = routeCustomerId;
      this.load();
    }
  }

  onCustomerChange(): void {
    this.ledger = undefined;
    if (this.customerId) this.load();
  }

  load(): void {
    if (!this.customerId) return;
    this.loading = true;
    this.service
      .getLedger({
        customerId: this.customerId,
        dateFrom: this.dateFrom || undefined,
        dateTo: this.dateTo || undefined,
        referenceType: this.referenceType === '' ? undefined : this.referenceType,
        filter: this.filter || undefined,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: dto => (this.ledger = dto),
        error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
      });
  }

  reset(): void {
    this.dateFrom = null;
    this.dateTo = null;
    this.referenceType = '';
    this.filter = '';
    if (this.customerId) this.load();
  }

  referenceTypeLabel(type: ShopCustomerLedgerReferenceType): string {
    return '::' + ShopCustomerLedgerReferenceType[type];
  }

  referenceTypeClass(type: ShopCustomerLedgerReferenceType): string {
    switch (type) {
      case ShopCustomerLedgerReferenceType.OpeningBalance: return 'opening';
      case ShopCustomerLedgerReferenceType.Sale: return 'sale';
      case ShopCustomerLedgerReferenceType.CustomerPayment: return 'payment';
      case ShopCustomerLedgerReferenceType.SaleReturn: return 'sale-return';
      default: return 'opening';
    }
  }

  openPrintLanguageDialog(action: 'print' | 'share'): void {
    if (!this.customerId) return;
    this.pendingAction = action;
    this.printLangModalOpen = true;
  }

  selectLanguage(lang: 'en' | 'ur'): void {
    this.printLang = lang;
    this.printLangModalOpen = false;
    const isPrint = this.pendingAction === 'print';
    if (isPrint) this.printing = true;
    else this.sharing = true;

    this.service
      .getStatement({
        customerId: this.customerId,
        dateFrom: this.dateFrom || undefined,
        dateTo: this.dateTo || undefined,
      })
      .subscribe({
        next: async statement => {
          this.statement = statement;
          // Let Angular render the freshly-fetched statement before we print/capture it.
          await this.afterRender();
          if (isPrint) {
            this.printing = false;
            window.print();
          } else {
            await this.shareOnWhatsApp();
          }
        },
        error: e => {
          if (isPrint) this.printing = false;
          else this.sharing = false;
          this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError');
        },
      });
  }

  private afterRender(): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, 100));
  }

  private async shareOnWhatsApp(): Promise<void> {
    try {
      const blob = await this.generateStatementPdf();
      const fileName = `Customer-Statement-${this.statement?.customerCode || 'statement'}.pdf`;
      const file = new File([blob], fileName, { type: 'application/pdf' });

      const nav = navigator as Navigator & { canShare?: (data: { files: File[] }) => boolean; share?: (data: { files: File[]; title?: string; text?: string }) => Promise<void> };
      if (nav.canShare && nav.canShare({ files: [file] }) && nav.share) {
        await nav.share({
          files: [file],
          title: '::CustomerStatement',
          text: `Customer Statement - ${this.statement?.customerName}`,
        });
      } else {
        // Desktop browsers can't attach a file to a WhatsApp link, so download the PDF
        // and open a WhatsApp chat with a text message the user can attach it to manually.
        saveAs(blob, fileName);
        const message = encodeURIComponent(
          `Customer Statement - ${this.statement?.customerName}\nThe PDF has been downloaded to your device — please attach it here.`,
        );
        window.open(`https://wa.me/?text=${message}`, '_blank');
      }
    } catch (e: any) {
      if (e?.name !== 'AbortError') {
        this.toaster.error(e?.message || '::UnexpectedError');
      }
    } finally {
      this.sharing = false;
    }
  }

  private async generateStatementPdf(): Promise<Blob> {
    this.capturing = true;
    await this.afterRender();

    const element = this.statementRef!.nativeElement;
    const canvas = await html2canvas(element, { scale: 2, backgroundColor: '#ffffff' });

    this.capturing = false;

    const pdf = new jsPDF({ orientation: 'portrait', unit: 'pt', format: 'a4' });
    const pageWidth = pdf.internal.pageSize.getWidth();
    const pageHeight = pdf.internal.pageSize.getHeight();
    const imgWidth = pageWidth;
    const imgHeight = (canvas.height * imgWidth) / canvas.width;
    const imgData = canvas.toDataURL('image/png');

    let heightLeft = imgHeight;
    let position = 0;
    pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
    heightLeft -= pageHeight;

    while (heightLeft > 0) {
      position = heightLeft - imgHeight;
      pdf.addPage();
      pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
      heightLeft -= pageHeight;
    }

    return pdf.output('blob');
  }
}
