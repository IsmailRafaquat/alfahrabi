import { Component, OnInit, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { CreateShopProductDto, ShopProductDto, ShopProductService, UpdateShopProductDto } from '../../proxy/shop-management/products';
import { ShopProductCategoryLookupDto, ShopProductCategoryService } from '../../proxy/shop-management/product-categories';
import { ShopUnitLookupDto, ShopUnitService } from '../../proxy/shop-management/units';

function minimumSalePriceValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const salePrice = control.get('salePrice')?.value;
    const minimumSalePrice = control.get('minimumSalePrice')?.value;
    if (minimumSalePrice == null || salePrice == null) return null;
    return minimumSalePrice > salePrice ? { minimumSalePriceExceedsSalePrice: true } : null;
  };
}

function maximumStockValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const minimumStockLevel = control.get('minimumStockLevel')?.value;
    const maximumStockLevel = control.get('maximumStockLevel')?.value;
    if (!maximumStockLevel || minimumStockLevel == null) return null;
    return maximumStockLevel > 0 && maximumStockLevel < minimumStockLevel ? { maximumStockBelowMinimum: true } : null;
  };
}

const BLANK_VALUE = {
  categoryId: null as string | null,
  unitId: null as string | null,
  name: '',
  code: '',
  sku: '',
  barcode: '',
  description: '',
  brand: '',
  model: '',
  purchasePrice: 0,
  salePrice: 0,
  wholesalePrice: null as number | null,
  minimumSalePrice: null as number | null,
  isTaxable: false,
  taxPercentage: 0,
  minimumStockLevel: 0,
  maximumStockLevel: null as number | null,
  reorderLevel: 0,
  trackBatch: false,
  trackExpiry: false,
  expiryAlertDays: null as number | null,
  blockExpiredSale: true,
  trackSerialNumber: false,
  isActive: true,
};

