import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  ShopBankAccountLookupDto,
  ShopBankAccountService,
  ShopBankTransferDto,
  ShopBankTransferStatus,
  ShopBankTransferType,
  ShopBankTransferService,
  shopBankTransferStatusOptions,
  shopBankTransferTypeOptions,
} from '../../proxy/shop-management/bank-accounts';

@Component({ selector: 'app-shop-bank-transfers', standalone: false, templateUrl: './shop-bank-transfers.component.html', styleUrl: './shop-bank-transfers.component.scss' })
export class ShopBankTransfersComponent implements OnInit {
  private readonly service = inject(ShopBankTransferService);
  private readonly accountService = inject(ShopBankAccountService);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly router = inject(Router);

  readonly Math = Math;
  readonly ShopBankTransferStatus = ShopBankTransferStatus;
  readonly typeOptions = shopBankTransferTypeOptions;
  readonly statusOptions = shopBankTransferStatusOptions;
  readonly canCreate = this.permissions.getGrantedPolicy('ShopManagement.BankTransfers.Create');
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.BankTransfers.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.BankTransfers.Delete');
  readonly canPost = this.permissions.getGrantedPolicy('ShopManagement.BankTransfers.Post');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.BankTransfers.Cancel');
  readonly canViewAmount = this.permissions.getGrantedPolicy('ShopManagement.BankTransfers.ViewAmount');

  accounts: ShopBankAccountLookupDto[] = [];
  items: ShopBankTransferDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;

  search = '';
  typeFilter: ShopBankTransferType | '' = '';
  statusFilter: ShopBankTransferStatus | '' = '';
  fromAccountFilter = '';
  toAccountFilter = '';
  dateFrom: string | null = null;
  dateTo: string | null = null;

  ngOnInit(): void {
    this.accountService.getLookup().subscribe(result => (this.accounts = result.items || []));
    this.load();
  }

  load(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;
    this.service
      .getList({
        filter: this.search || undefined,
        transferType: this.typeFilter === '' ? undefined : this.typeFilter,
        status: this.statusFilter === '' ? undefined : this.statusFilter,
        fromBankAccountId: this.fromAccountFilter || undefined,
        toBankAccountId: this.toAccountFilter || undefined,
        dateFrom: this.dateFrom || undefined,
        dateTo: this.dateTo || undefined,
        sorting: 'transferDate desc, creationTime desc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });
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

  create(): void {
    this.router.navigate(['/shop-management/bank-transfers/create']);
  }

  view(row: ShopBankTransferDto): void {
    this.router.navigate(['/shop-management/bank-transfers', row.id]);
  }

  edit(row: ShopBankTransferDto): void {
    this.router.navigate(['/shop-management/bank-transfers', row.id, 'edit']);
  }

  remove(row: ShopBankTransferDto): void {
    this.confirmation.warn('::ConfirmDeleteBankTransfer', row.transferNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(row.id).subscribe({
        next: () => {
          this.toaster.success('::BankTransferDeletedSuccessfully');
          this.load();
        },
        error: e => this.showError(e),
      });
    });
  }

  post(row: ShopBankTransferDto): void {
    this.confirmation.warn('::ConfirmPostBankTransfer', row.transferNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.post(row.id).subscribe({
        next: () => {
          this.toaster.success('::BankTransferPostedSuccessfully');
          this.load();
        },
        error: e => this.showError(e),
      });
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
