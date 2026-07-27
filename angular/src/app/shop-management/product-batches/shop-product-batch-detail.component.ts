import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  ShopProductBatchDto,
  ShopProductBatchService,
  ShopProductBatchStatus,
} from '../../proxy/shop-management/product-batches';
import { ShopStockTransactionDto } from '../../proxy/shop-management/stock-transactions';

@Component({ selector: 'app-shop-product-batch-detail', standalone: false, templateUrl: './shop-product-batch-detail.component.html', styleUrl: './shop-product-batch-detail.component.scss' })
export class ShopProductBatchDetailComponent implements OnInit {
  private readonly service = inject(ShopProductBatchService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);

  readonly ShopProductBatchStatus = ShopProductBatchStatus;
  readonly canEditMetadata = this.permissions.getGrantedPolicy('ShopManagement.ProductBatches.EditMetadata');
  readonly canBlock = this.permissions.getGrantedPolicy('ShopManagement.ProductBatches.Block');
  readonly canUnblock = this.permissions.getGrantedPolicy('ShopManagement.ProductBatches.Unblock');
  readonly canViewTransactions = this.permissions.getGrantedPolicy('ShopManagement.ProductBatches.ViewTransactions');
  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.ProductBatches.ViewCost');

  id!: string;
  dto?: ShopProductBatchDto;
  loading = false;
  actionInProgress = false;

  transactions: ShopStockTransactionDto[] = [];
  transactionsLoading = false;
  transactionsLoaded = false;

  editModalOpen = false;
  readonly editForm = this.fb.group({
    manufacturingDate: [null as string | null],
    expiryDate: [null as string | null],
    notes: ['', Validators.maxLength(1000)],
  });

  blockModalOpen = false;
  readonly blockForm = this.fb.group({ blockReason: ['', [Validators.required, Validators.maxLength(500)]] });

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id')!;
    this.load();
    if (this.route.snapshot.queryParamMap.get('tab') === 'transactions') this.loadTransactions();
  }

  load(): void {
    this.loading = true;
    this.service
      .get(this.id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => (this.dto = dto));
  }

  loadTransactions(): void {
    if (!this.canViewTransactions || this.transactionsLoading) return;
    this.transactionsLoading = true;
    this.service
      .getTransactions(this.id)
      .pipe(finalize(() => (this.transactionsLoading = false)))
      .subscribe(result => {
        this.transactions = result.items || [];
        this.transactionsLoaded = true;
      });
  }

  statusLabel(status?: ShopProductBatchStatus): string {
    return status == null ? '' : '::' + ShopProductBatchStatus[status];
  }

  statusClass(status?: ShopProductBatchStatus): string {
    switch (status) {
      case ShopProductBatchStatus.Active: return 'active';
      case ShopProductBatchStatus.NearExpiry: return 'near-expiry';
      case ShopProductBatchStatus.Expired: return 'expired';
      case ShopProductBatchStatus.Exhausted: return 'exhausted';
      case ShopProductBatchStatus.Blocked: return 'blocked';
      default: return 'active';
    }
  }

  back(): void {
    this.router.navigate(['/shop-management/product-batches']);
  }

  openEdit(): void {
    this.editForm.reset({
      manufacturingDate: this.dto!.manufacturingDate ? this.dto!.manufacturingDate.substring(0, 10) : null,
      expiryDate: this.dto!.expiryDate ? this.dto!.expiryDate.substring(0, 10) : null,
      notes: this.dto!.notes || '',
    });
    this.editModalOpen = true;
  }

  confirmEdit(): void {
    this.editForm.markAllAsTouched();
    if (this.editForm.invalid) return;
    this.actionInProgress = true;
    const raw = this.editForm.getRawValue();
    this.service
      .update(this.id, {
        manufacturingDate: raw.manufacturingDate || undefined,
        expiryDate: raw.expiryDate || undefined,
        notes: raw.notes || undefined,
      })
      .pipe(finalize(() => (this.actionInProgress = false)))
      .subscribe({
        next: dto => {
          this.dto = dto;
          this.editModalOpen = false;
          this.toaster.success('::BatchUpdatedSuccessfully');
        },
        error: e => this.showError(e),
      });
  }

  openBlock(): void {
    this.blockForm.reset();
    this.blockModalOpen = true;
  }

  confirmBlock(): void {
    this.blockForm.markAllAsTouched();
    if (this.blockForm.invalid) return;
    this.actionInProgress = true;
    this.service
      .block(this.id, this.blockForm.getRawValue() as { blockReason: string })
      .pipe(finalize(() => (this.actionInProgress = false)))
      .subscribe({
        next: dto => {
          this.dto = dto;
          this.blockModalOpen = false;
          this.toaster.success('::BatchBlockedSuccessfully');
        },
        error: e => this.showError(e),
      });
  }

  unblock(): void {
    this.confirmation.warn('::ConfirmUnblockBatch', this.dto!.batchNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .unblock(this.id)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: dto => {
            this.dto = dto;
            this.toaster.success('::BatchUnblockedSuccessfully');
          },
          error: e => this.showError(e),
        });
    });
  }

  private showError(error: any): void {
    this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError');
  }
}
