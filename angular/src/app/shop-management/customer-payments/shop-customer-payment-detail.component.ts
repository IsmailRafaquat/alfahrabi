import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  ShopCustomerPaymentDto,
  ShopCustomerPaymentMethod,
  ShopCustomerPaymentService,
  ShopCustomerPaymentStatus,
  ShopCustomerPaymentType,
} from '../../proxy/shop-management/customer-payments';
import { ShopPrintService } from '../../shared/shop-print/services/shop-print.service';
import { SHOP_PRINT_DOCUMENT_TYPES } from '../../shared/shop-print/models/shop-print-document-types';

@Component({ selector: 'app-shop-customer-payment-detail', standalone: false, templateUrl: './shop-customer-payment-detail.component.html', styleUrl: './shop-customer-payment-detail.component.scss' })
export class ShopCustomerPaymentDetailComponent implements OnInit {
  private readonly service = inject(ShopCustomerPaymentService);
  private readonly printService = inject(ShopPrintService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);

  readonly ShopCustomerPaymentStatus = ShopCustomerPaymentStatus;
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.CustomerPayments.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.CustomerPayments.Delete');
  readonly canPost = this.permissions.getGrantedPolicy('ShopManagement.CustomerPayments.Post');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.CustomerPayments.Cancel');
  readonly canViewAmount = this.permissions.getGrantedPolicy('ShopManagement.CustomerPayments.ViewAmount');
  readonly canPrint = this.permissions.getGrantedPolicy('ShopManagement.Print.Payments');

  id!: string;
  dto?: ShopCustomerPaymentDto;
  loading = false;
  actionInProgress = false;

  private saleSummaries: Record<string, { totalPaidAmount?: number; pendingAmount?: number }> = {};

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
      .subscribe(dto => {
        this.dto = dto;
        if (dto.customerId) this.loadSaleSummaries(dto.customerId);
      });
  }

  totalPaidFor(saleId?: string, grandTotal?: number): number | undefined {
    if (!saleId) return grandTotal;
    const row = this.saleSummaries[saleId];
    return row ? row.totalPaidAmount : grandTotal;
  }

  pendingFor(saleId?: string): number {
    if (!saleId) return 0;
    return this.saleSummaries[saleId]?.pendingAmount ?? 0;
  }

  private loadSaleSummaries(customerId: string): void {
    this.service.getOutstandingSales(customerId).subscribe(result => {
      const map: Record<string, { totalPaidAmount?: number; pendingAmount?: number }> = {};
      (result.items || []).forEach(x => {
        if (x.saleId) map[x.saleId] = { totalPaidAmount: x.totalPaidAmount, pendingAmount: x.pendingAmount };
      });
      this.saleSummaries = map;
    });
  }

  statusLabel(status?: ShopCustomerPaymentStatus): string {
    return '::' + ShopCustomerPaymentStatus[status ?? ShopCustomerPaymentStatus.Draft];
  }

  typeLabel(type?: ShopCustomerPaymentType): string {
    return '::' + ShopCustomerPaymentType[type ?? ShopCustomerPaymentType.Advance];
  }

  methodLabel(method?: ShopCustomerPaymentMethod): string {
    return '::' + ShopCustomerPaymentMethod[method ?? ShopCustomerPaymentMethod.Cash];
  }

  statusClass(status?: ShopCustomerPaymentStatus): string {
    switch (status) {
      case ShopCustomerPaymentStatus.Draft: return 'draft';
      case ShopCustomerPaymentStatus.Posted: return 'posted';
      case ShopCustomerPaymentStatus.Cancelled: return 'cancelled';
      default: return 'draft';
    }
  }

  edit(): void {
    this.router.navigate(['/shop-management/customer-payments', this.id, 'edit']);
  }

  print(): void {
    this.printService.openPreview({ documentType: SHOP_PRINT_DOCUMENT_TYPES.CustomerPayment, documentId: this.id });
  }

  back(): void {
    this.router.navigate(['/shop-management/customer-payments']);
  }

  remove(): void {
    this.confirmation.warn('::ConfirmDeleteCustomerPayment', this.dto!.paymentNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(this.id).subscribe({
        next: () => {
          this.toaster.success('::CustomerPaymentDeletedSuccessfully');
          this.back();
        },
        error: e => this.showError(e),
      });
    });
  }

  post(): void {
    this.confirmation.warn('::ConfirmPostCustomerPayment', this.dto!.paymentNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .post(this.id)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: dto => {
            this.dto = dto;
            this.toaster.success('::CustomerPaymentPostedSuccessfully');
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
          this.toaster.success('::CustomerPaymentCancelledSuccessfully');
        },
        error: e => this.showError(e),
      });
  }

  private showError(error: any): void {
    this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError');
  }
}
