import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  ShopSaleReturnDto,
  ShopSaleReturnReason,
  ShopSaleReturnService,
  ShopSaleReturnSettlementType,
  ShopSaleReturnStatus,
  shopSaleReturnReasonOptions,
  shopSaleReturnSettlementTypeOptions,
  shopSaleReturnStatusOptions,
} from '../../proxy/shop-management/sale-returns';
import { ShopCustomerLookupDto, ShopCustomerService } from '../../proxy/shop-management/customers';

@Component({ selector: 'app-shop-sale-returns', standalone: false, templateUrl: './shop-sale-returns.component.html', styleUrl: './shop-sale-returns.component.scss' })
export class ShopSaleReturnsComponent implements OnInit {
  readonly Math = Math;
  readonly ShopSaleReturnStatus = ShopSaleReturnStatus;
  readonly statusOptions = shopSaleReturnStatusOptions;
  readonly reasonOptions = shopSaleReturnReasonOptions;
  readonly settlementTypeOptions = shopSaleReturnSettlementTypeOptions;

  private readonly service = inject(ShopSaleReturnService);
  private readonly customerService = inject(ShopCustomerService);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);

  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.SaleReturns.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.SaleReturns.Delete');
  readonly canComplete = this.permissions.getGrantedPolicy('ShopManagement.SaleReturns.Complete');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.SaleReturns.Cancel');
  readonly canViewPrice = this.permissions.getGrantedPolicy('ShopManagement.SaleReturns.ViewPrice');
  readonly canViewStockTransactions = this.permissions.getGrantedPolicy('ShopManagement.StockTransactions');

  items: ShopSaleReturnDto[] = [];
  customers: ShopCustomerLookupDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;
  actionInProgress = false;

  search = '';
  customerFilter = '';
  statusFilter: ShopSaleReturnStatus | '' = '';
  reasonFilter: ShopSaleReturnReason | '' = '';
  settlementTypeFilter: ShopSaleReturnSettlementType | '' = '';
  returnDateFrom: string | null = null;
  returnDateTo: string | null = null;

  cancelModalOpen = false;
  cancelTarget?: ShopSaleReturnDto;
  readonly cancelForm = this.fb.group({ cancellationReason: ['', [Validators.required, Validators.maxLength(500)]] });

  ngOnInit(): void {
    this.customerService.getLookup().subscribe(result => (this.customers = result.items || []));
    this.load();
  }

  load(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;
    this.service
      .getList({
        filter: this.search || undefined,
        customerId: this.customerFilter || undefined,
        status: this.statusFilter === '' ? undefined : this.statusFilter,
        reason: this.reasonFilter === '' ? undefined : this.reasonFilter,
        settlementType: this.settlementTypeFilter === '' ? undefined : this.settlementTypeFilter,
        returnDateFrom: this.returnDateFrom || undefined,
        returnDateTo: this.returnDateTo || undefined,
        sorting: 'returnDate desc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });
  }

  isDraft(row: ShopSaleReturnDto): boolean {
    return row.status === ShopSaleReturnStatus.Draft;
  }

  statusLabel(status?: ShopSaleReturnStatus): string {
    return status == null ? '' : '::' + ShopSaleReturnStatus[status];
  }

  statusClass(status?: ShopSaleReturnStatus): string {
    switch (status) {
      case ShopSaleReturnStatus.Draft: return 'draft';
      case ShopSaleReturnStatus.Completed: return 'completed';
      case ShopSaleReturnStatus.Cancelled: return 'cancelled';
      default: return 'draft';
    }
  }

  reasonLabel(reason?: ShopSaleReturnReason): string {
    return reason == null ? '' : '::' + ShopSaleReturnReason[reason];
  }

  settlementTypeLabel(type?: ShopSaleReturnSettlementType): string {
    return type == null ? '' : '::' + ShopSaleReturnSettlementType[type];
  }

  view(row: ShopSaleReturnDto): void {
    this.router.navigate(['/shop-management/sale-returns', row.id]);
  }

  edit(row: ShopSaleReturnDto): void {
    this.router.navigate(['/shop-management/sale-returns', row.id, 'edit']);
  }

  viewStockTransactions(row: ShopSaleReturnDto): void {
    this.router.navigate(['/shop-management/stock-transactions'], { queryParams: { referenceNumber: row.saleReturnNumber } });
  }

  remove(row: ShopSaleReturnDto): void {
    this.confirmation.warn('::ConfirmDeleteSaleReturn', row.saleReturnNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(row.id).subscribe({
        next: () => {
          this.toaster.success('::SaleReturnDeletedSuccessfully');
          this.load();
        },
        error: e => this.showError(e),
      });
    });
  }

  complete(row: ShopSaleReturnDto): void {
    this.confirmation.warn('::ConfirmCompleteSaleReturn', row.saleReturnNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .complete(row.id)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: () => {
            this.toaster.success('::SaleReturnCompletedSuccessfully');
            this.load();
          },
          error: e => this.showError(e),
        });
    });
  }

  openCancel(row: ShopSaleReturnDto): void {
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
          this.toaster.success('::SaleReturnCancelledSuccessfully');
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
