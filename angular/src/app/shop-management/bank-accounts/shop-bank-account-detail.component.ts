import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  ShopBankAccountDto,
  ShopBankAccountService,
  ShopBankAccountSummaryDto,
  ShopBankDirection,
  ShopBankTransactionDto,
  ShopBankTransactionService,
  ShopBankTransactionType,
} from '../../proxy/shop-management/bank-accounts';

@Component({ selector: 'app-shop-bank-account-detail', standalone: false, templateUrl: './shop-bank-account-detail.component.html', styleUrl: './shop-bank-account-detail.component.scss' })
export class ShopBankAccountDetailComponent implements OnInit {
  private readonly accountService = inject(ShopBankAccountService);
  private readonly transactionService = inject(ShopBankTransactionService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly ShopBankDirection = ShopBankDirection;
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.BankAccounts.Edit');
  readonly canViewBalance = this.permissions.getGrantedPolicy('ShopManagement.BankAccounts.ViewBalance');
  readonly canManualMovement = this.permissions.getGrantedPolicy('ShopManagement.BankTransactions.ManualMovement');
  readonly canCreateTransfer = this.permissions.getGrantedPolicy('ShopManagement.BankTransfers.Create');

  accountId!: string;
  account: ShopBankAccountDto | null = null;
  summary: ShopBankAccountSummaryDto | null = null;
  recentTransactions: ShopBankTransactionDto[] = [];
  loading = false;
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
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.accountId = id;
    this.load();
  }

  load(): void {
    this.loading = true;
    this.accountService
      .get(this.accountId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(account => {
        this.account = account;
        this.transactionService.getSummary(this.accountId).subscribe(summary => (this.summary = summary));
        this.transactionService
          .getList({ bankAccountId: this.accountId, sorting: 'transactionDate desc, creationTime desc', maxResultCount: 10 })
          .subscribe(result => (this.recentTransactions = result.items || []));
      });
  }

  typeLabel(type: ShopBankTransactionType): string {
    return '::' + ShopBankTransactionType[type];
  }

  directionLabel(direction: ShopBankDirection): string {
    return direction === ShopBankDirection.In ? '::DirectionIn' : '::DirectionOut';
  }

  edit(): void {
    this.router.navigate(['/shop-management/bank-accounts', this.accountId, 'edit']);
  }

  back(): void {
    this.router.navigate(['/shop-management/bank-accounts']);
  }

  viewAllTransactions(): void {
    this.router.navigate(['/shop-management/bank-transactions'], { queryParams: { bankAccountId: this.accountId } });
  }

  createTransfer(): void {
    this.router.navigate(['/shop-management/bank-transfers/create'], { queryParams: { fromBankAccountId: this.accountId } });
  }

  openMovementModal(direction: ShopBankDirection): void {
    this.movementDirection = direction;
    this.movementForm.reset({ transactionDate: this.today(), amount: 0, referenceNumber: '', description: '' });
    this.movementModalOpen = true;
  }

  submitMovement(): void {
    this.movementForm.markAllAsTouched();
    if (this.movementForm.invalid || this.submitting) return;

    this.submitting = true;
    const value = this.movementForm.getRawValue();
    this.transactionService
      .createManualMovement({
        bankAccountId: this.accountId,
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
