import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  ShopSaleReturnDto,
  ShopSaleReturnReason,
  ShopSaleReturnService,
  ShopSaleReturnSettlementType,
  ShopSaleReturnStatus,
} from '../../proxy/shop-management/sale-returns';

@Component({ selector: 'app-shop-sale-return-detail', standalone: false, templateUrl: './shop-sale-return-detail.component.html', styleUrl: './shop-sale-return-detail.component.scss' })
export class ShopSaleReturnDetailComponent implements OnInit {
  private readonly service = inject(ShopSaleReturnService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);

  readonly ShopSaleReturnStatus = ShopSaleReturnStatus;
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.SaleReturns.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.SaleReturns.Delete');
  readonly canComplete = this.permissions.getGrantedPolicy('ShopManagement.SaleReturns.Complete');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.SaleReturns.Cancel');
  readonly canViewPrice = this.permissions.getGrantedPolicy('ShopManagement.SaleReturns.ViewPrice');
  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.SaleReturns.ViewCost');
  readonly canViewStockTransactions = this.permissions.getGrantedPolicy('ShopManagement.StockTransactions');
  readonly canViewCustomerLedger = this.permissions.getGrantedPolicy('ShopManagement.CustomerLedger');

  id!: string;
  dto?: ShopSaleReturnDto;
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

  edit(): void {
    this.router.navigate(['/shop-management/sale-returns', this.id, 'edit']);
  }

  back(): void {
    this.router.navigate(['/shop-management/sale-returns']);
  }

  viewSale(): void {
    if (this.dto?.saleId) this.router.navigate(['/shop-management/sales', this.dto.saleId]);
  }

  viewCustomerLedger(): void {
    if (this.dto?.customerId) this.router.navigate(['/shop-management/customer-ledger', this.dto.customerId]);
  }

  viewStockTransactions(): void {
    this.router.navigate(['/shop-management/stock-transactions'], { queryParams: { referenceNumber: this.dto!.saleReturnNumber } });
  }

  remove(): void {
    this.confirmation.warn('::ConfirmDeleteSaleReturn', this.dto!.saleReturnNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(this.id).subscribe({
        next: () => {
          this.toaster.success('::SaleReturnDeletedSuccessfully');
          this.back();
        },
        error: e => this.showError(e),
      });
    });
  }

  complete(): void {
    this.confirmation.warn('::ConfirmCompleteSaleReturn', this.dto!.saleReturnNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .complete(this.id)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: dto => {
            this.dto = dto;
            this.toaster.success('::SaleReturnCompletedSuccessfully');
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
          this.toaster.success('::SaleReturnCancelledSuccessfully');
        },
        error: e => this.showError(e),
      });
  }

  private showError(error: any): void {
    this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError');
  }
}
