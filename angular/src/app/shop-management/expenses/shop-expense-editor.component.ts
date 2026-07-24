import { Component, OnInit, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  CreateUpdateShopExpenseDto,
  ShopExpensePaymentMethod,
  ShopExpenseService,
  ShopExpenseStatus,
  shopExpensePaymentMethodOptions,
} from '../../proxy/shop-management/expenses';
import { ShopExpenseCategoryLookupDto, ShopExpenseCategoryService } from '../../proxy/shop-management/expense-categories';

function chequeAndBankValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const paymentMethod = control.get('paymentMethod')?.value;
    const chequeNumber = (control.get('chequeNumber')?.value || '').trim();
    const bankName = (control.get('bankName')?.value || '').trim();

    const errors: ValidationErrors = {};
    if (paymentMethod === ShopExpensePaymentMethod.Cheque && !chequeNumber) errors['chequeNumberRequired'] = true;
    if ((paymentMethod === ShopExpensePaymentMethod.Cheque || paymentMethod === ShopExpensePaymentMethod.BankTransfer) && !bankName)
      errors['bankNameRequired'] = true;

    return Object.keys(errors).length ? errors : null;
  };
}

@Component({ selector: 'app-shop-expense-editor', standalone: false, templateUrl: './shop-expense-editor.component.html', styleUrl: './shop-expense-editor.component.scss' })
export class ShopExpenseEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopExpenseService);
  private readonly categoryService = inject(ShopExpenseCategoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);

  readonly ShopExpensePaymentMethod = ShopExpensePaymentMethod;
  readonly paymentMethodOptions = shopExpensePaymentMethodOptions;

  categories: ShopExpenseCategoryLookupDto[] = [];
  editId?: string;
  loading = false;
  submitting = false;
  expenseNumber = '';
  helpOpen = false;

  get isEdit(): boolean {
    return !!this.editId;
  }

  readonly form = this.fb.group(
    {
      expenseCategoryId: [null as string | null, Validators.required],
      expenseDate: [this.todayIso(), Validators.required],
      amount: [0, [Validators.required, Validators.min(0.01)]],
      paymentMethod: [ShopExpensePaymentMethod.Cash, Validators.required],
      paidTo: ['', Validators.maxLength(200)],
      referenceNumber: ['', Validators.maxLength(128)],
      chequeNumber: ['', Validators.maxLength(64)],
      bankName: ['', Validators.maxLength(128)],
      description: ['', Validators.maxLength(500)],
      notes: ['', Validators.maxLength(1000)],
    },
    { validators: [chequeAndBankValidator()] },
  );

  ngOnInit(): void {
    this.editId = this.route.snapshot.paramMap.get('id') || undefined;

    this.categoryService.getLookup().subscribe(result => {
      this.categories = result.items || [];
      if (this.editId) this.loadForEdit(this.editId);
    });
  }

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting) return;

    this.submitting = true;
    const raw = this.form.getRawValue();
    const input: CreateUpdateShopExpenseDto = {
      expenseCategoryId: raw.expenseCategoryId!,
      expenseDate: raw.expenseDate!,
      amount: raw.amount!,
      paymentMethod: raw.paymentMethod!,
      paidTo: raw.paidTo || undefined,
      referenceNumber: raw.referenceNumber || undefined,
      chequeNumber: raw.chequeNumber || undefined,
      bankName: raw.bankName || undefined,
      description: raw.description || undefined,
      notes: raw.notes || undefined,
    };

    const request = this.editId ? this.service.update(this.editId, input) : this.service.create(input);
    request.pipe(finalize(() => (this.submitting = false))).subscribe({
      next: saved => {
        this.toaster.success(this.isEdit ? '::ExpenseUpdatedSuccessfully' : '::ExpenseCreatedSuccessfully');
        this.router.navigate(['/shop-management/expenses', saved.id]);
      },
      error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
    });
  }

  cancel(): void {
    this.router.navigate(['/shop-management/expenses']);
  }

  private loadForEdit(id: string): void {
    this.loading = true;
    this.service
      .get(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => {
        if (dto.status !== ShopExpenseStatus.Draft) {
          this.toaster.error('::ExpenseCannotBeEdited');
          this.router.navigate(['/shop-management/expenses', id]);
          return;
        }

        this.expenseNumber = dto.expenseNumber || '';

        // The active-only category lookup may not include this expense's own category if it
        // was deactivated after creation; keep it selectable so the current value still displays.
        if (dto.expenseCategoryId && !this.categories.some(x => x.id === dto.expenseCategoryId)) {
          this.categories = [...this.categories, { id: dto.expenseCategoryId, code: dto.expenseCategoryCode, name: dto.expenseCategoryName }];
        }

        this.form.patchValue({
          expenseCategoryId: dto.expenseCategoryId,
          expenseDate: dto.expenseDate ? dto.expenseDate.substring(0, 10) : this.todayIso(),
          amount: dto.amount ?? 0,
          paymentMethod: dto.paymentMethod,
          paidTo: dto.paidTo || '',
          referenceNumber: dto.referenceNumber || '',
          chequeNumber: dto.chequeNumber || '',
          bankName: dto.bankName || '',
          description: dto.description || '',
          notes: dto.notes || '',
        });
      });
  }

  private todayIso(): string {
    return new Date().toISOString().substring(0, 10);
  }
}
