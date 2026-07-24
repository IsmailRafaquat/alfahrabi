import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  ShopExpenseDto,
  ShopExpensePaymentMethod,
  ShopExpenseService,
  ShopExpenseStatus,
} from '../../proxy/shop-management/expenses';

@Component({ selector: 'app-shop-expense-detail', standalone: false, templateUrl: './shop-expense-detail.component.html', styleUrl: './shop-expense-detail.component.scss' })
export class ShopExpenseDetailComponent implements OnInit {
  private readonly service = inject(ShopExpenseService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);

  readonly ShopExpenseStatus = ShopExpenseStatus;
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.Expenses.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.Expenses.Delete');
  readonly canPost = this.permissions.getGrantedPolicy('ShopManagement.Expenses.Post');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.Expenses.Cancel');
  readonly canViewAmount = this.permissions.getGrantedPolicy('ShopManagement.Expenses.ViewAmount');

  id!: string;
  dto?: ShopExpenseDto;
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

  statusLabel(status?: ShopExpenseStatus): string {
    return status == null ? '' : '::' + ShopExpenseStatus[status];
  }

  statusClass(status?: ShopExpenseStatus): string {
    switch (status) {
      case ShopExpenseStatus.Draft: return 'draft';
      case ShopExpenseStatus.Posted: return 'posted';
      case ShopExpenseStatus.Cancelled: return 'cancelled';
      default: return 'draft';
    }
  }

  paymentMethodLabel(method?: ShopExpensePaymentMethod): string {
    return method == null ? '' : '::' + ShopExpensePaymentMethod[method];
  }

  edit(): void {
    this.router.navigate(['/shop-management/expenses', this.id, 'edit']);
  }

  back(): void {
    this.router.navigate(['/shop-management/expenses']);
  }

  remove(): void {
    this.confirmation.warn('::ConfirmDeleteExpense', this.dto!.expenseNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(this.id).subscribe({
        next: () => {
          this.toaster.success('::ExpenseDeletedSuccessfully');
          this.back();
        },
        error: e => this.showError(e),
      });
    });
  }

  post(): void {
    this.confirmation.warn('::ConfirmPostExpense', this.dto!.expenseNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .post(this.id)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: dto => {
            this.dto = dto;
            this.toaster.success('::ExpensePostedSuccessfully');
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
          this.toaster.success('::ExpenseCancelledSuccessfully');
        },
        error: e => this.showError(e),
      });
  }

  private showError(error: any): void {
    this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError');
  }
}
