import { Component, OnInit, inject } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  CreateUpdateShopCustomerPaymentAllocationDto,
  CreateUpdateShopCustomerPaymentDto,
  ShopCustomerOutstandingSaleDto,
  ShopCustomerPaymentMethod,
  ShopCustomerPaymentService,
  ShopCustomerPaymentStatus,
  ShopCustomerPaymentType,
  shopCustomerPaymentMethodOptions,
  shopCustomerPaymentTypeOptions,
} from '../../proxy/shop-management/customer-payments';
import { ShopSaleService } from '../../proxy/shop-management/sales';
import { ShopCustomerLookupDto, ShopCustomerService } from '../../proxy/shop-management/customers';

function paymentMethodValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const method = control.get('paymentMethod')?.value;
    const chequeNumber = control.get('chequeNumber')?.value;
    const bankName = control.get('bankName')?.value;

    const errors: ValidationErrors = {};
    if (method === ShopCustomerPaymentMethod.Cheque && !chequeNumber) errors['chequeNumberRequired'] = true;
    if ((method === ShopCustomerPaymentMethod.Cheque || method === ShopCustomerPaymentMethod.BankTransfer) && !bankName) errors['bankNameRequired'] = true;
    return Object.keys(errors).length ? errors : null;
  };
}

function allocationTotalValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const amount = control.get('amount')?.value || 0;
    const paymentType = control.get('paymentType')?.value;
    const allocations = (control.get('allocations') as FormArray)?.controls ?? [];
    const total = allocations.reduce((sum, row) => sum + (row.get('allocateAmount')?.value || 0), 0);

    const errors: ValidationErrors = {};
    if (total > amount) errors['allocationExceedsAmount'] = true;
    if (paymentType === ShopCustomerPaymentType.InvoicePayment && total <= 0) errors['requiresAllocation'] = true;
    return Object.keys(errors).length ? errors : null;
  };
}

function allocationRowValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const allocateAmount = control.get('allocateAmount')?.value;
    const pendingAmount = control.get('pendingAmount')?.value;

    const errors: ValidationErrors = {};
    if (allocateAmount < 0) errors['negative'] = true;
    else if (pendingAmount != null && allocateAmount > pendingAmount) errors['exceedsPending'] = true;
    return Object.keys(errors).length ? errors : null;
  };
}

