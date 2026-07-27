import { Component, OnInit, inject } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  CreateShopSaleReturnDto,
  CreateShopSaleReturnItemDto,
  ShopSaleReturnableDto,
  ShopSaleReturnReason,
  ShopSaleReturnService,
  ShopSaleReturnSettlementType,
  ShopSaleReturnStatus,
  UpdateShopSaleReturnDto,
  shopSaleReturnReasonOptions,
  shopSaleReturnSettlementTypeOptions,
} from '../../proxy/shop-management/sale-returns';
import { ShopBankAccountLookupDto, ShopBankAccountService } from '../../proxy/shop-management/bank-accounts';

function wholeQuantityValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const allowDecimal = control.get('allowDecimal')?.value;
    const quantity = control.get('returnQuantity')?.value;
    if (allowDecimal || quantity == null) return null;
    return quantity !== Math.trunc(quantity) ? { wholeQuantityRequired: true } : null;
  };
}

function quantityExceedsReturnableValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const returnableQuantity = control.get('returnableQuantity')?.value;
    const quantity = control.get('returnQuantity')?.value;
    if (returnableQuantity == null || quantity == null) return null;
    return quantity > returnableQuantity ? { quantityExceedsReturnable: true } : null;
  };
}

function atLeastOneReturnItemValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const items = (control.get('items') as FormArray)?.controls ?? [];
    const hasAny = items.some(x => (x.get('returnQuantity')?.value || 0) > 0);
    return hasAny ? null : { requiresReturnItem: true };
  };
}

function bankRefundValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const settlementType = control.get('settlementType')?.value;
    const bankAccountId = control.get('bankAccountId')?.value;
    return settlementType === ShopSaleReturnSettlementType.BankRefund && !bankAccountId ? { bankAccountRequired: true } : null;
  };
}

