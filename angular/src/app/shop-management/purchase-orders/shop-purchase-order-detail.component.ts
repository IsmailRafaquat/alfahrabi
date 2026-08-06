import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { ShopPurchaseOrderDto, ShopPurchaseOrderService, ShopPurchaseOrderStatus } from '../../proxy/shop-management/purchase-orders';
import { ShopPrintService } from '../../shared/shop-print/services/shop-print.service';
import { SHOP_PRINT_DOCUMENT_TYPES } from '../../shared/shop-print/models/shop-print-document-types';

@Component({ selector: 'app-shop-purchase-order-detail', standalone: false, templateUrl: './shop-purchase-order-detail.component.html', styleUrl: './shop-purchase-order-detail.component.scss' })
export class ShopPurchaseOrderDetailComponent implements OnInit {
  private readonly service = inject(ShopPurchaseOrderService);
  private readonly printService = inject(ShopPrintService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);

  readonly ShopPurchaseOrderStatus = ShopPurchaseOrderStatus;
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.PurchaseOrders.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.PurchaseOrders.Delete');
  readonly canSubmit = this.permissions.getGrantedPolicy('ShopManagement.PurchaseOrders.Submit');
  readonly canApprove = this.permissions.getGrantedPolicy('ShopManagement.PurchaseOrders.Approve');
  readonly canReject = this.permissions.getGrantedPolicy('ShopManagement.PurchaseOrders.Reject');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.PurchaseOrders.Cancel');
  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.PurchaseOrders.ViewCost');
  readonly canReceiveGoods = this.permissions.getGrantedPolicy('ShopManagement.GoodsReceipts.Create');
  readonly canPrint = this.permissions.getGrantedPolicy('ShopManagement.Print.Purchases');

  id!: string;
  dto?: ShopPurchaseOrderDto;
  loading = false;
  actionInProgress = false;

  rejectModalOpen = false;
  cancelModalOpen = false;
  readonly rejectForm = this.fb.group({ rejectionReason: ['', [Validators.required, Validators.maxLength(500)]] });
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
      .subscribe(dto => (this.dto = dto));
  }

  statusLabel(status: ShopPurchaseOrderStatus): string {
    return '::' + ShopPurchaseOrderStatus[status];
  }

  statusClass(status: ShopPurchaseOrderStatus): string {
    switch (status) {
      case ShopPurchaseOrderStatus.Draft: return 'draft';
      case ShopPurchaseOrderStatus.PendingApproval: return 'pending';
      case ShopPurchaseOrderStatus.Approved: return 'approved';
      case ShopPurchaseOrderStatus.Rejected: return 'rejected';
      case ShopPurchaseOrderStatus.PartiallyReceived: return 'partial';
      case ShopPurchaseOrderStatus.FullyReceived: return 'received';
      case ShopPurchaseOrderStatus.Cancelled: return 'cancelled';
      case ShopPurchaseOrderStatus.Closed: return 'closed';
      default: return 'draft';
    }
  }

  edit(): void {
    this.router.navigate(['/shop-management/purchase-orders', this.id, 'edit']);
  }

  receiveGoods(): void {
    this.router.navigate(['/shop-management/goods-receipts/create', this.id]);
  }

  print(): void {
    this.printService.openPreview({ documentType: SHOP_PRINT_DOCUMENT_TYPES.PurchaseOrder, documentId: this.id });
  }

  back(): void {
    this.router.navigate(['/shop-management/purchase-orders']);
  }

  remove(): void {
    this.confirmation.warn('::ConfirmDeletePurchaseOrder', this.dto!.purchaseOrderNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(this.id).subscribe({
        next: () => {
          this.toaster.success('::PurchaseOrderDeletedSuccessfully');
          this.back();
        },
        error: e => this.showError(e),
      });
    });
  }

  submit(): void {
    this.confirmation.warn('::ConfirmSubmitPurchaseOrder', this.dto!.purchaseOrderNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .submit(this.id)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: dto => {
            this.dto = dto;
            this.toaster.success('::PurchaseOrderSubmittedSuccessfully');
          },
          error: e => this.showError(e),
        });
    });
  }

  approve(): void {
    this.confirmation.warn('::ConfirmApprovePurchaseOrder', this.dto!.purchaseOrderNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .approve(this.id)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: dto => {
            this.dto = dto;
            this.toaster.success('::PurchaseOrderApprovedSuccessfully');
          },
          error: e => this.showError(e),
        });
    });
  }

  openReject(): void {
    this.rejectForm.reset();
    this.rejectModalOpen = true;
  }

  confirmReject(): void {
    this.rejectForm.markAllAsTouched();
    if (this.rejectForm.invalid) return;
    this.actionInProgress = true;
    this.service
      .reject(this.id, this.rejectForm.getRawValue() as { rejectionReason: string })
      .pipe(finalize(() => (this.actionInProgress = false)))
      .subscribe({
        next: dto => {
          this.dto = dto;
          this.rejectModalOpen = false;
          this.toaster.success('::PurchaseOrderRejectedSuccessfully');
        },
        error: e => this.showError(e),
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
          this.toaster.success('::PurchaseOrderCancelledSuccessfully');
        },
        error: e => this.showError(e),
      });
  }

  private showError(error: any): void {
    this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError');
  }
}