@Component({ selector: 'app-shop-customer-payment-editor', standalone: false, templateUrl: './shop-customer-payment-editor.component.html', styleUrl: './shop-customer-payment-editor.component.scss' })
export class ShopCustomerPaymentEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopCustomerPaymentService);
  private readonly saleService = inject(ShopSaleService);
  private readonly customerService = inject(ShopCustomerService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canViewAmount = this.permissions.getGrantedPolicy('ShopManagement.CustomerPayments.ViewAmount');
  readonly ShopCustomerPaymentMethod = ShopCustomerPaymentMethod;
  readonly typeOptions = shopCustomerPaymentTypeOptions;
  readonly methodOptions = shopCustomerPaymentMethodOptions;

  editId?: string;
  customers: ShopCustomerLookupDto[] = [];
  loading = false;
  loadingSales = false;
  submitting = false;
  submitted = false;
  helpOpen = false;
  customerLocked = false;

  private existingAllocations: { saleId: string; allocatedAmount?: number }[] = [];
  private preselectedSaleId?: string;
  private amountManuallyEdited = false;

  get isEdit(): boolean {
    return !!this.editId;
  }

  readonly form = this.fb.group(
    {
      customerId: ['', Validators.required],
      paymentDate: [this.todayIso(), Validators.required],
      paymentType: [ShopCustomerPaymentType.InvoicePayment, Validators.required],
      paymentMethod: [ShopCustomerPaymentMethod.Cash, Validators.required],
      amount: [0, [Validators.required, Validators.min(0.01)]],
      referenceNumber: ['', Validators.maxLength(128)],
      chequeNumber: ['', Validators.maxLength(64)],
      bankName: ['', Validators.maxLength(128)],
      notes: ['', Validators.maxLength(1000)],
      allocations: this.fb.array<FormGroup>([]),
    },
    { validators: [paymentMethodValidator(), allocationTotalValidator()] },
  );

  get allocations(): FormArray<FormGroup> {
    return this.form.controls.allocations as FormArray<FormGroup>;
  }

  get totalAllocated(): number {
    return round2(this.allocations.controls.reduce((sum, row) => sum + (row.value.allocateAmount || 0), 0));
  }

  get totalPending(): number {
    return round2(this.allocations.controls.reduce((sum, row) => sum + (row.value.pendingAmount || 0), 0));
  }

  ngOnInit(): void {
    this.customerService.getLookup().subscribe(result => (this.customers = result.items || []));

    this.editId = this.route.snapshot.paramMap.get('id') || undefined;
    const saleId = this.route.snapshot.queryParamMap.get('saleId') || undefined;

    if (this.editId) {
      this.loadForEdit(this.editId);
    } else if (saleId) {
      this.loadForSale(saleId);
    }

    this.form.controls.customerId.valueChanges.subscribe(customerId => {
      if (customerId) this.loadOutstandingSales(customerId);
      else this.allocations.clear();
    });

    // Any real (non-programmatic) change to Amount stops it from auto-following the
    // allocated total. Our own sync below always passes emitEvent: false, so this only
    // fires for user edits (typing) or the initial patchValue() when loading an existing payment.
    this.form.controls.amount.valueChanges.subscribe(() => (this.amountManuallyEdited = true));

    this.form.valueChanges.subscribe(() => {
      if (!this.amountManuallyEdited) {
        const total = this.totalAllocated;
        if (total > 0 && this.form.controls.amount.value !== total) {
          this.form.controls.amount.setValue(total, { emitEvent: false });
        }
      }
    });
  }

  resetCustomerLock(): void {
    this.customerLocked = false;
    this.preselectedSaleId = undefined;
  }

  save(): void {
    this.submitted = true;
    this.form.markAllAsTouched();
    this.allocations.controls.forEach(row => row.markAllAsTouched());
    if (this.form.invalid || this.submitting) return;

    this.submitting = true;
    const raw = this.form.getRawValue();
    const allocations: CreateUpdateShopCustomerPaymentAllocationDto[] = raw.allocations
      .filter(x => (x.allocateAmount || 0) > 0)
      .map(x => ({ saleId: x.saleId!, allocatedAmount: x.allocateAmount }));

    const input: CreateUpdateShopCustomerPaymentDto = {
      customerId: raw.customerId!,
      paymentDate: raw.paymentDate!,
      paymentType: raw.paymentType!,
      paymentMethod: raw.paymentMethod!,
      amount: raw.amount!,
      referenceNumber: raw.referenceNumber || undefined,
      chequeNumber: raw.chequeNumber || undefined,
      bankName: raw.bankName || undefined,
      notes: raw.notes || undefined,
      allocations,
    };

    const request = this.editId ? this.service.update(this.editId, input) : this.service.create(input);
    request.pipe(finalize(() => (this.submitting = false))).subscribe({
      next: saved => {
        this.toaster.success(this.isEdit ? '::CustomerPaymentUpdatedSuccessfully' : '::CustomerPaymentCreatedSuccessfully');
        this.router.navigate(['/shop-management/customer-payments', saved.id]);
      },
      error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
    });
  }

  cancel(): void {
    this.router.navigate(['/shop-management/customer-payments']);
  }

  private loadForEdit(id: string): void {
    this.loading = true;
    this.service
      .get(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => {
        if (dto.status !== ShopCustomerPaymentStatus.Draft) {
          this.toaster.error('::CustomerPaymentCannotBeEdited');
          this.router.navigate(['/shop-management/customer-payments', id]);
          return;
        }

        this.existingAllocations = (dto.allocations || []).map(a => ({ saleId: a.saleId!, allocatedAmount: a.allocatedAmount }));

        this.form.patchValue({
          customerId: dto.customerId,
          paymentDate: dto.paymentDate ? dto.paymentDate.substring(0, 10) : this.todayIso(),
          paymentType: dto.paymentType,
          paymentMethod: dto.paymentMethod,
          amount: dto.amount ?? 0,
          referenceNumber: dto.referenceNumber || '',
          chequeNumber: dto.chequeNumber || '',
          bankName: dto.bankName || '',
          notes: dto.notes || '',
        });

        if (dto.customerId) this.loadOutstandingSales(dto.customerId);
      });
  }

  private loadForSale(saleId: string): void {
    this.loading = true;
    this.saleService
      .get(saleId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: sale => {
          this.preselectedSaleId = saleId;
          this.customerLocked = true;
          this.form.patchValue({ customerId: sale.customerId });
        },
        error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
      });
  }

  private loadOutstandingSales(customerId: string): void {
    this.loadingSales = true;
    this.service
      .getOutstandingSales(customerId)
      .pipe(finalize(() => (this.loadingSales = false)))
      .subscribe(result => {
        const sales = result.items || [];
        this.allocations.clear();
        sales.forEach(sale => {
          const existing = this.existingAllocations.find(a => a.saleId === sale.saleId);
          let allocateAmount = existing?.allocatedAmount ?? 0;

          if (!existing && this.preselectedSaleId && this.preselectedSaleId === sale.saleId) {
            allocateAmount = sale.pendingAmount ?? 0;
            this.form.controls.amount.setValue(sale.pendingAmount ?? 0, { emitEvent: false });
          }

          this.allocations.push(this.createAllocationRow(sale, allocateAmount));
        });
      });
  }

  private createAllocationRow(sale: ShopCustomerOutstandingSaleDto, allocateAmount: number): FormGroup {
    return this.fb.group(
      {
        saleId: [sale.saleId],
        saleNumber: [sale.saleNumber],
        saleDate: [sale.saleDate],
        dueDate: [sale.dueDate],
        grandTotal: [sale.grandTotal],
        initialPaidAmount: [sale.initialPaidAmount],
        additionalPaidAmount: [sale.additionalPaidAmount],
        totalPaidAmount: [sale.totalPaidAmount],
        pendingAmount: [sale.pendingAmount],
        allocateAmount: [allocateAmount, [Validators.min(0)]],
      },
      { validators: allocationRowValidator() },
    );
  }

  private todayIso(): string {
    return new Date().toISOString().substring(0, 10);
  }
}

function round2(value: number): number {
  return Math.round((value + Number.EPSILON) * 100) / 100;
}
