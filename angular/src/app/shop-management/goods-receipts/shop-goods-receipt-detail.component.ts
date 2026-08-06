import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { PurchaseReturnService } from '../purchase-returns/purchase-return.service';
import {
  ShopGoodsReceiptDto,
  ShopGoodsReceiptPaymentStatus,
  ShopGoodsReceiptService,
  ShopGoodsReceiptStatus,
} from '../../proxy/shop-management/goods-receipts';
import { ShopPrintService } from '../../shared/shop-print/services/shop-print.service';
import { SHOP_PRINT_DOCUMENT_TYPES } from '../../shared/shop-print/models/shop-print-document-types';

@Component({ selector: 'app-shop-goods-receipt-detail', standalone: false, templateUrl: './shop-goods-receipt-detail.component.html', styleUrl: './shop-goods-receipt-detail.component.scss' })
export class ShopGoodsReceiptDetailComponent implements OnInit {
  private readonly service = inject(ShopGoodsReceiptService);
  private readonly printService = inject(ShopPrintService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);
  private readonly purchaseReturns = inject(PurchaseReturnService);

  readonly ShopGoodsReceiptStatus = ShopGoodsReceiptStatus;
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.GoodsReceipts.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.GoodsReceipts.Delete');
  readonly canComplete = this.permissions.getGrantedPolicy('ShopManagement.GoodsReceipts.Complete');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.GoodsReceipts.Cancel');
  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.GoodsReceipts.ViewCost');
  readonly canViewStockTransactions = this.permissions.getGrantedPolicy('ShopManagement.StockTransactions');
  readonly canViewPaymentAmount = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.ViewAmount');
  readonly canCreateSupplierPayment = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.Create');
  readonly canCreatePurchaseReturn = this.permissions.getGrantedPolicy('ShopManagement.PurchaseReturns.Create');
  readonly canPrint = this.permissions.getGrantedPolicy('ShopManagement.Print.Purchases');

  id!: string;
  dto?: ShopGoodsReceiptDto;
  loading = false;
  actionInProgress = false;
  hasReturnableItems = false;

  cancelModalOpen = false;
  readonly cancelForm = this.fb.group({ cancellationReason: ['', [Validators.required, Validators.maxLength(500)]] });

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service
      .get(this.id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => {
        this.dto = dto;
        this.hasReturnableItems = false;
        if (dto.status === ShopGoodsReceiptStatus.Completed && this.canCreatePurchaseReturn) {
          this.purchaseReturns.getReceipt(this.id).subscribe({
            next: x => this.hasReturnableItems = !!x.items?.length,
            error: () => this.hasReturnableItems = false,
          });
        }
      });
  }

  statusLabel(status: ShopGoodsReceiptStatus): string {
    return '::' + ShopGoodsReceiptStatus[status];
  }

  statusClass(status: ShopGoodsReceiptStatus): string {
    switch (status) {
      case ShopGoodsReceiptStatus.Draft: return 'draft';
      case ShopGoodsReceiptStatus.Completed: return 'completed';
      case ShopGoodsReceiptStatus.Cancelled: return 'cancelled';
      default: return 'draft';
    }
  }

  paymentStatusLabel(status?: ShopGoodsReceiptPaymentStatus): string {
    return '::' + ShopGoodsReceiptPaymentStatus[status ?? ShopGoodsReceiptPaymentStatus.Unpaid];
  }

  paymentStatusClass(status?: ShopGoodsReceiptPaymentStatus): string {
    switch (status) {
      case ShopGoodsReceiptPaymentStatus.Unpaid: return 'unpaid';
      case ShopGoodsReceiptPaymentStatus.PartiallyPaid: return 'partially-paid';
      case ShopGoodsReceiptPaymentStatus.Paid: return 'paid';
      default: return 'unpaid';
    }
  }

  canMakeSupplierPayment(): boolean {
    return this.dto?.status === ShopGoodsReceiptStatus.Completed && (this.dto?.pendingAmount ?? 0) > 0;
  }

  makeSupplierPayment(): void {
    this.router.navigate(['/shop-management/supplier-payments/create'], { queryParams: { goodsReceiptId: this.id } });
  }
  createPurchaseReturn(): void { this.router.navigate(['/shop-management/purchase-returns/create', this.id]); }

  edit(): void {
    this.router.navigate(['/shop-management/goods-receipts', this.id, 'edit']);
  }

  print(): void {
    this.printService.openPreview({ documentType: SHOP_PRINT_DOCUMENT_TYPES.GoodsReceipt, documentId: this.id });
  }

  back(): void {
    this.router.navigate(['/shop-management/goods-receipts']);
  }

  viewStockTransactions(): void {
    this.router.navigate(['/shop-management/stock-transactions'], { queryParams: { referenceNumber: this.dto?.goodsReceiptNumber } });
  }

  remove(): void {
    this.confirmation.warn('::ConfirmDeleteGoodsReceipt', this.dto!.goodsReceiptNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(this.id).subscribe({
        next: () => {
          this.toaster.success('::GoodsReceiptDeletedSuccessfully');
          this.back();
        },
        error: e => this.showError(e),
      });
    });
  }

  complete(): void {
    this.confirmation.warn('::ConfirmCompleteGoodsReceipt', this.dto!.goodsReceiptNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .complete(this.id)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: dto => {
            this.dto = dto;
            this.toaster.success('::GoodsReceiptCompletedSuccessfully');
          },
          error: e => this.showError(e),
        });
    });
  }

  openCancel(): void {
    this.cancelForm.reset();
    this.cancelModalOpen = true;
  }

  confirmCancel(): void {
    this.cancelForm.markAllAsTouched();
    if (this.cancelForm.invalid) return;
    this.actionInProgress = true;
    this.service
      .cancel(this.id, this.cancelForm.getRawValue() as { cancellationReason: string })
      .pipe(finalize(() => (this.actionInProgress = false)))
      .subscribe({
        next: dto => {
          this.dto = dto;
          this.cancelModalOpen = false;
          this.toaster.success('::GoodsReceiptCancelledSuccessfully');
        },
        error: e => this.showError(e),
      });
  }

  private showError(error: any): void {
    this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError');
  }
}
