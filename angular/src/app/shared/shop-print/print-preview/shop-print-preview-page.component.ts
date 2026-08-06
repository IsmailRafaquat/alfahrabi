import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { isValidShopPrintDocumentType } from '../models/shop-print-document-types';

// Thin route wrapper for /shop-management/print-preview/:documentType/:id - reads route params,
// validates documentType against the frontend allowlist BEFORE ever rendering the preview
// component (defense in depth alongside the backend's own allowlist check), then embeds
// <shop-print-preview> directly as a full page instead of a modal. The modal-launched path
// (ShopPrintService.openPreview) renders the exact same ShopPrintPreviewComponent, just
// instantiated dynamically instead of through routing - see ShopPrintService for that path.
@Component({
  selector: 'shop-print-preview-page',
  standalone: false,
  template: `
    <div class="sp-page-wrap" *ngIf="documentType && documentId">
      <shop-print-preview [documentType]="documentType" [documentId]="documentId" (closed)="onClosed()"></shop-print-preview>
    </div>
    <div class="sp-page-invalid alert alert-danger m-3" *ngIf="!documentType || !documentId">
      Invalid or unsupported print document type.
    </div>
  `,
})
export class ShopPrintPreviewPageComponent implements OnInit {
  documentType?: string;
  documentId?: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit(): void {
    const type = this.route.snapshot.paramMap.get('documentType');
    const id = this.route.snapshot.paramMap.get('id');
    if (isValidShopPrintDocumentType(type) && id) {
      this.documentType = type;
      this.documentId = id;
    }
  }

  onClosed(): void {
    if (window.history.length > 1) {
      window.history.back();
    } else {
      this.router.navigateByUrl('/shop-management/dashboard');
    }
  }
}
