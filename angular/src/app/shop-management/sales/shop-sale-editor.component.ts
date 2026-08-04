import { Component, OnInit, inject } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  CreateShopSaleDto,
  CreateShopSaleItemDto,
  ShopSaleProductLookupDto,
  ShopSaleService,
  ShopSaleStatus,
  ShopSaleType,
  shopSalePaymentMethodOptions,
  shopSaleTypeOptions,
  UpdateShopSaleDto,
} from '../../proxy/shop-management/sales';
import { ShopCustomerLookupDto, ShopCustomerService } from '../../proxy/shop-management/customers';

function dueDateValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const saleType = control.get('saleType')?.value;
    const saleDate = control.get('saleDate')?.value;
    const dueDate = control.get('dueDate')?.value;
    if (saleType === ShopSaleType.Credit && !dueDate) return { dueDateRequired: true };
    if (saleDate && dueDate && new Date(dueDate) < new Date(saleDate)) return { dueDateBeforeSaleDate: true };
    return null;
  };
}

function duplicateProductValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const items = (control.get('items') as FormArray)?.controls ?? [];
    const productIds = items.map(x => x.get('productId')?.value).filter((x): x is string => !!x);
    return new Set(productIds).size !== productIds.length ? { duplicateProduct: true } : null;
  };
}

function wholeQuantityValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const allowDecimal = control.get('allowDecimal')?.value;
    const quantity = control.get('quantity')?.value;
    if (allowDecimal || quantity == null) return null;
    return quantity !== Math.trunc(quantity) ? { wholeQuantityRequired: true } : null;
  };
}

function quantityExceedsStockValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const availableStock = control.get('availableStock')?.value;
    const quantity = control.get('quantity')?.value;
    if (availableStock == null || quantity == null) return null;
    return quantity > availableStock ? { quantityExceedsStock: true } : null;
  };
}

