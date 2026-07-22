import { Component, OnInit, inject } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  CreateShopPurchaseOrderDto,
  CreateShopPurchaseOrderItemDto,
  ShopPurchaseOrderService,
  ShopPurchaseOrderStatus,
  UpdateShopPurchaseOrderDto,
} from '../../proxy/shop-management/purchase-orders';
import { ShopSupplierLookupDto, ShopSupplierService } from '../../proxy/shop-management/suppliers';
import { ShopProductLookupDto, ShopProductService } from '../../proxy/shop-management/products';

function expectedDeliveryDateValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const orderDate = control.get('orderDate')?.value;
    const expectedDeliveryDate = control.get('expectedDeliveryDate')?.value;
    if (!orderDate || !expectedDeliveryDate) return null;
    return new Date(expectedDeliveryDate) < new Date(orderDate) ? { expectedDeliveryDateBeforeOrderDate: true } : null;
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
    const quantity = control.get('orderedQuantity')?.value;
    if (allowDecimal || quantity == null) return null;
    return quantity !== Math.trunc(quantity) ? { wholeQuantityRequired: true } : null;
  };
}

@Component({ selector: 'app-shop-purchase-order-editor', standalone: false, templateUrl: './shop-purchase-order-editor.component.html', styleUrl: './shop-purchase-order-editor.component.scss' })
export class ShopPurchaseOrderEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopPurchaseOrderService);
  private readonly supplierService = inject(ShopSupplierService);
  private readonly productService = inject(ShopProductService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.PurchaseOrders.ViewCost');

  suppliers: ShopSupplierLookupDto[] = [];
  products: ShopProductLookupDto[] = [];
  editId?: string;
  loading = false;
  submitting = false;
  helpOpen = false;
  purchaseOrderNumber = '';

  get isEdit(): boolean {
    return !!this.editId;
  }

  readonly form = this.fb.group(
    {
      supplierId: [null as string | null, Validators.required],
      orderDate: [this.todayIso(), Validators.required],
      expectedDeliveryDate: [null as string | null],
      supplierReference: ['', Validators.maxLength(128)],
      shippingCharges: [0, Validators.min(0)],
      otherCharges: [0, Validators.min(0)],
      notes: ['', Validators.maxLength(1000)],
      items: this.fb.array<FormGroup>([]),
    },
    { validators: [expectedDeliveryDateValidator(), duplicateProductValidator()] },
  );

  get items(): FormArray<FormGroup> {
    return this.form.controls.items as FormArray<FormGroup>;
  }

  ngOnInit(): void {
    this.editId = this.route.snapshot.paramMap.get('id') || undefined;

    this.supplierService.getLookup().subscribe(result => (this.suppliers = result.items || []));
    this.productService.getLookup().subscribe(result => (this.products = result.items || []));

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

    row.patchValue({ unitName: product.unitName, unitShortName: product.unitShortName, allowDecimal: product.unitAllowDecimal });

    if (this.canViewCost) {
      this.productService.get(productId).subscribe(full => {
        row.patchValue({ unitPurchasePrice: full.purchasePrice ?? 0 });
      });
    }
  }

  productOptions(index: number): ShopProductLookupDto[] {
    const currentValue = this.items.at(index).value.productId;
    const selectedElsewhere = this.items.controls.filter((_, i) => i !== index).map(x => x.value.productId);
    return this.products.filter(x => x.id === currentValue || !selectedElsewhere.includes(x.id));
  }

  lineSubTotal(index: number): number {
    const row = this.items.at(index).value;
    return round2((row.orderedQuantity || 0) * (row.unitPurchasePrice || 0));
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
    return round2(this.subTotal - this.totalDiscount + this.totalTax + (raw.shippingCharges || 0) + (raw.otherCharges || 0));
  }

  save(): void {
    this.form.markAllAsTouched();
    this.items.controls.forEach(row => row.markAllAsTouched());
    if (this.form.invalid || this.submitting) return;

    this.submitting = true;
    const raw = this.form.getRawValue();
    const items: CreateShopPurchaseOrderItemDto[] = raw.items.map(x => ({
      productId: x.productId!,
      description: x.description || undefined,
      orderedQuantity: x.orderedQuantity,
      unitPurchasePrice: x.unitPurchasePrice,
      discountPercentage: x.discountPercentage,
      taxPercentage: x.taxPercentage,
    }));

    const input: CreateShopPurchaseOrderDto | UpdateShopPurchaseOrderDto = {
      supplierId: raw.supplierId!,
      orderDate: raw.orderDate!,
      expectedDeliveryDate: raw.expectedDeliveryDate || undefined,
      supplierReference: raw.supplierReference || undefined,
      shippingCharges: raw.shippingCharges,
      otherCharges: raw.otherCharges,
      notes: raw.notes || undefined,
      items,
    };

    const request = this.editId ? this.service.update(this.editId, input) : this.service.create(input);
    request.pipe(finalize(() => (this.submitting = false))).subscribe({
      next: saved => {
        this.toaster.success(this.isEdit ? '::PurchaseOrderUpdatedSuccessfully' : '::PurchaseOrderCreatedSuccessfully');
        this.router.navigate(['/shop-management/purchase-orders', saved.id]);
      },
      error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
    });
  }

  cancel(): void {
    this.router.navigate(['/shop-management/purchase-orders']);
  }

  private loadForEdit(id: string): void {
    this.loading = true;
    this.service
      .get(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => {
        if (dto.status !== ShopPurchaseOrderStatus.Draft) {
          this.toaster.error('::PurchaseOrderCannotBeEdited');
          this.router.navigate(['/shop-management/purchase-orders', id]);
          return;
        }

        this.purchaseOrderNumber = dto.purchaseOrderNumber;
        this.form.patchValue({
          supplierId: dto.supplierId,
          orderDate: dto.orderDate.substring(0, 10),
          expectedDeliveryDate: dto.expectedDeliveryDate ? dto.expectedDeliveryDate.substring(0, 10) : null,
          supplierReference: dto.supplierReference || '',
          shippingCharges: dto.shippingCharges ?? 0,
          otherCharges: dto.otherCharges ?? 0,
          notes: dto.notes || '',
        });

        this.items.clear();
        dto.items.forEach(item => {
          this.items.push(
            this.createItemRow({
              productId: item.productId,
              description: item.description || '',
              unitName: item.unitName,
              unitShortName: item.unitShortName,
              allowDecimal: true,
              orderedQuantity: item.orderedQuantity,
              unitPurchasePrice: item.unitPurchasePrice ?? 0,
              discountPercentage: item.discountPercentage,
              taxPercentage: item.taxPercentage,
            }),
          );
        });

        this.products.length &&
          dto.items.forEach((item, index) => {
            const product = this.products.find(x => x.id === item.productId);
            if (product) this.items.at(index).patchValue({ allowDecimal: product.unitAllowDecimal });
          });
      });
  }

  private createItemRow(value: Partial<Record<string, any>> = {}): FormGroup {
    return this.fb.group(
      {
        productId: [value.productId ?? null, Validators.required],
        description: [value.description ?? '', Validators.maxLength(500)],
        unitName: [value.unitName ?? ''],
        unitShortName: [value.unitShortName ?? ''],
        allowDecimal: [value.allowDecimal ?? true],
        orderedQuantity: [value.orderedQuantity ?? 1, [Validators.required, Validators.min(0.0001)]],
        unitPurchasePrice: [value.unitPurchasePrice ?? 0, [Validators.required, Validators.min(0)]],
        discountPercentage: [value.discountPercentage ?? 0, [Validators.min(0), Validators.max(100)]],
        taxPercentage: [value.taxPercentage ?? 0, [Validators.min(0), Validators.max(100)]],
      },
      { validators: wholeQuantityValidator() },
    );
  }

  private todayIso(): string {
    return new Date().toISOString().substring(0, 10);
  }
}

function round2(value: number): number {
  return Math.round((value + Number.EPSILON) * 100) / 100;
}
