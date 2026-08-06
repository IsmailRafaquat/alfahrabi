import { ApplicationRef, ComponentRef, EmbeddedViewRef, EnvironmentInjector, Injectable, Injector, createComponent } from '@angular/core';
import { saveAs } from 'file-saver';
import html2canvas from 'html2canvas';
import jsPDF from 'jspdf';
import { ShopPrintDocumentDto } from '../../../proxy/shop-management/print-templates/models';
import { ShopPrintPaperSize } from '../../../proxy/shop-management/print-settings';
import { ShopPrintRequest } from '../models/shop-print-request';
import { isValidShopPrintDocumentType } from '../models/shop-print-document-types';
import { ShopPrintPreviewComponent } from '../print-preview/shop-print-preview.component';

// openPreview/print/downloadPdf per the required frontend service surface. openPreview
// dynamically mounts ShopPrintPreviewComponent onto document.body via Angular's createComponent
// API (no Angular CDK dependency, no per-page modal markup needed - any page can call
// shopPrintService.openPreview({...}) directly). print()/downloadPdf() take the currently
// rendered print-root element explicitly rather than tracking hidden global state, since only
// ShopPrintPreviewComponent (which owns that DOM) ever calls them.
@Injectable({ providedIn: 'root' })
export class ShopPrintService {
  private activePreviewRef?: ComponentRef<ShopPrintPreviewComponent>;

  constructor(
    private appRef: ApplicationRef,
    private environmentInjector: EnvironmentInjector,
  ) {}

  openPreview(request: ShopPrintRequest, parentInjector?: Injector): void {
    if (!isValidShopPrintDocumentType(request.documentType)) {
      throw new Error(`Unknown or disallowed print document type: ${request.documentType}`);
    }

    this.closeActivePreview();

    const componentRef = createComponent(ShopPrintPreviewComponent, {
      environmentInjector: this.environmentInjector,
      elementInjector: parentInjector,
    });

    componentRef.instance.documentType = request.documentType;
    componentRef.instance.documentId = request.documentId;
    componentRef.instance.initialPaperSize = request.paperSize;
    componentRef.instance.initialCopies = request.copies ?? 1;
    componentRef.instance.whatsAppShareMessage = request.whatsAppShareMessage;
    componentRef.instance.closed.subscribe(() => this.closeActivePreview());

    this.appRef.attachView(componentRef.hostView);
    const domRoot = (componentRef.hostView as EmbeddedViewRef<unknown>).rootNodes[0] as HTMLElement;
    document.body.appendChild(domRoot);

    this.activePreviewRef = componentRef;
  }

  private closeActivePreview(): void {
    if (!this.activePreviewRef) return;
    this.appRef.detachView(this.activePreviewRef.hostView);
    this.activePreviewRef.destroy();
    this.activePreviewRef = undefined;
  }

  /**
   * Waits for the print root to finish rendering (fonts + one animation frame, since QR/barcode
   * canvases and web fonts both render asynchronously) before invoking the browser print dialog.
   */
  async print(): Promise<void> {
    if ((document as any).fonts?.ready) {
      await (document as any).fonts.ready;
    }
    await new Promise((resolve) => requestAnimationFrame(resolve));
    window.print();
  }

  /**
   * Never forces an A4 page for thermal paper sizes - the PDF page format is derived from the
   * rendered content's own pixel dimensions for Thermal58Mm/Thermal80Mm, and only uses a fixed A4
   * page for ShopPrintPaperSize.A4.
   */
  async downloadPdf(element: HTMLElement, doc: ShopPrintDocumentDto, fileName: string): Promise<Blob> {
    const canvas = await html2canvas(element, { scale: 2, backgroundColor: '#ffffff' });
    const imgData = canvas.toDataURL('image/png');

    let pdf: jsPDF;

    if (doc.paperSize === ShopPrintPaperSize.A4) {
      pdf = new jsPDF({ orientation: 'portrait', unit: 'pt', format: 'a4' });
      const pageWidth = pdf.internal.pageSize.getWidth();
      const pageHeight = pdf.internal.pageSize.getHeight();
      const imgWidth = pageWidth;
      const imgHeight = (canvas.height * imgWidth) / canvas.width;
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
    } else {
      const widthMm = doc.paperSize === ShopPrintPaperSize.Thermal58Mm ? 48 : 72;
      const pxPerMm = canvas.width / widthMm;
      const heightMm = canvas.height / pxPerMm;
      pdf = new jsPDF({ orientation: 'portrait', unit: 'mm', format: [widthMm, Math.max(heightMm, 40)] });
      pdf.addImage(imgData, 'PNG', 0, 0, widthMm, heightMm);
    }

    const blob = pdf.output('blob');
    saveAs(blob, fileName);
    return blob;
  }
}