@Component({ selector: 'app-shop-sale-editor', standalone: false, templateUrl: './shop-sale-editor.component.html', styleUrl: './shop-sale-editor.component.scss' })
export class ShopSaleEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopSaleService);
  private readonly customerService = inject(ShopCustomerService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly ShopSaleType = ShopSaleType;
  readonly paymentMethodOptions = shopSalePaymentMethodOptions;
  readonly saleTypeOptions = shopSaleTypeOptions;
  readonly canViewPrice = this.permissions.getGrantedPolicy('ShopManagement.Sales.ViewPrice');
  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.Sales.ViewCost');

  customers: ShopCustomerLookupDto[] = [];
  products: ShopSaleProductLookupDto[] = [];
  editId?: string;
  loading = false;
  submitting = false;
  helpOpen = false;
  helpLang: 'en' | 'ur' = 'en';
  saleNumber = '';

  private paidAmountManuallyEdited = false;

  get isEdit(): boolean {
    return !!this.editId;
  }

  readonly form = this.fb.group(
    {
      customerId: [null as string | null, Validators.required],
      saleDate: [this.todayIso(), Validators.required],
      saleType: [ShopSaleType.Cash, Validators.required],
      dueDate: [null as string | null],
      paymentMethod: [0, Validators.required],
      paidAmount: [0, [Validators.required, Validators.min(0)]],
      referenceNumber: ['', Validators.maxLength(128)],
      otherCharges: [0, Validators.min(0)],
      notes: ['', Validators.maxLength(1000)],
      items: this.fb.array<FormGroup>([]),
    },
    { validators: [dueDateValidator(), duplicateProductValidator()] },
  );

  get items(): FormArray<FormGroup> {
    return this.form.controls.items as FormArray<FormGroup>;
  }

  ngOnInit(): void {
    this.editId = this.route.snapshot.paramMap.get('id') || undefined;

    this.customerService.getLookup().subscribe(result => (this.customers = result.items || []));
    this.service.getSaleProductLookup().subscribe(result => (this.products = result.items || []));

    if (this.editId) this.loadForEdit(this.editId);
    else this.addItem();

    // Any real (non-programmatic) change to Paid Amount stops it from auto-following the
    // grand total. Our own sync below always passes emitEvent: false, so this only fires
    // for user edits (typing) or the initial patchValue() when loading an existing sale.
    this.form.controls.paidAmount.valueChanges.subscribe(() => (this.paidAmountManuallyEdited = true));

    // Switching to Cash re-enables the "default paid amount to grand total" behavior.
    this.form.controls.saleType.valueChanges.subscribe(saleType => {
      if (saleType === ShopSaleType.Cash) this.paidAmountManuallyEdited = false;
    });

    this.form.valueChanges.subscribe(() => {
      if (this.form.controls.saleType.value === ShopSaleType.Cash && !this.paidAmountManuallyEdited) {
        const gt = this.grandTotal;
        if (this.form.controls.paidAmount.value !== gt) {
          this.form.controls.paidAmount.setValue(gt, { emitEvent: false });
        }
      }
    });
  }

  addItem(): void {
    this.items.push(this.createItemRow());
  }

  removeItem(index: number): void {
    if (this.items.length > 1) this.items.removeAt(index);
  }

  onProductChange(index: number): void {
    const row = this.items.at(index);
    const productId = row.value.productId;
    const product = this.products.find(x => x.id === productId);
    if (!product) return;

    row.patchValue({
      unitName: product.unitName,
      unitShortName: product.unitShortName,
      allowDecimal: product.unitAllowDecimal,
      availableStock: product.currentStock,
      trackBatch: product.trackBatch,
      trackExpiry: product.trackExpiry,
      unitSalePrice: product.salePrice ?? 0,
    });
  }

  productOptions(index: number): ShopSaleProductLookupDto[] {
    const currentValue = this.items.at(index).value.productId;
    const selectedElsewhere = this.items.controls.filter((_, i) => i !== index).map(x => x.value.productId);
    return this.products.filter(x => x.id === currentValue || !selectedElsewhere.includes(x.id));
  }

  lineSubTotal(index: number): number {
    const row = this.items.at(index).value;
    return round2((row.quantity || 0) * (row.unitSalePrice || 0));
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

  get pendingAmount(): number {
    return round2(this.grandTotal - (this.form.value.paidAmount || 0));
  }

  get paidAmountExceedsGrandTotal(): boolean {
    return (this.form.value.paidAmount || 0) > this.grandTotal;
  }

  save(): void {
    this.form.markAllAsTouched();
    this.items.controls.forEach(row => row.markAllAsTouched());
    if (this.form.invalid || this.paidAmountExceedsGrandTotal || this.submitting) return;

    this.submitting = true;
    const raw = this.form.getRawValue();
    const items: CreateShopSaleItemDto[] = raw.items.map(x => ({
      productId: x.productId!,
      quantity: x.quantity,
      unitSalePrice: x.unitSalePrice,
      discountPercentage: x.discountPercentage,
      taxPercentage: x.taxPercentage,
      batchNumber: x.batchNumber || undefined,
      expiryDate: x.expiryDate || undefined,
    }));

    const input: CreateShopSaleDto | UpdateShopSaleDto = {
      customerId: raw.customerId!,
      saleDate: raw.saleDate!,
      dueDate: raw.dueDate || undefined,
      saleType: raw.saleType,
      paymentMethod: raw.paymentMethod,
      paidAmount: raw.paidAmount,
      referenceNumber: raw.referenceNumber || undefined,
      otherCharges: raw.otherCharges,
      notes: raw.notes || undefined,
      items,
    };

    const request = this.editId ? this.service.update(this.editId, input) : this.service.create(input);
    request.pipe(finalize(() => (this.submitting = false))).subscribe({
      next: saved => {
        this.toaster.success(this.isEdit ? '::SaleUpdatedSuccessfully' : '::SaleCreatedSuccessfully');
        this.router.navigate(['/shop-management/sales', saved.id]);
      },
      error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
    });
  }

  cancel(): void {
    this.router.navigate(['/shop-management/sales']);
  }

  private loadForEdit(id: string): void {
    this.loading = true;
    this.service
      .get(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => {
        if (dto.status !== ShopSaleStatus.Draft) {
          this.toaster.error('::SaleCannotBeEdited');
          this.router.navigate(['/shop-management/sales', id]);
          return;
        }

        this.saleNumber = dto.saleNumber;
        this.form.patchValue({
          customerId: dto.customerId,
          saleDate: dto.saleDate.substring(0, 10),
          saleType: dto.saleType,
          dueDate: dto.dueDate ? dto.dueDate.substring(0, 10) : null,
          paymentMethod: dto.paymentMethod,
          paidAmount: dto.paidAmount ?? 0,
          referenceNumber: dto.referenceNumber || '',
          otherCharges: dto.otherCharges ?? 0,
          notes: dto.notes || '',
        });

        this.items.clear();
        dto.items.forEach(item => {
          const product = this.products.find(x => x.id === item.productId);
          this.items.push(
            this.createItemRow({
              productId: item.productId,
              unitName: item.unitName,
              unitShortName: item.unitShortName,
              allowDecimal: item.unitAllowDecimal,
              availableStock: product?.currentStock ?? item.quantity,
              trackBatch: product?.trackBatch ?? false,
              trackExpiry: product?.trackExpiry ?? false,
              quantity: item.quantity,
              unitSalePrice: item.unitSalePrice ?? 0,
              discountPercentage: item.discountPercentage,
              taxPercentage: item.taxPercentage,
              batchNumber: item.batchNumber || '',
              expiryDate: item.expiryDate ? item.expiryDate.substring(0, 10) : null,
            }),
          );
        });
      });
  }

  private createItemRow(value: Partial<Record<string, any>> = {}): FormGroup {
    return this.fb.group(
      {
        productId: [value.productId ?? null, Validators.required],
        unitName: [value.unitName ?? ''],
        unitShortName: [value.unitShortName ?? ''],
        allowDecimal: [value.allowDecimal ?? true],
        availableStock: [value.availableStock ?? null],
        trackBatch: [value.trackBatch ?? false],
        trackExpiry: [value.trackExpiry ?? false],
        quantity: [value.quantity ?? 1, [Validators.required, Validators.min(0.0001)]],
        unitSalePrice: [value.unitSalePrice ?? 0, [Validators.required, Validators.min(0)]],
        discountPercentage: [value.discountPercentage ?? 0, [Validators.min(0), Validators.max(100)]],
        taxPercentage: [value.taxPercentage ?? 0, [Validators.min(0), Validators.max(100)]],
        batchNumber: [value.batchNumber ?? '', Validators.maxLength(128)],
        expiryDate: [value.expiryDate ?? null],
      },
      { validators: [wholeQuantityValidator(), quantityExceedsStockValidator()] },
    );
  }

  private todayIso(): string {
    return new Date().toISOString().substring(0, 10);
  }
}

function round2(value: number): number {
  return Math.round((value + Number.EPSILON) * 100) / 100;
}
