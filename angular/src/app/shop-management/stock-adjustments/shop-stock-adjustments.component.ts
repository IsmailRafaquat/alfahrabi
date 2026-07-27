import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  ShopStockAdjustmentDto,
  ShopStockAdjustmentReason,
  ShopStockAdjustmentService,
  ShopStockAdjustmentStatus,
  ShopStockAdjustmentType,
  shopStockAdjustmentReasonOptions,
  shopStockAdjustmentStatusOptions,
  shopStockAdjustmentTypeOptions,
} from '../../proxy/shop-management/stock-adjustments';
import { ShopStockAdjustmentProductLookupDto } from '../../proxy/shop-management/stock-adjustments';

@Component({ selector: 'app-shop-stock-adjustments', standalone: false, templateUrl: './shop-stock-adjustments.component.html', styleUrl: './shop-stock-adjustments.component.scss' })
export class ShopStockAdjustmentsComponent implements OnInit {
  readonly Math = Math;
  readonly ShopStockAdjustmentStatus = ShopStockAdjustmentStatus;
  readonly ShopStockAdjustmentType = ShopStockAdjustmentType;
  readonly statusOptions = shopStockAdjustmentStatusOptions;
  readonly reasonOptions = shopStockAdjustmentReasonOptions;
  readonly typeOptions = shopStockAdjustmentTypeOptions;

  private readonly service = inject(ShopStockAdjustmentService);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);

  readonly canCreate = this.permissions.getGrantedPolicy('ShopManagement.StockAdjustments.Create');
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.StockAdjustments.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.StockAdjustments.Delete');
  readonly canPost = this.permissions.getGrantedPolicy('ShopManagement.StockAdjustments.Post');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.StockAdjustments.Cancel');
  readonly canViewStockTransactions = this.permissions.getGrantedPolicy('ShopManagement.StockTransactions');

  items: ShopStockAdjustmentDto[] = [];
  products: ShopStockAdjustmentProductLookupDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;
  actionInProgress = false;

  tooltipLang: 'en' | 'ur' = 'en';

  search = '';
  statusFilter: ShopStockAdjustmentStatus | '' = '';
  reasonFilter: ShopStockAdjustmentReason | '' = '';
  typeFilter: ShopStockAdjustmentType | '' = '';
  productFilter = '';
  adjustmentDateFrom: string | null = null;
  adjustmentDateTo: string | null = null;

  cancelModalOpen = false;
  cancelTarget?: ShopStockAdjustmentDto;
  readonly cancelForm = this.fb.group({ cancellationReason: ['', [Validators.required, Validators.maxLength(500)]] });

  ngOnInit(): void {
    this.service.getProductLookup().subscribe(result => (this.products = result.items || []));
    this.load();
  }

  load(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;
    this.service
      .getList({
        filter: this.search || undefined,
        status: this.statusFilter === '' ? undefined : this.statusFilter,
        reason: this.reasonFilter === '' ? undefined : this.reasonFilter,
        adjustmentType: this.typeFilter === '' ? undefined : this.typeFilter,
        productId: this.productFilter || undefined,
        adjustmentDateFrom: this.adjustmentDateFrom || undefined,
        adjustmentDateTo: this.adjustmentDateTo || undefined,
        sorting: 'adjustmentDate desc, creationTime desc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });
  }

  isDraft(row: ShopStockAdjustmentDto): boolean {
    return row.status === ShopStockAdjustmentStatus.Draft;
  }

  itemCount(row: ShopStockAdjustmentDto): number {
    return row.items?.length ?? 0;
  }

  totalIncreaseQuantity(row: ShopStockAdjustmentDto): number {
    return (row.items || []).filter(x => x.adjustmentType === ShopStockAdjustmentType.Increase).reduce((sum, x) => sum + (x.adjustmentQuantity || 0), 0);
  }

  totalDecreaseQuantity(row: ShopStockAdjustmentDto): number {
    return (row.items || []).filter(x => x.adjustmentType === ShopStockAdjustmentType.Decrease).reduce((sum, x) => sum + (x.adjustmentQuantity || 0), 0);
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

  create(): void {
    this.router.navigate(['/shop-management/stock-adjustments/create']);
  }

  view(row: ShopStockAdjustmentDto): void {
    this.router.navigate(['/shop-management/stock-adjustments', row.id]);
  }

  edit(row: ShopStockAdjustmentDto): void {
    this.router.navigate(['/shop-management/stock-adjustments', row.id, 'edit']);
  }

  viewStockTransactions(row: ShopStockAdjustmentDto): void {
    this.router.navigate(['/shop-management/stock-transactions'], { queryParams: { referenceNumber: row.adjustmentNumber } });
  }

  remove(row: ShopStockAdjustmentDto): void {
    this.confirmation.warn('::ConfirmDeleteStockAdjustment', row.adjustmentNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(row.id).subscribe({
        next: () => {
          this.toaster.success('::StockAdjustmentDeletedSuccessfully');
          this.load();
        },
        error: e => this.showError(e),
      });
    });
  }

  post(row: ShopStockAdjustmentDto): void {
    this.confirmation.warn('::ConfirmPostStockAdjustment', row.adjustmentNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .post(row.id)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: () => {
            this.toaster.success('::StockAdjustmentPostedSuccessfully');
            this.load();
          },
          error: e => this.showError(e),
        });
    });
  }

  openCancel(row: ShopStockAdjustmentDto): void {
    this.cancelTarget = row;
    this.cancelForm.reset();
    this.cancelModalOpen = true;
  }

  confirmCancel(): void {
    this.cancelForm.markAllAsTouched();
    if (this.cancelForm.invalid || !this.cancelTarget) return;
    this.actionInProgress = true;
    this.service
      .cancel(this.cancelTarget.id, this.cancelForm.getRawValue() as { cancellationReason: string })
      .pipe(finalize(() => (this.actionInProgress = false)))
      .subscribe({
        next: () => {
          this.cancelModalOpen = false;
          this.toaster.success('::StockAdjustmentCancelledSuccessfully');
          this.load();
        },
        error: e => this.showError(e),
      });
  }

  previousPage(): void {
    if (this.page > 0) {
      this.page--;
      this.load();
    }
  }

  nextPage(): void {
    if ((this.page + 1) * this.pageSize < this.totalCount) {
      this.page++;
      this.load();
    }
  }

  private showError(error: any): void {
    this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError');
  }
}
