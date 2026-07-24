import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  ShopExpenseDto,
  ShopExpensePaymentMethod,
  ShopExpenseService,
  ShopExpenseStatus,
  ShopExpenseSummaryDto,
  shopExpensePaymentMethodOptions,
  shopExpenseStatusOptions,
} from '../../proxy/shop-management/expenses';
import { ShopExpenseCategoryLookupDto, ShopExpenseCategoryService } from '../../proxy/shop-management/expense-categories';

@Component({ selector: 'app-shop-expenses', standalone: false, templateUrl: './shop-expenses.component.html', styleUrl: './shop-expenses.component.scss' })
export class ShopExpensesComponent implements OnInit {
  readonly Math = Math;
  readonly ShopExpenseStatus = ShopExpenseStatus;
  readonly statusOptions = shopExpenseStatusOptions;
  readonly paymentMethodOptions = shopExpensePaymentMethodOptions;

  private readonly service = inject(ShopExpenseService);
  private readonly categoryService = inject(ShopExpenseCategoryService);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);

  readonly canCreate = this.permissions.getGrantedPolicy('ShopManagement.Expenses.Create');
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.Expenses.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.Expenses.Delete');
  readonly canPost = this.permissions.getGrantedPolicy('ShopManagement.Expenses.Post');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.Expenses.Cancel');
  readonly canViewAmount = this.permissions.getGrantedPolicy('ShopManagement.Expenses.ViewAmount');

  items: ShopExpenseDto[] = [];
  categories: ShopExpenseCategoryLookupDto[] = [];
  summary?: ShopExpenseSummaryDto;
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;
  actionInProgress = false;

  search = '';
  categoryFilter = '';
  statusFilter: ShopExpenseStatus | '' = '';
  paymentMethodFilter: ShopExpensePaymentMethod | '' = '';
  expenseDateFrom: string | null = null;
  expenseDateTo: string | null = null;

  cancelModalOpen = false;
  cancelTarget?: ShopExpenseDto;
  readonly cancelForm = this.fb.group({ cancellationReason: ['', [Validators.required, Validators.maxLength(500)]] });

  tooltipLang: 'en' | 'ur' = 'en';

  ngOnInit(): void {
    this.categoryService.getLookup().subscribe(result => (this.categories = result.items || []));
    this.load();
  }

  load(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;

    const input = {
      filter: this.search || undefined,
      expenseCategoryId: this.categoryFilter || undefined,
      status: this.statusFilter === '' ? undefined : this.statusFilter,
      paymentMethod: this.paymentMethodFilter === '' ? undefined : this.paymentMethodFilter,
      expenseDateFrom: this.expenseDateFrom || undefined,
      expenseDateTo: this.expenseDateTo || undefined,
    };

    this.service
      .getList({
        ...input,
        sorting: 'expenseDate desc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });

    if (this.canViewAmount) {
      // GetShopExpensesInput inherits paging fields for filter-shape consistency with GetListAsync,
      // but GetSummaryAsync never applies them - maxResultCount just needs to pass DTO validation.
      this.service.getSummary({ ...input, maxResultCount: 1, skipCount: 0 }).subscribe(summary => (this.summary = summary));
    }
  }

  isDraft(row: ShopExpenseDto): boolean {
    return row.status === ShopExpenseStatus.Draft;
  }

  isPosted(row: ShopExpenseDto): boolean {
    return row.status === ShopExpenseStatus.Posted;
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

  view(row: ShopExpenseDto): void {
    this.router.navigate(['/shop-management/expenses', row.id]);
  }

  create(): void {
    this.router.navigate(['/shop-management/expenses/create']);
  }

  edit(row: ShopExpenseDto): void {
    this.router.navigate(['/shop-management/expenses', row.id, 'edit']);
  }

  remove(row: ShopExpenseDto): void {
    this.confirmation.warn('::ConfirmDeleteExpense', row.expenseNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(row.id).subscribe({
        next: () => {
          this.toaster.success('::ExpenseDeletedSuccessfully');
          this.load();
        },
        error: e => this.showError(e),
      });
    });
  }

  post(row: ShopExpenseDto): void {
    this.confirmation.warn('::ConfirmPostExpense', row.expenseNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.actionInProgress = true;
      this.service
        .post(row.id)
        .pipe(finalize(() => (this.actionInProgress = false)))
        .subscribe({
          next: () => {
            this.toaster.success('::ExpensePostedSuccessfully');
            this.load();
          },
          error: e => this.showError(e),
        });
    });
  }

  openCancel(row: ShopExpenseDto): void {
    this.cancelTarget = row;
    this.cancelForm.reset();
    this.cancelModalOpen = true;
  }

  confirmCancel(): void {
    this.cancelForm.markAllAsTouched();
    if (this.cancelForm.invalid || !this.cancelTarget) return;
    this.actionInProgress = true;
    this.service
      .cancel(this.cancelTarget.id, this.cancelForm.getRawValue() as { cancellationReason: string })
      .pipe(finalize(() => (this.actionInProgress = false)))
      .subscribe({
        next: () => {
          this.cancelModalOpen = false;
          this.toaster.success('::ExpenseCancelledSuccessfully');
          this.load();
        },
        error: e => this.showError(e),
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
