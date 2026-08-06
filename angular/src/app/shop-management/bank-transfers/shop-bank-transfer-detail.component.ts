import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  ShopBankTransferDto,
  ShopBankTransferService,
  ShopBankTransferStatus,
  ShopBankTransferType,
} from '../../proxy/shop-management/bank-accounts';
import { ShopPrintService } from '../../shared/shop-print/services/shop-print.service';
import { SHOP_PRINT_DOCUMENT_TYPES } from '../../shared/shop-print/models/shop-print-document-types';

@Component({ selector: 'app-shop-bank-transfer-detail', standalone: false, templateUrl: './shop-bank-transfer-detail.component.html', styleUrl: './shop-bank-transfer-detail.component.scss' })
export class ShopBankTransferDetailComponent implements OnInit {
  private readonly service = inject(ShopBankTransferService);
  private readonly printService = inject(ShopPrintService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);

  readonly ShopBankTransferStatus = ShopBankTransferStatus;
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.BankTransfers.Edit');
  readonly canPost = this.permissions.getGrantedPolicy('ShopManagement.BankTransfers.Post');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.BankTransfers.Cancel');
  readonly canViewAmount = this.permissions.getGrantedPolicy('ShopManagement.BankTransfers.ViewAmount');
  readonly canPrint = this.permissions.getGrantedPolicy('ShopManagement.Print.Payments');

  transferId!: string;
  transfer: ShopBankTransferDto | null = null;
  loading = false;
  actionInProgress = false;
  cancelModalOpen = false;

  readonly cancelForm = this.fb.group({
    cancellationReason: ['', Validators.required],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.transferId = id;
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service
      .get(this.transferId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => (this.transfer = dto));
  }

  typeLabel(type: ShopBankTransferType): string {
    return '::' + ShopBankTransferType[type];
  }

  statusLabel(status: ShopBankTransferStatus): string {
    return '::' + ShopBankTransferStatus[status];
  }

  statusClass(status: ShopBankTransferStatus): string {
    switch (status) {
      case ShopBankTransferStatus.Draft: return 'draft';
      case ShopBankTransferStatus.Posted: return 'posted';
      case ShopBankTransferStatus.Cancelled: return 'cancelled';
      default: return 'draft';
    }
  }

  edit(): void {
    this.router.navigate(['/shop-management/bank-transfers', this.transferId, 'edit']);
  }

  print(): void {
    this.printService.openPreview({ documentType: SHOP_PRINT_DOCUMENT_TYPES.BankTransfer, documentId: this.transferId });
  }

  back(): void {
    this.router.navigate(['/shop-management/bank-transfers']);
  }

  post(): void {
    if (!this.transfer) return;
    this.confirmation.warn('::ConfirmPostBankTransfer', this.transfer.transferNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;

      this.actionInProgress = true;
      this.service
        .post(this.transferId)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: () => {
            this.toaster.success('::BankTransferPostedSuccessfully');
            this.load();
          },
          error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
        });
    });
  }

  openCancelModal(): void {
    this.cancelForm.reset({ cancellationReason: '' });
    this.cancelModalOpen = true;
  }

  confirmCancel(): void {
    this.cancelForm.markAllAsTouched();
    if (this.cancelForm.invalid || this.actionInProgress) return;

    this.actionInProgress = true;
    const value = this.cancelForm.getRawValue();
    this.service
      .cancel(this.transferId, { cancellationReason: value.cancellationReason! })
      .pipe(finalize(() => (this.actionInProgress = false)))
      .subscribe({
        next: () => {
          this.cancelModalOpen = false;
          this.toaster.success('::BankTransferCancelledSuccessfully');
          this.load();
        },
        error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
      });
  }
}
