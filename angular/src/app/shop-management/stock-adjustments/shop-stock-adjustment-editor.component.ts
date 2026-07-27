import { Component, OnInit, inject } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  CreateShopStockAdjustmentDto,
  CreateShopStockAdjustmentItemDto,
  ShopStockAdjustmentProductLookupDto,
  ShopStockAdjustmentReason,
  ShopStockAdjustmentService,
  ShopStockAdjustmentStatus,
  ShopStockAdjustmentType,
  UpdateShopStockAdjustmentDto,
  shopStockAdjustmentReasonOptions,
  shopStockAdjustmentTypeOptions,
} from '../../proxy/shop-management/stock-adjustments';
import { ShopProductBatchLookupDto, ShopProductBatchService } from '../../proxy/shop-management/product-batches';

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
    const quantity = control.get('adjustmentQuantity')?.value;
    if (allowDecimal || quantity == null) return null;
    return quantity !== Math.trunc(quantity) ? { wholeQuantityRequired: true } : null;
  };
}

function insufficientStockValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const type = control.get('adjustmentType')?.value;
    const systemStock = control.get('systemStock')?.value;
    const quantity = control.get('adjustmentQuantity')?.value;
    if (type !== ShopStockAdjustmentType.Decrease || systemStock == null || quantity == null) return null;
    return quantity > systemStock ? { insufficientStock: true } : null;
  };
}

function serialTrackingValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => (control.get('trackSerialNumber')?.value ? { serialTrackingNotSupported: true } : null);
}

function batchValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const trackBatch = control.get('trackBatch')?.value;
    if (!trackBatch) return null;
    const type = control.get('adjustmentType')?.value;
    if (type === ShopStockAdjustmentType.Decrease) {
      return control.get('productBatchId')?.value ? null : { batchRequired: true };
    }
    return control.get('batchNumber')?.value ? null : { batchRequired: true };
  };
}

function decreaseBatchQuantityValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const type = control.get('adjustmentType')?.value;
    if (type !== ShopStockAdjustmentType.Decrease) return null;
    const productBatchId = control.get('productBatchId')?.value;
    const available = control.get('selectedBatchAvailableQuantity')?.value;
    const quantity = control.get('adjustmentQuantity')?.value;
    if (!productBatchId || available == null || quantity == null) return null;
    return quantity > available ? { batchInsufficientStock: true } : null;
  };
}

function expiryValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const trackExpiry = control.get('trackExpiry')?.value;
    const type = control.get('adjustmentType')?.value;
    if (!trackExpiry || type !== ShopStockAdjustmentType.Increase) return null;
    return control.get('expiryDate')?.value ? null : { expiryRequired: true };
  };
}

