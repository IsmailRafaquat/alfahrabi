import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  ShopStockAdjustmentDto,
  ShopStockAdjustmentReason,
  ShopStockAdjustmentService,
  ShopStockAdjustmentStatus,
  ShopStockAdjustmentType,
} from '../../proxy/shop-management/stock-adjustments';

@Component({ selector: 'app-shop-stock-adjustment-detail', standalone: false, templateUrl: './shop-stock-adjustment-detail.component.html', styleUrl: './shop-stock-adjustment-detail.component.scss' })
export class ShopStockAdjustmentDetailComponent implements OnInit {
  private readonly service = inject(ShopStockAdjustmentService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);

  readonly ShopStockAdjustmentStatus = ShopStockAdjustmentStatus;
  readonly ShopStockAdjustmentType = ShopStockAdjustmentType;
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.StockAdjustments.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.StockAdjustments.Delete');
  readonly canPost = this.permissions.getGrantedPolicy('ShopManagement.StockAdjustments.Post');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.StockAdjustments.Cancel');
  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.StockAdjustments.ViewCost');
  readonly canViewStockTransactions = this.permissions.getGrantedPolicy('ShopManagement.StockTransactions');

  id!: string;
  dto?: ShopStockAdjustmentDto;
  loading = false;
  actionInProgress = false;

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
      .subscribe(dto => (this.dto = dto));
  }

  statusLabel(status?: ShopStockAdjustmentStatus): string {
    return status == null ? '' : '::' + ShopStockAdjustmentStatus[status];
  }

  statusClass(status?: ShopStockAdjustmentStatus): string {
    switch (status) {
      case ShopStockAdjustmentStatus.Draft: return 'draft';
      case ShopStockAdjustmentStatus.Posted: return 'posted';
      case ShopStockAdjustmentStatus.Cancelled: return 'cancelled';
      default: return 'draft';
    }
  }

  reasonLabel(reason?: ShopStockAdjustmentReason): string {
    return reason == null ? '' : '::' + ShopStockAdjustmentReason[reason];
  }

  typeLabel(type?: ShopStockAdjustmentType): string {
    return type == null ? '' : '::' + ShopStockAdjustmentType[type];
  }

  edit(): void {
    this.router.navigate(['/shop-management/stock-adjustments', this.id, 'edit']);
  }

  back(): void {
    this.router.navigate(['/shop-management/stock-adjustments']);
  }

  viewStockTransactions(): void {
    this.router.navigate(['/shop-management/stock-transactions'], { queryParams: { referenceNumber: this.dto!.adjustmentNumber } });
  }

  remove(): void {
    this.confirmation.warn('::ConfirmDeleteStockAdjustment', this.dto!.adjustmentNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(this.id).subscribe({
        next: () => {
          this.toaster.success('::StockAdjustmentDeletedSuccessfully');
          this.back();
        },
        error: e => this.showError(e),
      });
    });
  }

  post(): void {
    this.confirmation.warn('::ConfirmPostStockAdjustment', this.dto!.adjustmentNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .post(this.id)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: dto => {
            this.dto = dto;
            this.toaster.success('::StockAdjustmentPostedSuccessfully');
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
          this.toaster.success('::StockAdjustmentCancelledSuccessfully');
        },
        error: e => this.showError(e),
      });
  }

  private showError(error: any): void {
    this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError');
  }
}
