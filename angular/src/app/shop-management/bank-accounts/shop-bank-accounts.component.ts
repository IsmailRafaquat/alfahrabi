import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { ShopBankAccountDto, ShopBankAccountService } from '../../proxy/shop-management/bank-accounts';

@Component({ selector: 'app-shop-bank-accounts', standalone: false, templateUrl: './shop-bank-accounts.component.html', styleUrl: './shop-bank-accounts.component.scss' })
export class ShopBankAccountsComponent implements OnInit {
  private readonly service = inject(ShopBankAccountService);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly router = inject(Router);

  readonly Math = Math;
  readonly canCreate = this.permissions.getGrantedPolicy('ShopManagement.BankAccounts.Create');
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.BankAccounts.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.BankAccounts.Delete');
  readonly canViewBalance = this.permissions.getGrantedPolicy('ShopManagement.BankAccounts.ViewBalance');

  items: ShopBankAccountDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;

  filters: { filter?: string } = {};
  bankNameFilter = '';
  statusFilter: boolean | null = null;
  defaultFilter: boolean | null = null;

  ngOnInit(): void {
    this.load();
  }

  load(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;
    this.service
      .getList({
        filter: this.filters.filter || undefined,
        bankName: this.bankNameFilter || undefined,
        isActive: this.statusFilter ?? undefined,
        isDefault: this.defaultFilter ?? undefined,
        sorting: 'accountName asc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });
  }

  create(): void {
    this.router.navigate(['/shop-management/bank-accounts/create']);
  }

  view(row: ShopBankAccountDto): void {
    this.router.navigate(['/shop-management/bank-accounts', row.id]);
  }

  edit(row: ShopBankAccountDto): void {
    this.router.navigate(['/shop-management/bank-accounts', row.id, 'edit']);
  }

  remove(row: ShopBankAccountDto): void {
    this.confirmation.warn('::ConfirmDeleteBankAccount', row.accountName).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(row.id).subscribe({
        next: () => {
          this.toaster.success('::BankAccountDeletedSuccessfully');
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
