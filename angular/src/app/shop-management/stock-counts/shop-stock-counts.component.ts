import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  ShopStockCountDto,
  ShopStockCountPostingPreviewDto,
  ShopStockCountScope,
  ShopStockCountService,
  ShopStockCountStatus,
  shopStockCountScopeOptions,
  shopStockCountStatusOptions,
} from '../../proxy/shop-management/stock-counts';
import { ShopProductCategoryLookupDto, ShopProductCategoryService } from '../../proxy/shop-management/product-categories';

@Component({ selector: 'app-shop-stock-counts', standalone: false, templateUrl: './shop-stock-counts.component.html', styleUrl: './shop-stock-counts.component.scss' })
export class ShopStockCountsComponent implements OnInit {
  readonly Math = Math;
  readonly ShopStockCountStatus = ShopStockCountStatus;
  readonly statusOptions = shopStockCountStatusOptions;
  readonly scopeOptions = shopStockCountScopeOptions;

  private readonly service = inject(ShopStockCountService);
  private readonly categoryService = inject(ShopProductCategoryService);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);

  readonly canCreate = this.permissions.getGrantedPolicy('ShopManagement.StockCounts.Create');
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.StockCounts.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.StockCounts.Delete');
  readonly canStart = this.permissions.getGrantedPolicy('ShopManagement.StockCounts.Start');
  readonly canPost = this.permissions.getGrantedPolicy('ShopManagement.StockCounts.Post');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.StockCounts.Cancel');
  readonly canViewStockTransactions = this.permissions.getGrantedPolicy('ShopManagement.StockTransactions');
  readonly canViewStockAdjustments = this.permissions.getGrantedPolicy('ShopManagement.StockAdjustments');

  items: ShopStockCountDto[] = [];
  categories: ShopProductCategoryLookupDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;
  actionInProgress = false;

  tooltipLang: 'en' | 'ur' = 'en';

  search = '';
  statusFilter: ShopStockCountStatus | '' = '';
  scopeFilter: ShopStockCountScope | '' = '';
  categoryFilter = '';
  countDateFrom: string | null = null;
  countDateTo: string | null = null;
  hasDifferencesFilter: boolean | '' = '';

  cancelModalOpen = false;
  cancelTarget?: ShopStockCountDto;
  readonly cancelForm = this.fb.group({ cancellationReason: ['', [Validators.required, Validators.maxLength(500)]] });

  postModalOpen = false;
  postTarget?: ShopStockCountDto;
  postPreview?: ShopStockCountPostingPreviewDto;
  postPreviewLoading = false;

  ngOnInit(): void {
    this.categoryService.getLookup().subscribe(result => (this.categories = result.items || []));
    this.load();
  }

  load(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;
    this.service
      .getList({
        filter: this.search || undefined,
        status: this.statusFilter === '' ? undefined : this.statusFilter,
        scope: this.scopeFilter === '' ? undefined : this.scopeFilter,
        productCategoryId: this.categoryFilter || undefined,
        countDateFrom: this.countDateFrom || undefined,
        countDateTo: this.countDateTo || undefined,
        hasDifferences: this.hasDifferencesFilter === '' ? undefined : this.hasDifferencesFilter,
        sorting: 'countDate desc, creationTime desc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });
  }

  isDraft(row: ShopStockCountDto): boolean {
    return row.status === ShopStockCountStatus.Draft;
  }

  isInProgress(row: ShopStockCountDto): boolean {
    return row.status === ShopStockCountStatus.InProgress;
  }

  isCounted(row: ShopStockCountDto): boolean {
    return row.status === ShopStockCountStatus.Counted;
  }

  isCancellable(row: ShopStockCountDto): boolean {
    return row.status === ShopStockCountStatus.Draft || row.status === ShopStockCountStatus.InProgress || row.status === ShopStockCountStatus.Counted;
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

  scopeLabel(scope?: ShopStockCountScope): string {
    return scope == null ? '' : '::' + ShopStockCountScope[scope];
  }

  create(): void {
    this.router.navigate(['/shop-management/stock-counts/create']);
  }

  view(row: ShopStockCountDto): void {
    this.router.navigate(['/shop-management/stock-counts', row.id]);
  }

  edit(row: ShopStockCountDto): void {
    this.router.navigate(['/shop-management/stock-counts', row.id, 'edit']);
  }

  count(row: ShopStockCountDto): void {
    this.router.navigate(['/shop-management/stock-counts', row.id, 'count']);
  }

  viewGeneratedStockAdjustment(row: ShopStockCountDto): void {
    if (row.generatedStockAdjustmentId) this.router.navigate(['/shop-management/stock-adjustments', row.generatedStockAdjustmentId]);
  }

  viewStockTransactions(row: ShopStockCountDto): void {
    this.router.navigate(['/shop-management/stock-transactions'], { queryParams: { referenceNumber: row.generatedStockAdjustmentNumber } });
  }

  remove(row: ShopStockCountDto): void {
    this.confirmation.warn('::ConfirmDeleteStockCount', row.stockCountNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(row.id).subscribe({
        next: () => {
          this.toaster.success('::StockCountDeletedSuccessfully');
          this.load();
        },
        error: e => this.showError(e),
      });
    });
  }

  start(row: ShopStockCountDto): void {
    this.confirmation.warn('::ConfirmStartStockCount', row.stockCountNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .start(row.id)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: () => {
            this.toaster.success('::StockCountStartedSuccessfully');
            this.router.navigate(['/shop-management/stock-counts', row.id, 'count']);
          },
          error: e => this.showError(e),
        });
    });
  }

  openPost(row: ShopStockCountDto): void {
    this.postTarget = row;
    this.postPreview = undefined;
    this.postModalOpen = true;
    this.postPreviewLoading = true;
    this.service
      .getPostingPreview(row.id)
      .pipe(finalize(() => (this.postPreviewLoading = false)))
      .subscribe({
        next: preview => (this.postPreview = preview),
        error: e => this.showError(e),
      });
  }

  confirmPost(): void {
    if (!this.postTarget || !this.postPreview?.canPost) return;
    this.actionInProgress = true;
    this.service
      .post(this.postTarget.id)
      .pipe(finalize(() => (this.actionInProgress = false)))
      .subscribe({
        next: () => {
          this.postModalOpen = false;
          this.toaster.success('::StockCountPostedSuccessfully');
          this.load();
        },
        error: e => this.showError(e),
      });
  }

  openCancel(row: ShopStockCountDto): void {
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
          this.toaster.success('::StockCountCancelledSuccessfully');
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
