import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  ShopBankAccountLookupDto,
  ShopBankAccountService,
  ShopBankDirection,
  ShopBankTransactionDto,
  ShopBankTransactionType,
  shopBankTransactionTypeOptions,
} from '../../proxy/shop-management/bank-accounts';
import { ShopBankTransactionService } from '../../proxy/shop-management/bank-accounts';

@Component({ selector: 'app-shop-bank-transactions', standalone: false, templateUrl: './shop-bank-transactions.component.html', styleUrl: './shop-bank-transactions.component.scss' })
export class ShopBankTransactionsComponent implements OnInit {
  private readonly transactionService = inject(ShopBankTransactionService);
  private readonly accountService = inject(ShopBankAccountService);
  private readonly permissions = inject(PermissionService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly toaster = inject(ToasterService);

  readonly Math = Math;
  readonly ShopBankDirection = ShopBankDirection;
  readonly typeOptions = shopBankTransactionTypeOptions;
  readonly canViewAmount = this.permissions.getGrantedPolicy('ShopManagement.BankTransactions.ViewAmount');
  readonly canManualMovement = this.permissions.getGrantedPolicy('ShopManagement.BankTransactions.ManualMovement');

  accounts: ShopBankAccountLookupDto[] = [];
  items: ShopBankTransactionDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 20;
  loading = false;

  filters: { filter?: string } = {};
  accountFilter = '';
  typeFilter: ShopBankTransactionType | '' = '';
  directionFilter: ShopBankDirection | '' = '';
  dateFrom: string | null = null;
  dateTo: string | null = null;
  minimumAmount: number | null = null;
  maximumAmount: number | null = null;

  submitting = false;
  movementModalOpen = false;
  movementDirection: ShopBankDirection = ShopBankDirection.In;

  readonly movementForm = this.fb.group({
    transactionDate: [this.today(), Validators.required],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    referenceNumber: [''],
    description: [''],
  });

  ngOnInit(): void {
    this.accountService.getLookup().subscribe(result => (this.accounts = result.items || []));

    const bankAccountId = this.route.snapshot.queryParamMap.get('bankAccountId');
    if (bankAccountId) this.accountFilter = bankAccountId;

    this.load();
  }

  load(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;
    this.transactionService
      .getList({
        filter: this.filters.filter || undefined,
        bankAccountId: this.accountFilter || undefined,
        transactionType: this.typeFilter === '' ? undefined : this.typeFilter,
        direction: this.directionFilter === '' ? undefined : this.directionFilter,
        dateFrom: this.dateFrom || undefined,
        dateTo: this.dateTo || undefined,
        minimumAmount: this.minimumAmount ?? undefined,
        maximumAmount: this.maximumAmount ?? undefined,
        sorting: 'transactionDate desc, creationTime desc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });
  }

  typeLabel(type: ShopBankTransactionType): string {
    return '::' + ShopBankTransactionType[type];
  }

  directionLabel(direction: ShopBankDirection): string {
    return direction === ShopBankDirection.In ? '::DirectionIn' : '::DirectionOut';
  }

  back(): void {
    this.router.navigate(['/shop-management/bank-accounts']);
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

  openMovementModal(direction: ShopBankDirection): void {
    if (!this.accountFilter) return;
    this.movementDirection = direction;
    this.movementForm.reset({ transactionDate: this.today(), amount: 0, referenceNumber: '', description: '' });
    this.movementModalOpen = true;
  }

  submitMovement(): void {
    this.movementForm.markAllAsTouched();
    if (this.movementForm.invalid || this.submitting || !this.accountFilter) return;

    this.submitting = true;
    const value = this.movementForm.getRawValue();
    this.transactionService
      .createManualMovement({
        bankAccountId: this.accountFilter,
        transactionDate: value.transactionDate!,
        direction: this.movementDirection,
        amount: value.amount!,
        referenceNumber: value.referenceNumber || undefined,
        description: value.description || undefined,
      })
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: () => {
          this.movementModalOpen = false;
          this.toaster.success('::BankMovementRecordedSuccessfully');
          this.load();
        },
        error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
      });
  }

  private today(): string {
    return new Date().toISOString().slice(0, 10);
  }
}
