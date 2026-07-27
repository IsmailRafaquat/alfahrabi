import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  ShopStockCountDto,
  ShopStockCountPostingPreviewDto,
  ShopStockCountService,
  ShopStockCountStatus,
} from '../../proxy/shop-management/stock-counts';
import { ShopStockAdjustmentType } from '../../proxy/shop-management/stock-adjustments';

@Component({ selector: 'app-shop-stock-count-detail', standalone: false, templateUrl: './shop-stock-count-detail.component.html', styleUrl: './shop-stock-count-detail.component.scss' })
export class ShopStockCountDetailComponent implements OnInit {
  private readonly service = inject(ShopStockCountService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);

  readonly ShopStockCountStatus = ShopStockCountStatus;
  readonly ShopStockAdjustmentType = ShopStockAdjustmentType;
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.StockCounts.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.StockCounts.Delete');
  readonly canStart = this.permissions.getGrantedPolicy('ShopManagement.StockCounts.Start');
  readonly canPost = this.permissions.getGrantedPolicy('ShopManagement.StockCounts.Post');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.StockCounts.Cancel');
  readonly canViewStockTransactions = this.permissions.getGrantedPolicy('ShopManagement.StockTransactions');
  readonly canViewStockAdjustments = this.permissions.getGrantedPolicy('ShopManagement.StockAdjustments');

  id!: string;
  dto?: ShopStockCountDto;
  loading = false;
  actionInProgress = false;

  cancelModalOpen = false;
  readonly cancelForm = this.fb.group({ cancellationReason: ['', [Validators.required, Validators.maxLength(500)]] });

  postModalOpen = false;
  postPreview?: ShopStockCountPostingPreviewDto;
  postPreviewLoading = false;

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

  statusLabel(status?: ShopStockCountStatus): string {
    return status == null ? '' : '::' + ShopStockCountStatus[status];
  }

  statusClass(status?: ShopStockCountStatus): string {
    switch (status) {
      case ShopStockCountStatus.Draft: return 'draft';
      case ShopStockCountStatus.InProgress: return 'inprogress';
      case ShopStockCountStatus.Counted: return 'counted';
      case ShopStockCountStatus.Posted: return 'posted';
      case ShopStockCountStatus.Cancelled: return 'cancelled';
      default: return 'draft';
    }
  }

  scopeLabel(scope?: number): string {
    return scope == null ? '' : '::' + ['AllProducts', 'SelectedProducts', 'Category'][scope];
  }

  isCancellable(): boolean {
    const status = this.dto?.status;
    return status === ShopStockCountStatus.Draft || status === ShopStockCountStatus.InProgress || status === ShopStockCountStatus.Counted;
  }

  edit(): void {
    this.router.navigate(['/shop-management/stock-counts', this.id, 'edit']);
  }

  count(): void {
    this.router.navigate(['/shop-management/stock-counts', this.id, 'count']);
  }

  back(): void {
    this.router.navigate(['/shop-management/stock-counts']);
  }

  viewGeneratedStockAdjustment(): void {
    if (this.dto?.generatedStockAdjustmentId) this.router.navigate(['/shop-management/stock-adjustments', this.dto.generatedStockAdjustmentId]);
  }

  viewStockTransactions(): void {
    this.router.navigate(['/shop-management/stock-transactions'], { queryParams: { referenceNumber: this.dto!.generatedStockAdjustmentNumber } });
  }

  remove(): void {
    this.confirmation.warn('::ConfirmDeleteStockCount', this.dto!.stockCountNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(this.id).subscribe({
        next: () => {
          this.toaster.success('::StockCountDeletedSuccessfully');
          this.back();
        },
        error: e => this.showError(e),
      });
    });
  }

  start(): void {
    this.confirmation.warn('::ConfirmStartStockCount', this.dto!.stockCountNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .start(this.id)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: () => {
            this.toaster.success('::StockCountStartedSuccessfully');
            this.count();
          },
          error: e => this.showError(e),
        });
    });
  }

  openPost(): void {
    this.postPreview = undefined;
    this.postModalOpen = true;
    this.postPreviewLoading = true;
    this.service
      .getPostingPreview(this.id)
      .pipe(finalize(() => (this.postPreviewLoading = false)))
      .subscribe({
        next: preview => (this.postPreview = preview),
        error: e => this.showError(e),
      });
  }

  confirmPost(): void {
    if (!this.postPreview?.canPost) return;
    this.actionInProgress = true;
    this.service
      .post(this.id)
      .pipe(finalize(() => (this.actionInProgress = false)))
      .subscribe({
        next: dto => {
          this.dto = dto;
          this.postModalOpen = false;
          this.toaster.success('::StockCountPostedSuccessfully');
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
          this.toaster.success('::StockCountCancelledSuccessfully');
        },
        error: e => this.showError(e),
      });
  }

  private showError(error: any): void {
    this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError');
  }
}
