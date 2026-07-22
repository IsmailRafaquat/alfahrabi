import { Component, OnInit, inject } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  CreateUpdateShopSupplierPaymentAllocationDto,
  CreateUpdateShopSupplierPaymentDto,
  ShopSupplierOutstandingReceiptDto,
  ShopSupplierPaymentMethod,
  ShopSupplierPaymentService,
  ShopSupplierPaymentStatus,
  ShopSupplierPaymentType,
  shopSupplierPaymentMethodOptions,
  shopSupplierPaymentTypeOptions,
} from '../../proxy/shop-management/supplier-payments';
import { ShopGoodsReceiptService } from '../../proxy/shop-management/goods-receipts';
import { ShopSupplierLookupDto, ShopSupplierService } from '../../proxy/shop-management/suppliers';

function paymentMethodValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const method = control.get('paymentMethod')?.value;
    const chequeNumber = control.get('chequeNumber')?.value;
    const bankName = control.get('bankName')?.value;

    const errors: ValidationErrors = {};
    if (method === ShopSupplierPaymentMethod.Cheque && !chequeNumber) errors['chequeNumberRequired'] = true;
    if ((method === ShopSupplierPaymentMethod.Cheque || method === ShopSupplierPaymentMethod.BankTransfer) && !bankName) errors['bankNameRequired'] = true;
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
    if (paymentType === ShopSupplierPaymentType.InvoicePayment && total <= 0) errors['requiresAllocation'] = true;
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

@Component({ selector: 'app-shop-supplier-payment-editor', standalone: false, templateUrl: './shop-supplier-payment-editor.component.html', styleUrl: './shop-supplier-payment-editor.component.scss' })
export class ShopSupplierPaymentEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopSupplierPaymentService);
  private readonly goodsReceiptService = inject(ShopGoodsReceiptService);
  private readonly supplierService = inject(ShopSupplierService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canViewAmount = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.ViewAmount');
  readonly ShopSupplierPaymentMethod = ShopSupplierPaymentMethod;
  readonly typeOptions = shopSupplierPaymentTypeOptions;
  readonly methodOptions = shopSupplierPaymentMethodOptions;

  editId?: string;
  suppliers: ShopSupplierLookupDto[] = [];
  loading = false;
  loadingReceipts = false;
  submitting = false;
  supplierLocked = false;

  private existingAllocations: { goodsReceiptId: string; allocatedAmount?: number }[] = [];
  private preselectedGoodsReceiptId?: string;

  get isEdit(): boolean {
    return !!this.editId;
  }

  readonly form = this.fb.group(
    {
      supplierId: ['', Validators.required],
      paymentDate: [this.todayIso(), Validators.required],
      paymentType: [ShopSupplierPaymentType.InvoicePayment, Validators.required],
      paymentMethod: [ShopSupplierPaymentMethod.Cash, Validators.required],
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

  ngOnInit(): void {
    this.supplierService.getLookup().subscribe(result => (this.suppliers = result.items || []));

    this.editId = this.route.snapshot.paramMap.get('id') || undefined;
    const goodsReceiptId = this.route.snapshot.queryParamMap.get('goodsReceiptId') || undefined;

    if (this.editId) {
      this.loadForEdit(this.editId);
    } else if (goodsReceiptId) {
      this.loadForGoodsReceipt(goodsReceiptId);
    }

    this.form.controls.supplierId.valueChanges.subscribe(supplierId => {
      if (supplierId) this.loadOutstandingReceipts(supplierId);
      else this.allocations.clear();
    });
  }

  resetSupplierLock(): void {
    this.supplierLocked = false;
    this.preselectedGoodsReceiptId = undefined;
  }

  save(): void {
    this.form.markAllAsTouched();
    this.allocations.controls.forEach(row => row.markAllAsTouched());
    if (this.form.invalid || this.submitting) return;

    this.submitting = true;
    const raw = this.form.getRawValue();
    const allocations: CreateUpdateShopSupplierPaymentAllocationDto[] = raw.allocations
      .filter(x => (x.allocateAmount || 0) > 0)
      .map(x => ({ goodsReceiptId: x.goodsReceiptId!, allocatedAmount: x.allocateAmount }));

    const input: CreateUpdateShopSupplierPaymentDto = {
      supplierId: raw.supplierId!,
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
        this.toaster.success(this.isEdit ? '::SupplierPaymentUpdatedSuccessfully' : '::SupplierPaymentCreatedSuccessfully');
        this.router.navigate(['/shop-management/supplier-payments', saved.id]);
      },
      error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
    });
  }

  cancel(): void {
    this.router.navigate(['/shop-management/supplier-payments']);
  }

  private loadForEdit(id: string): void {
    this.loading = true;
    this.service
      .get(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => {
        if (dto.status !== ShopSupplierPaymentStatus.Draft) {
          this.toaster.error('::SupplierPaymentCannotBeEdited');
          this.router.navigate(['/shop-management/supplier-payments', id]);
          return;
        }

        this.existingAllocations = (dto.allocations || []).map(a => ({ goodsReceiptId: a.goodsReceiptId!, allocatedAmount: a.allocatedAmount }));

        this.form.patchValue({
          supplierId: dto.supplierId,
          paymentDate: dto.paymentDate ? dto.paymentDate.substring(0, 10) : this.todayIso(),
          paymentType: dto.paymentType,
          paymentMethod: dto.paymentMethod,
          amount: dto.amount ?? 0,
          referenceNumber: dto.referenceNumber || '',
          chequeNumber: dto.chequeNumber || '',
          bankName: dto.bankName || '',
          notes: dto.notes || '',
        });

        if (dto.supplierId) this.loadOutstandingReceipts(dto.supplierId);
      });
  }

  private loadForGoodsReceipt(goodsReceiptId: string): void {
    this.loading = true;
    this.goodsReceiptService
      .get(goodsReceiptId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: gr => {
          this.preselectedGoodsReceiptId = goodsReceiptId;
          this.supplierLocked = true;
          this.form.patchValue({ supplierId: gr.supplierId });
        },
        error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
      });
  }

  private loadOutstandingReceipts(supplierId: string): void {
    this.loadingReceipts = true;
    this.service
      .getOutstandingReceipts(supplierId)
      .pipe(finalize(() => (this.loadingReceipts = false)))
      .subscribe(receipts => {
        this.allocations.clear();
        receipts.forEach(receipt => {
          const existing = this.existingAllocations.find(a => a.goodsReceiptId === receipt.goodsReceiptId);
          let allocateAmount = existing?.allocatedAmount ?? 0;

          if (!existing && this.preselectedGoodsReceiptId && this.preselectedGoodsReceiptId === receipt.goodsReceiptId) {
            allocateAmount = receipt.pendingAmount ?? 0;
            this.form.patchValue({ amount: receipt.pendingAmount ?? 0 });
          }

          this.allocations.push(this.createAllocationRow(receipt, allocateAmount));
        });
      });
  }

  private createAllocationRow(receipt: ShopSupplierOutstandingReceiptDto, allocateAmount: number): FormGroup {
    return this.fb.group(
      {
        goodsReceiptId: [receipt.goodsReceiptId],
        goodsReceiptNumber: [receipt.goodsReceiptNumber],
        supplierInvoiceNumber: [receipt.supplierInvoiceNumber],
        receiptDate: [receipt.receiptDate],
        grandTotal: [receipt.grandTotal],
        paidAmount: [receipt.paidAmount],
        pendingAmount: [receipt.pendingAmount],
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
