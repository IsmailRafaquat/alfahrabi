import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  ShopSupplierPaymentDto,
  ShopSupplierPaymentMethod,
  ShopSupplierPaymentService,
  ShopSupplierPaymentStatus,
  ShopSupplierPaymentType,
} from '../../proxy/shop-management/supplier-payments';

@Component({ selector: 'app-shop-supplier-payment-detail', standalone: false, templateUrl: './shop-supplier-payment-detail.component.html', styleUrl: './shop-supplier-payment-detail.component.scss' })
export class ShopSupplierPaymentDetailComponent implements OnInit {
  private readonly service = inject(ShopSupplierPaymentService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);

  readonly ShopSupplierPaymentStatus = ShopSupplierPaymentStatus;
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.Delete');
  readonly canPost = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.Post');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.Cancel');
  readonly canViewAmount = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.ViewAmount');

  id!: string;
  dto?: ShopSupplierPaymentDto;
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

  statusLabel(status?: ShopSupplierPaymentStatus): string {
    return '::' + ShopSupplierPaymentStatus[status ?? ShopSupplierPaymentStatus.Draft];
  }

  typeLabel(type?: ShopSupplierPaymentType): string {
    return '::' + ShopSupplierPaymentType[type ?? ShopSupplierPaymentType.Advance];
  }

  methodLabel(method?: ShopSupplierPaymentMethod): string {
    return '::' + ShopSupplierPaymentMethod[method ?? ShopSupplierPaymentMethod.Cash];
  }

  statusClass(status?: ShopSupplierPaymentStatus): string {
    switch (status) {
      case ShopSupplierPaymentStatus.Draft: return 'draft';
      case ShopSupplierPaymentStatus.Posted: return 'posted';
      case ShopSupplierPaymentStatus.Cancelled: return 'cancelled';
      default: return 'draft';
    }
  }

  edit(): void {
    this.router.navigate(['/shop-management/supplier-payments', this.id, 'edit']);
  }

  back(): void {
    this.router.navigate(['/shop-management/supplier-payments']);
  }

  remove(): void {
    this.confirmation.warn('::ConfirmDeleteSupplierPayment', this.dto!.paymentNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(this.id).subscribe({
        next: () => {
          this.toaster.success('::SupplierPaymentDeletedSuccessfully');
          this.back();
        },
        error: e => this.showError(e),
      });
    });
  }

  post(): void {
    this.confirmation.warn('::ConfirmPostSupplierPayment', this.dto!.paymentNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .post(this.id)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: dto => {
            this.dto = dto;
            this.toaster.success('::SupplierPaymentPostedSuccessfully');
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
          this.toaster.success('::SupplierPaymentCancelledSuccessfully');
        },
        error: e => this.showError(e),
      });
  }

  private showError(error: any): void {
    this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError');
  }
}
