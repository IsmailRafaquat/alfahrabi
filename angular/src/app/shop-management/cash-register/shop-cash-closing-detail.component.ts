import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { saveAs } from 'file-saver';
import html2canvas from 'html2canvas';
import jsPDF from 'jspdf';
import {
  ShopCashClosingDto,
  ShopCashClosingStatus,
  ShopCashDirection,
  ShopCashRegisterService,
  ShopCashRegisterTransactionDto,
  ShopCashTransactionType,
} from '../../proxy/shop-management/cash-registers';
import { ShopSettingDto, ShopSettingService } from '../../proxy/shop-management/settings';

@Component({ selector: 'app-shop-cash-closing-detail', standalone: false, templateUrl: './shop-cash-closing-detail.component.html', styleUrl: './shop-cash-closing-detail.component.scss' })
export class ShopCashClosingDetailComponent implements OnInit {
  private readonly service = inject(ShopCashRegisterService);
  private readonly settingService = inject(ShopSettingService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);

  readonly ShopCashClosingStatus = ShopCashClosingStatus;
  readonly ShopCashDirection = ShopCashDirection;

  @ViewChild('printRef') printRef?: ElementRef<HTMLElement>;

  closing: ShopCashClosingDto | null = null;
  transactions: ShopCashRegisterTransactionDto[] = [];
  shopSetting: ShopSettingDto | null = null;
  loading = false;
  printing = false;
  sharing = false;
  capturing = false;

  ngOnInit(): void {
    this.settingService.get().subscribe(setting => (this.shopSetting = setting));

    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.load(id);
  }

  load(id: string): void {
    this.loading = true;
    this.service.getClosing(id).subscribe(closing => {
      this.closing = closing;
      this.service
        .getTransactions({ cashClosingId: id, sorting: 'transactionDate asc, creationTime asc', maxResultCount: 1000 })
        .pipe(finalize(() => (this.loading = false)))
        .subscribe(result => (this.transactions = result.items || []));
    });
  }

  statusLabel(status: ShopCashClosingStatus): string {
    return '::' + ShopCashClosingStatus[status];
  }

  statusClass(status: ShopCashClosingStatus): string {
    switch (status) {
      case ShopCashClosingStatus.Open: return 'open';
      case ShopCashClosingStatus.Closed: return 'closed';
      case ShopCashClosingStatus.Cancelled: return 'cancelled';
      default: return 'open';
    }
  }

  typeLabel(type: ShopCashTransactionType): string {
    return '::' + ShopCashTransactionType[type];
  }

  directionLabel(direction: ShopCashDirection): string {
    return direction === ShopCashDirection.In ? '::DirectionIn' : '::DirectionOut';
  }

  async print(): Promise<void> {
    if (!this.closing || this.printing) return;
    this.printing = true;
    await this.afterRender();
    this.printing = false;
    window.print();
  }

  async shareOnWhatsApp(): Promise<void> {
    if (!this.closing || this.sharing) return;
    this.sharing = true;
    try {
      const blob = await this.generatePdf();
      const fileName = `Cash-Closing-${this.closing.cashRegisterCode}-${this.formatDateForFileName(this.closing.businessDate)}.pdf`;
      const file = new File([blob], fileName, { type: 'application/pdf' });

      const nav = navigator as Navigator & { canShare?: (data: { files: File[] }) => boolean; share?: (data: { files: File[]; title?: string; text?: string }) => Promise<void> };
      if (nav.canShare && nav.canShare({ files: [file] }) && nav.share) {
        await nav.share({
          files: [file],
          title: '::DailyClosings',
          text: `Cash Closing - ${this.closing.cashRegisterName}`,
        });
      } else {
        saveAs(blob, fileName);
        const message = encodeURIComponent(
          `Cash Closing - ${this.closing.cashRegisterName}\nThe PDF has been downloaded to your device — please attach it here.`,
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

  back(): void {
    this.router.navigate(['/shop-management/cash-register/closings']);
  }

  private afterRender(): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, 100));
  }

  private async generatePdf(): Promise<Blob> {
    this.capturing = true;
    await this.afterRender();

    const element = this.printRef!.nativeElement;
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

  private formatDateForFileName(value: string): string {
    return (value || '').slice(0, 10);
  }
}
