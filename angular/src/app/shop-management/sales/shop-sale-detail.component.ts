import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { ShopSaleDto, ShopSalePaymentMethod, ShopSaleService, ShopSaleStatus, ShopSaleType } from '../../proxy/shop-management/sales';

@Component({ selector: 'app-shop-sale-detail', standalone: false, templateUrl: './shop-sale-detail.component.html', styleUrl: './shop-sale-detail.component.scss' })
export class ShopSaleDetailComponent implements OnInit {
  private readonly service = inject(ShopSaleService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);

  readonly ShopSaleStatus = ShopSaleStatus;
  readonly ShopSaleType = ShopSaleType;
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.Sales.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.Sales.Delete');
  readonly canComplete = this.permissions.getGrantedPolicy('ShopManagement.Sales.Complete');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.Sales.Cancel');
  readonly canViewPrice = this.permissions.getGrantedPolicy('ShopManagement.Sales.ViewPrice');
  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.Sales.ViewCost');
  readonly canViewStockTransactions = this.permissions.getGrantedPolicy('ShopManagement.StockTransactions');

  id!: string;
  dto?: ShopSaleDto;
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

  statusLabel(status: ShopSaleStatus): string {
    return '::' + ShopSaleStatus[status];
  }

  paymentMethodLabel(method: ShopSalePaymentMethod): string {
    return '::' + ShopSalePaymentMethod[method];
  }

  statusClass(status: ShopSaleStatus): string {
    switch (status) {
      case ShopSaleStatus.Draft: return 'draft';
      case ShopSaleStatus.Completed: return 'completed';
      case ShopSaleStatus.Cancelled: return 'cancelled';
      default: return 'draft';
    }
  }

  edit(): void {
    this.router.navigate(['/shop-management/sales', this.id, 'edit']);
  }

  back(): void {
    this.router.navigate(['/shop-management/sales']);
  }

  viewStockTransactions(): void {
    this.router.navigate(['/shop-management/stock-transactions'], { queryParams: { referenceNumber: this.dto!.saleNumber } });
  }

  remove(): void {
    this.confirmation.warn('::ConfirmDeleteSale', this.dto!.saleNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(this.id).subscribe({
        next: () => {
          this.toaster.success('::SaleDeletedSuccessfully');
          this.back();
        },
        error: e => this.showError(e),
      });
    });
  }

  complete(): void {
    this.confirmation.warn('::ConfirmCompleteSale', this.dto!.saleNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .complete(this.id)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: dto => {
            this.dto = dto;
            this.toaster.success('::SaleCompletedSuccessfully');
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
          this.toaster.success('::SaleCancelledSuccessfully');
        },
        error: e => this.showError(e),
      });
  }

  private showError(error: any): void {
    this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError');
  }
}