@Component({ selector: 'app-shop-sale-return-editor', standalone: false, templateUrl: './shop-sale-return-editor.component.html', styleUrl: './shop-sale-return-editor.component.scss' })
export class ShopSaleReturnEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopSaleReturnService);
  private readonly bankAccountService = inject(ShopBankAccountService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly reasonOptions = shopSaleReturnReasonOptions;
  readonly settlementTypeOptions = shopSaleReturnSettlementTypeOptions;
  readonly ShopSaleReturnSettlementType = ShopSaleReturnSettlementType;
  readonly canViewPrice = this.permissions.getGrantedPolicy('ShopManagement.SaleReturns.ViewPrice');
  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.SaleReturns.ViewCost');

  saleInfo?: ShopSaleReturnableDto;
  editId?: string;
  saleId = '';
  saleReturnNumber = '';
  bankAccounts: ShopBankAccountLookupDto[] = [];
  loading = false;
  submitting = false;

  get isEdit(): boolean {
    return !!this.editId;
  }

  get requiresBankAccount(): boolean {
    return this.form.controls.settlementType.value === ShopSaleReturnSettlementType.BankRefund;
  }

  readonly form = this.fb.group(
    {
      returnDate: [this.todayIso(), Validators.required],
      reason: [ShopSaleReturnReason.Damaged, Validators.required],
      reasonDetails: ['', Validators.maxLength(500)],
      settlementType: [ShopSaleReturnSettlementType.CustomerCredit, Validators.required],
      bankAccountId: [''],
      otherCharges: [0, Validators.min(0)],
      notes: ['', Validators.maxLength(1000)],
      items: this.fb.array<FormGroup>([]),
    },
    { validators: [atLeastOneReturnItemValidator(), bankRefundValidator()] },
  );

  get items(): FormArray<FormGroup> {
    return this.form.controls.items as FormArray<FormGroup>;
  }

  ngOnInit(): void {
    this.editId = this.route.snapshot.paramMap.get('id') || undefined;
    const routeSaleId = this.route.snapshot.paramMap.get('saleId');

    if (this.editId) {
      this.loadForEdit(this.editId);
    } else if (routeSaleId) {
      this.saleId = routeSaleId;
      this.loadSaleForReturn(routeSaleId);
    }

    this.bankAccountService.getLookup().subscribe(result => (this.bankAccounts = result.items || []));

    this.form.controls.settlementType.valueChanges.subscribe(settlementType => {
      if (settlementType !== ShopSaleReturnSettlementType.BankRefund) {
        this.form.controls.bankAccountId.setValue('');
      } else if (!this.form.controls.bankAccountId.value) {
        const defaultAccount = this.bankAccounts.find(a => a.isDefault);
        if (defaultAccount) this.form.controls.bankAccountId.setValue(defaultAccount.id);
      }
    });
  }

  lineSubTotal(index: number): number {
    const row = this.items.at(index).value;
    return round2((row.returnQuantity || 0) * (row.unitSalePrice || 0));
  }

  lineDiscountAmount(index: number): number {
    return round2(this.lineSubTotal(index) * ((this.items.at(index).value.discountPercentage || 0) / 100));
  }

  lineTaxAmount(index: number): number {
    const taxable = this.lineSubTotal(index) - this.lineDiscountAmount(index);
    return round2(taxable * ((this.items.at(index).value.taxPercentage || 0) / 100));
  }

  lineTotal(index: number): number {
    return round2(this.lineSubTotal(index) - this.lineDiscountAmount(index) + this.lineTaxAmount(index));
  }

  get subTotal(): number {
    return round2(this.items.controls.reduce((sum, _, i) => sum + this.lineSubTotal(i), 0));
  }

  get totalDiscount(): number {
    return round2(this.items.controls.reduce((sum, _, i) => sum + this.lineDiscountAmount(i), 0));
  }

  get totalTax(): number {
    return round2(this.items.controls.reduce((sum, _, i) => sum + this.lineTaxAmount(i), 0));
  }

  get grandTotal(): number {
    const raw = this.form.getRawValue();
    return round2(this.subTotal - this.totalDiscount + this.totalTax + (raw.otherCharges || 0));
  }

  save(): void {
    this.form.markAllAsTouched();
    this.items.controls.forEach(row => row.markAllAsTouched());
    if (this.form.invalid || this.submitting) return;

    this.submitting = true;
    const raw = this.form.getRawValue();
    const items: CreateShopSaleReturnItemDto[] = raw.items
      .filter(x => (x.returnQuantity || 0) > 0)
      .map(x => ({
        saleItemId: x.saleItemId!,
        returnQuantity: x.returnQuantity,
        reason: x.itemReason,
        notes: x.itemNotes || undefined,
      }));

    const request = this.editId
      ? this.service.update(this.editId, {
          returnDate: raw.returnDate!,
          reason: raw.reason,
          reasonDetails: raw.reasonDetails || undefined,
          settlementType: raw.settlementType,
          bankAccountId: raw.bankAccountId || undefined,
          otherCharges: raw.otherCharges,
          notes: raw.notes || undefined,
          items,
        } as UpdateShopSaleReturnDto)
      : this.service.create({
          saleId: this.saleId,
          returnDate: raw.returnDate!,
          reason: raw.reason,
          reasonDetails: raw.reasonDetails || undefined,
          settlementType: raw.settlementType,
          bankAccountId: raw.bankAccountId || undefined,
          otherCharges: raw.otherCharges,
          notes: raw.notes || undefined,
          items,
        } as CreateShopSaleReturnDto);

    request.pipe(finalize(() => (this.submitting = false))).subscribe({
      next: saved => {
        this.toaster.success(this.isEdit ? '::SaleReturnUpdatedSuccessfully' : '::SaleReturnCreatedSuccessfully');
        this.router.navigate(['/shop-management/sale-returns', saved.id]);
      },
      error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
    });
  }

  cancel(): void {
    if (this.editId) this.router.navigate(['/shop-management/sale-returns', this.editId]);
    else this.router.navigate(['/shop-management/sales', this.saleId]);
  }

  private loadSaleForReturn(saleId: string): void {
    this.loading = true;
    this.service
      .getSaleForReturn(saleId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: dto => {
          this.saleInfo = dto;
          this.buildRowsFromSaleInfo(dto);
        },
        error: e => {
          this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError');
          this.router.navigate(['/shop-management/sales']);
        },
      });
  }

  private loadForEdit(id: string): void {
    this.loading = true;
    this.service
      .get(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => {
        if (dto.status !== ShopSaleReturnStatus.Draft) {
          this.toaster.error('::SaleReturnCannotBeEdited');
          this.router.navigate(['/shop-management/sale-returns', id]);
          return;
        }

        this.saleReturnNumber = dto.saleReturnNumber || '';
        this.saleId = dto.saleId!;

        this.form.patchValue({
          returnDate: dto.returnDate!.substring(0, 10),
          reason: dto.reason,
          reasonDetails: dto.reasonDetails || '',
          settlementType: dto.settlementType,
          bankAccountId: dto.bankAccountId || '',
          otherCharges: dto.otherCharges ?? 0,
          notes: dto.notes || '',
        });

        const savedByItem = new Map(dto.items.map(x => [x.saleItemId, x]));

        this.service.getSaleForReturn(this.saleId).subscribe(saleInfo => {
          this.saleInfo = saleInfo;
          this.buildRowsFromSaleInfo(saleInfo, savedByItem);
        });
      });
  }

  private buildRowsFromSaleInfo(saleInfo: ShopSaleReturnableDto, savedByItem?: Map<string | undefined, any>): void {
    this.items.clear();
    const headerReason = this.form.controls.reason.value ?? ShopSaleReturnReason.Damaged;

    saleInfo.items.forEach(item => {
      const saved = savedByItem?.get(item.saleItemId);
      this.items.push(
        this.createItemRow({
          saleItemId: item.saleItemId,
          productCode: item.productCode,
          productName: item.productName,
          unitName: item.unitName,
          unitShortName: item.unitShortName,
          allowDecimal: item.unitAllowDecimal,
          soldQuantity: item.soldQuantity,
          previouslyReturnedQuantity: item.previouslyReturnedQuantity,
          returnableQuantity: saved ? item.returnableQuantity + saved.returnQuantity : item.returnableQuantity,
          unitSalePrice: item.unitSalePrice ?? 0,
          discountPercentage: item.discountPercentage,
          taxPercentage: item.taxPercentage,
          batchNumber: item.batchNumber || '',
          expiryDate: item.expiryDate ? item.expiryDate.substring(0, 10) : null,
          returnQuantity: saved ? saved.returnQuantity : 0,
          itemReason: saved ? saved.reason : headerReason,
          itemNotes: saved ? saved.notes || '' : '',
        }),
      );
    });
  }

  private createItemRow(value: Partial<Record<string, any>> = {}): FormGroup {
    return this.fb.group(
      {
        saleItemId: [value.saleItemId ?? null],
        productCode: [value.productCode ?? ''],
        productName: [value.productName ?? ''],
        unitName: [value.unitName ?? ''],
        unitShortName: [value.unitShortName ?? ''],
        allowDecimal: [value.allowDecimal ?? true],
        soldQuantity: [value.soldQuantity ?? 0],
        previouslyReturnedQuantity: [value.previouslyReturnedQuantity ?? 0],
        returnableQuantity: [value.returnableQuantity ?? 0],
        unitSalePrice: [value.unitSalePrice ?? 0],
        discountPercentage: [value.discountPercentage ?? 0],
        taxPercentage: [value.taxPercentage ?? 0],
        batchNumber: [value.batchNumber ?? ''],
        expiryDate: [value.expiryDate ?? null],
        returnQuantity: [value.returnQuantity ?? 0, [Validators.min(0)]],
        itemReason: [value.itemReason ?? ShopSaleReturnReason.Damaged],
        itemNotes: [value.itemNotes ?? '', Validators.maxLength(1000)],
      },
      { validators: [wholeQuantityValidator(), quantityExceedsReturnableValidator()] },
    );
  }

  private todayIso(): string {
    return new Date().toISOString().substring(0, 10);
  }
}

function round2(value: number): number {
  return Math.round((value + Number.EPSILON) * 100) / 100;
}