@Component({ selector: 'app-shop-stock-adjustment-editor', standalone: false, templateUrl: './shop-stock-adjustment-editor.component.html', styleUrl: './shop-stock-adjustment-editor.component.scss' })
export class ShopStockAdjustmentEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopStockAdjustmentService);
  private readonly productBatchService = inject(ShopProductBatchService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);

  readonly ShopStockAdjustmentType = ShopStockAdjustmentType;
  readonly reasonOptions = shopStockAdjustmentReasonOptions;
  readonly typeOptions = shopStockAdjustmentTypeOptions;

  products: ShopStockAdjustmentProductLookupDto[] = [];
  editId?: string;
  adjustmentNumber = '';
  loading = false;
  submitting = false;
  helpOpen = false;

  get isEdit(): boolean {
    return !!this.editId;
  }

  readonly form = this.fb.group(
    {
      adjustmentDate: [this.todayIso(), Validators.required],
      reason: [ShopStockAdjustmentReason.Damaged, Validators.required],
      reasonDetails: ['', Validators.maxLength(500)],
      notes: ['', Validators.maxLength(1000)],
      items: this.fb.array<FormGroup>([]),
    },
    { validators: [duplicateProductValidator()] },
  );

  get items(): FormArray<FormGroup> {
    return this.form.controls.items as FormArray<FormGroup>;
  }

  ngOnInit(): void {
    this.editId = this.route.snapshot.paramMap.get('id') || undefined;

    this.service.getProductLookup().subscribe(result => (this.products = result.items || []));

    if (this.editId) this.loadForEdit(this.editId);
    else this.addItem();
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
      systemStock: product.currentStock,
      trackBatch: product.trackBatch,
      trackExpiry: product.trackExpiry,
      trackSerialNumber: product.trackSerialNumber,
      productBatchId: null,
      selectedBatchAvailableQuantity: null,
    });
    row.get('availableBatches')?.setValue([]);
    this.loadAvailableBatchesIfNeeded(index);
  }

  onTypeChange(index: number): void {
    const row = this.items.at(index);
    row.patchValue({ productBatchId: null, selectedBatchAvailableQuantity: null });
    this.loadAvailableBatchesIfNeeded(index);
  }

  onBatchSelected(index: number): void {
    const row = this.items.at(index);
    const batches: ShopProductBatchLookupDto[] = row.value.availableBatches || [];
    const batch = batches.find(x => x.id === row.value.productBatchId);
    row.patchValue({ selectedBatchAvailableQuantity: batch?.availableQuantity ?? null, expiryDate: batch?.expiryDate ? batch.expiryDate.substring(0, 10) : null });
  }

  private loadAvailableBatchesIfNeeded(index: number): void {
    const row = this.items.at(index);
    const productId = row.value.productId;
    if (!productId || !row.value.trackBatch || row.value.adjustmentType !== ShopStockAdjustmentType.Decrease) return;

    this.productBatchService.getAvailableBatches(productId).subscribe(result => {
      const batches = result.items || [];
      row.get('availableBatches')?.setValue(batches);
      const selected = batches.find(x => x.id === row.value.productBatchId);
      if (selected) row.get('selectedBatchAvailableQuantity')?.setValue(selected.availableQuantity);
    });
  }

  productOptions(index: number): ShopStockAdjustmentProductLookupDto[] {
    const currentValue = this.items.at(index).value.productId;
    const selectedElsewhere = this.items.controls.filter((_, i) => i !== index).map(x => x.value.productId);
    return this.products.filter(x => x.id === currentValue || !selectedElsewhere.includes(x.id));
  }

  finalQuantity(index: number): number {
    const row = this.items.at(index).value;
    const systemStock = row.systemStock || 0;
    const quantity = row.adjustmentQuantity || 0;
    return row.adjustmentType === ShopStockAdjustmentType.Decrease ? systemStock - quantity : systemStock + quantity;
  }

  expiryDateInvalid(index: number): boolean {
    const row = this.items.at(index).value;
    const adjustmentDate = this.form.controls.adjustmentDate.value;
    if (!row.expiryDate || !adjustmentDate) return false;
    return new Date(row.expiryDate) <= new Date(adjustmentDate);
  }

  save(): void {
    this.form.markAllAsTouched();
    this.items.controls.forEach(row => row.markAllAsTouched());
    const hasExpiryError = this.items.controls.some((_, i) => this.expiryDateInvalid(i));
    if (this.form.invalid || hasExpiryError || this.submitting) return;

    this.submitting = true;
    const raw = this.form.getRawValue();
    const items: CreateShopStockAdjustmentItemDto[] = raw.items.map(x => ({
      productId: x.productId!,
      adjustmentType: x.adjustmentType,
      adjustmentQuantity: x.adjustmentQuantity,
      batchNumber: x.batchNumber || undefined,
      manufacturingDate: x.manufacturingDate || undefined,
      expiryDate: x.expiryDate || undefined,
      productBatchId: x.productBatchId || undefined,
      reason: x.itemReason,
      notes: x.itemNotes || undefined,
    }));

    const input: CreateShopStockAdjustmentDto | UpdateShopStockAdjustmentDto = {
      adjustmentDate: raw.adjustmentDate!,
      reason: raw.reason,
      reasonDetails: raw.reasonDetails || undefined,
      notes: raw.notes || undefined,
      items,
    };

    const request = this.editId ? this.service.update(this.editId, input) : this.service.create(input);
    request.pipe(finalize(() => (this.submitting = false))).subscribe({
      next: saved => {
        this.toaster.success(this.isEdit ? '::StockAdjustmentUpdatedSuccessfully' : '::StockAdjustmentCreatedSuccessfully');
        this.router.navigate(['/shop-management/stock-adjustments', saved.id]);
      },
      error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
    });
  }

  cancel(): void {
    if (this.editId) this.router.navigate(['/shop-management/stock-adjustments', this.editId]);
    else this.router.navigate(['/shop-management/stock-adjustments']);
  }

  private loadForEdit(id: string): void {
    this.loading = true;
    this.service
      .get(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => {
        if (dto.status !== ShopStockAdjustmentStatus.Draft) {
          this.toaster.error('::StockAdjustmentCannotBeEdited');
          this.router.navigate(['/shop-management/stock-adjustments', id]);
          return;
        }

        this.adjustmentNumber = dto.adjustmentNumber || '';

        this.form.patchValue({
          adjustmentDate: dto.adjustmentDate!.substring(0, 10),
          reason: dto.reason,
          reasonDetails: dto.reasonDetails || '',
          notes: dto.notes || '',
        });

        this.items.clear();
        dto.items.forEach((item, index) => {
          const product = this.products.find(x => x.id === item.productId);
          this.items.push(
            this.createItemRow({
              productId: item.productId,
              unitName: item.unitName,
              unitShortName: item.unitShortName,
              allowDecimal: item.unitAllowDecimal,
              systemStock: product?.currentStock ?? item.systemQuantity,
              trackBatch: product?.trackBatch ?? false,
              trackExpiry: product?.trackExpiry ?? false,
              trackSerialNumber: product?.trackSerialNumber ?? false,
              adjustmentType: item.adjustmentType,
              adjustmentQuantity: item.adjustmentQuantity,
              batchNumber: item.batchNumber || '',
              manufacturingDate: item.manufacturingDate ? item.manufacturingDate.substring(0, 10) : null,
              expiryDate: item.expiryDate ? item.expiryDate.substring(0, 10) : null,
              productBatchId: item.productBatchId ?? null,
              itemReason: item.reason,
              itemNotes: item.notes || '',
            }),
          );
          this.loadAvailableBatchesIfNeeded(index);
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
        systemStock: [value.systemStock ?? null],
        trackBatch: [value.trackBatch ?? false],
        trackExpiry: [value.trackExpiry ?? false],
        trackSerialNumber: [value.trackSerialNumber ?? false],
        adjustmentType: [value.adjustmentType ?? ShopStockAdjustmentType.Increase, Validators.required],
        adjustmentQuantity: [value.adjustmentQuantity ?? 0, [Validators.required, Validators.min(0.0001)]],
        batchNumber: [value.batchNumber ?? '', Validators.maxLength(128)],
        manufacturingDate: [value.manufacturingDate ?? null],
        expiryDate: [value.expiryDate ?? null],
        productBatchId: [value.productBatchId ?? null],
        availableBatches: [[] as ShopProductBatchLookupDto[]],
        selectedBatchAvailableQuantity: [null as number | null],
        itemReason: [value.itemReason ?? this.form?.controls.reason.value ?? ShopStockAdjustmentReason.Damaged],
        itemNotes: [value.itemNotes ?? '', Validators.maxLength(500)],
      },
      { validators: [wholeQuantityValidator(), insufficientStockValidator(), decreaseBatchQuantityValidator(), serialTrackingValidator(), batchValidator(), expiryValidator()] },
    );
  }

  private todayIso(): string {
    return new Date().toISOString().substring(0, 10);
  }
}