@Component({ selector: 'app-shop-product-editor', standalone: false, templateUrl: './shop-product-editor.component.html', styleUrl: './shop-product-editor.component.scss' })
export class ShopProductEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopProductService);
  private readonly categoryService = inject(ShopProductCategoryService);
  private readonly unitService = inject(ShopUnitService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.Products.ViewCost');

  categories: ShopProductCategoryLookupDto[] = [];
  units: ShopUnitLookupDto[] = [];
  editId?: string;
  loading = false;
  submitting = false;
  currentStock = 0;
  savedItems: ShopProductDto[] = [];
  helpOpen = false;
  helpLang: 'en' | 'ur' = 'en';

  get isEdit(): boolean {
    return !!this.editId;
  }

  readonly form = this.fb.group(
    {
      categoryId: [BLANK_VALUE.categoryId, Validators.required],
      unitId: [BLANK_VALUE.unitId, Validators.required],
      name: [BLANK_VALUE.name, [Validators.required, Validators.maxLength(200)]],
      code: [BLANK_VALUE.code, [Validators.required, Validators.maxLength(64)]],
      sku: [BLANK_VALUE.sku, Validators.maxLength(64)],
      barcode: [BLANK_VALUE.barcode, Validators.maxLength(64)],
      description: [BLANK_VALUE.description, Validators.maxLength(1000)],
      brand: [BLANK_VALUE.brand, Validators.maxLength(100)],
      model: [BLANK_VALUE.model, Validators.maxLength(100)],
      purchasePrice: [BLANK_VALUE.purchasePrice, [Validators.required, Validators.min(0)]],
      salePrice: [BLANK_VALUE.salePrice, [Validators.required, Validators.min(0)]],
      wholesalePrice: [BLANK_VALUE.wholesalePrice, Validators.min(0)],
      minimumSalePrice: [BLANK_VALUE.minimumSalePrice, Validators.min(0)],
      isTaxable: [BLANK_VALUE.isTaxable],
      taxPercentage: [{ value: BLANK_VALUE.taxPercentage, disabled: true }, [Validators.min(0), Validators.max(100)]],
      minimumStockLevel: [BLANK_VALUE.minimumStockLevel, [Validators.required, Validators.min(0)]],
      maximumStockLevel: [BLANK_VALUE.maximumStockLevel, Validators.min(0)],
      reorderLevel: [BLANK_VALUE.reorderLevel, [Validators.required, Validators.min(0)]],
      trackBatch: [BLANK_VALUE.trackBatch],
      trackExpiry: [{ value: BLANK_VALUE.trackExpiry, disabled: true }],
      expiryAlertDays: [{ value: BLANK_VALUE.expiryAlertDays, disabled: true }, Validators.min(0)],
      blockExpiredSale: [{ value: BLANK_VALUE.blockExpiredSale, disabled: true }],
      trackSerialNumber: [BLANK_VALUE.trackSerialNumber],
      isActive: [BLANK_VALUE.isActive],
    },
    { validators: [minimumSalePriceValidator(), maximumStockValidator()] },
  );

  ngOnInit(): void {
    this.editId = this.route.snapshot.paramMap.get('id') || undefined;

    this.form.controls.isTaxable.valueChanges.subscribe(isTaxable => {
      if (isTaxable) {
        this.form.controls.taxPercentage.enable({ emitEvent: false });
      } else {
        this.form.controls.taxPercentage.setValue(0, { emitEvent: false });
        this.form.controls.taxPercentage.disable({ emitEvent: false });
      }
    });

    this.form.controls.trackBatch.valueChanges.subscribe(trackBatch => {
      if (trackBatch) {
        this.form.controls.trackExpiry.enable({ emitEvent: false });
      } else {
        this.form.controls.trackExpiry.setValue(false, { emitEvent: false });
        this.form.controls.trackExpiry.disable({ emitEvent: false });
      }
    });

    this.form.controls.trackExpiry.valueChanges.subscribe(trackExpiry => {
      if (trackExpiry) {
        this.form.controls.expiryAlertDays.enable({ emitEvent: false });
        this.form.controls.blockExpiredSale.enable({ emitEvent: false });
        if (this.form.controls.expiryAlertDays.value == null) this.form.controls.expiryAlertDays.setValue(30, { emitEvent: false });
      } else {
        this.form.controls.expiryAlertDays.setValue(null, { emitEvent: false });
        this.form.controls.expiryAlertDays.disable({ emitEvent: false });
        this.form.controls.blockExpiredSale.disable({ emitEvent: false });
      }
    });

    this.categoryService.getLookup().subscribe(result => (this.categories = result.items || []));
    this.unitService.getLookup().subscribe(result => (this.units = result.items || []));

    if (this.editId) this.loadForEdit(this.editId);
  }

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting) return;

    this.submitting = true;
    const raw = this.form.getRawValue();
    const input: CreateShopProductDto | UpdateShopProductDto = {
      categoryId: raw.categoryId!,
      unitId: raw.unitId!,
      name: raw.name,
      code: raw.code,
      sku: raw.sku || undefined,
      barcode: raw.barcode || undefined,
      description: raw.description || undefined,
      brand: raw.brand || undefined,
      model: raw.model || undefined,
      purchasePrice: raw.purchasePrice,
      salePrice: raw.salePrice,
      wholesalePrice: raw.wholesalePrice ?? undefined,
      minimumSalePrice: raw.minimumSalePrice ?? undefined,
      isTaxable: raw.isTaxable,
      taxPercentage: raw.isTaxable ? raw.taxPercentage : 0,
      minimumStockLevel: raw.minimumStockLevel,
      maximumStockLevel: raw.maximumStockLevel ?? undefined,
      reorderLevel: raw.reorderLevel,
      trackBatch: raw.trackBatch,
      trackExpiry: raw.trackExpiry,
      expiryAlertDays: raw.trackExpiry ? raw.expiryAlertDays ?? 30 : undefined,
      blockExpiredSale: raw.blockExpiredSale,
      trackSerialNumber: raw.trackSerialNumber,
      isActive: raw.isActive,
    };

    const request = this.editId ? this.service.update(this.editId, input) : this.service.create(input);
    request.pipe(finalize(() => (this.submitting = false))).subscribe({
      next: saved => {
        this.toaster.success(this.isEdit ? '::ProductUpdatedSuccessfully' : '::ProductCreatedSuccessfully');
        const existingIndex = this.savedItems.findIndex(x => x.id === saved.id);
        if (existingIndex >= 0) this.savedItems[existingIndex] = saved;
        else this.savedItems = [saved, ...this.savedItems];
        this.editId = undefined;
        this.resetForm();
      },
      error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
    });
  }

  editSaved(dto: ShopProductDto): void {
    this.editId = dto.id;
    this.patchForm(dto);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  cancelEdit(): void {
    this.editId = undefined;
    this.resetForm();
  }

  done(): void {
    this.router.navigate(['/shop-management/products']);
  }

  private resetForm(): void {
    this.form.reset(BLANK_VALUE);
    this.currentStock = 0;
    this.form.controls.taxPercentage.disable({ emitEvent: false });
    this.form.controls.trackExpiry.disable({ emitEvent: false });
    this.form.controls.expiryAlertDays.disable({ emitEvent: false });
    this.form.controls.blockExpiredSale.disable({ emitEvent: false });
  }

  private patchForm(dto: ShopProductDto): void {
    this.currentStock = dto.currentStock;
    this.form.patchValue({
      categoryId: dto.categoryId,
      unitId: dto.unitId,
      name: dto.name,
      code: dto.code,
      sku: dto.sku || '',
      barcode: dto.barcode || '',
      description: dto.description || '',
      brand: dto.brand || '',
      model: dto.model || '',
      purchasePrice: dto.purchasePrice ?? 0,
      salePrice: dto.salePrice,
      wholesalePrice: dto.wholesalePrice ?? null,
      minimumSalePrice: dto.minimumSalePrice ?? null,
      isTaxable: dto.isTaxable,
      taxPercentage: dto.taxPercentage,
      minimumStockLevel: dto.minimumStockLevel,
      maximumStockLevel: dto.maximumStockLevel ?? null,
      reorderLevel: dto.reorderLevel,
      trackBatch: dto.trackBatch,
      trackExpiry: dto.trackExpiry,
      expiryAlertDays: dto.expiryAlertDays ?? null,
      blockExpiredSale: dto.blockExpiredSale,
      trackSerialNumber: dto.trackSerialNumber,
      isActive: dto.isActive,
    });
    if (dto.isTaxable) this.form.controls.taxPercentage.enable({ emitEvent: false });
    else this.form.controls.taxPercentage.disable({ emitEvent: false });

    if (dto.trackBatch) this.form.controls.trackExpiry.enable({ emitEvent: false });
    else this.form.controls.trackExpiry.disable({ emitEvent: false });

    if (dto.trackExpiry) {
      this.form.controls.expiryAlertDays.enable({ emitEvent: false });
      this.form.controls.blockExpiredSale.enable({ emitEvent: false });
    } else {
      this.form.controls.expiryAlertDays.disable({ emitEvent: false });
      this.form.controls.blockExpiredSale.disable({ emitEvent: false });
    }
  }

  private loadForEdit(id: string): void {
    this.loading = true;
    this.service
      .get(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => this.patchForm(dto));
  }
}
