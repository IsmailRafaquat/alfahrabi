import { Component, OnInit, inject } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  CreateShopGoodsReceiptDto,
  CreateShopGoodsReceiptItemDto,
  ShopGoodsReceiptService,
  ShopGoodsReceiptStatus,
  UpdateShopGoodsReceiptDto,
} from '../../proxy/shop-management/goods-receipts';

function rowValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.get('included')?.value) return null;

    const errors: ValidationErrors = {};
    const receivedQuantity = control.get('receivedQuantity')?.value;
    const remainingQuantity = control.get('remainingQuantity')?.value;
    const allowDecimal = control.get('unitAllowDecimal')?.value;
    const trackBatch = control.get('trackBatch')?.value;
    const trackExpiry = control.get('trackExpiry')?.value;
    const trackSerialNumber = control.get('trackSerialNumber')?.value;
    const batchNumber = control.get('batchNumber')?.value;
    const expiryDate = control.get('expiryDate')?.value;
    const manufacturingDate = control.get('manufacturingDate')?.value;
    const bonusQuantity = control.get('bonusQuantity')?.value;

    if (trackSerialNumber) errors['serialNotSupported'] = true;
    if (receivedQuantity == null || receivedQuantity <= 0) errors['quantityRequired'] = true;
    else if (!allowDecimal && receivedQuantity !== Math.trunc(receivedQuantity)) errors['wholeQuantityRequired'] = true;
    else if (remainingQuantity != null && receivedQuantity > remainingQuantity) errors['exceedsRemaining'] = true;

    if (bonusQuantity != null && bonusQuantity < 0) errors['invalidBonus'] = true;
    if (trackBatch && !batchNumber) errors['batchRequired'] = true;
    if (trackExpiry && !expiryDate) errors['expiryRequired'] = true;
    if (manufacturingDate && expiryDate && new Date(manufacturingDate) >= new Date(expiryDate)) errors['invalidManufacturingDate'] = true;

    return Object.keys(errors).length ? errors : null;
  };
}

function atLeastOneIncludedValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const items = (control.get('items') as FormArray)?.controls ?? [];
    return items.some(x => x.get('included')?.value) ? null : { noItemsIncluded: true };
  };
}

@Component({ selector: 'app-shop-goods-receipt-editor', standalone: false, templateUrl: './shop-goods-receipt-editor.component.html', styleUrl: './shop-goods-receipt-editor.component.scss' })
export class ShopGoodsReceiptEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopGoodsReceiptService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.GoodsReceipts.ViewCost');

  editId?: string;
  purchaseOrderId = '';
  purchaseOrderNumber = '';
  supplierCode = '';
  supplierName = '';
  orderDate?: string;
  expectedDeliveryDate?: string;
  loading = false;
  submitting = false;
  helpOpen = false;

  get isEdit(): boolean {
    return !!this.editId;
  }

  readonly form = this.fb.group(
    {
      receiptDate: [this.todayIso(), Validators.required],
      supplierInvoiceNumber: ['', Validators.maxLength(128)],
      shippingCharges: [0, Validators.min(0)],
      otherCharges: [0, Validators.min(0)],
      notes: ['', Validators.maxLength(1000)],
      items: this.fb.array<FormGroup>([]),
    },
    { validators: atLeastOneIncludedValidator() },
  );

  get items(): FormArray<FormGroup> {
    return this.form.controls.items as FormArray<FormGroup>;
  }

  ngOnInit(): void {
    this.editId = this.route.snapshot.paramMap.get('id') || undefined;
    const poIdParam = this.route.snapshot.paramMap.get('purchaseOrderId');

    if (this.editId) {
      this.loadForEdit(this.editId);
    } else if (poIdParam) {
      this.purchaseOrderId = poIdParam;
      this.loadPurchaseOrderForReceiving(poIdParam);
    } else {
      this.toaster.error('::GoodsReceiptPurchaseOrderNotFound');
      this.router.navigate(['/shop-management/purchase-orders']);
    }
  }

  includedCount(): number {
    return this.items.controls.filter(x => x.value.included).length;
  }

  lineTotal(index: number): number {
    const row = this.items.at(index).value;
    if (!row.included) return 0;
    const subTotal = round2((row.receivedQuantity || 0) * (row.purchasePrice || 0));
    const discount = round2(subTotal * ((row.discountPercentage || 0) / 100));
    const taxable = subTotal - discount;
    const tax = round2(taxable * ((row.taxPercentage || 0) / 100));
    return round2(taxable + tax);
  }

  get subTotal(): number {
    return round2(this.items.controls.reduce((sum, row, i) => (row.value.included ? sum + (row.value.receivedQuantity || 0) * (row.value.purchasePrice || 0) : sum), 0));
  }

  get grandTotal(): number {
    const raw = this.form.getRawValue();
    const totalDiscount = round2(this.items.controls.reduce((sum, row) => {
      if (!row.value.included) return sum;
      const lineSubTotal = (row.value.receivedQuantity || 0) * (row.value.purchasePrice || 0);
      return sum + lineSubTotal * ((row.value.discountPercentage || 0) / 100);
    }, 0));
    const totalTax = round2(this.items.controls.reduce((sum, row, i) => (row.value.included ? sum + this.lineTaxAmount(i) : sum), 0));
    return round2(this.subTotal - totalDiscount + totalTax + (raw.shippingCharges || 0) + (raw.otherCharges || 0));
  }

  private lineTaxAmount(index: number): number {
    const row = this.items.at(index).value;
    const subTotal = (row.receivedQuantity || 0) * (row.purchasePrice || 0);
    const discount = subTotal * ((row.discountPercentage || 0) / 100);
    return round2((subTotal - discount) * ((row.taxPercentage || 0) / 100));
  }

  save(): void {
    this.form.markAllAsTouched();
    this.items.controls.forEach(row => row.markAllAsTouched());
    if (this.form.invalid || this.submitting) return;

    this.submitting = true;
    const raw = this.form.getRawValue();
    const items: CreateShopGoodsReceiptItemDto[] = raw.items
      .filter(x => x.included)
      .map(x => ({
        purchaseOrderItemId: x.purchaseOrderItemId!,
        receivedQuantity: x.receivedQuantity,
        bonusQuantity: x.bonusQuantity,
        purchasePrice: x.purchasePrice,
        salePrice: x.salePrice,
        batchNumber: x.batchNumber || undefined,
        manufacturingDate: x.manufacturingDate || undefined,
        expiryDate: x.expiryDate || undefined,
        discountPercentage: x.discountPercentage,
        taxPercentage: x.taxPercentage,
      }));

    const input: CreateShopGoodsReceiptDto | UpdateShopGoodsReceiptDto = {
      purchaseOrderId: this.purchaseOrderId,
      receiptDate: raw.receiptDate!,
      supplierInvoiceNumber: raw.supplierInvoiceNumber || undefined,
      shippingCharges: raw.shippingCharges,
      otherCharges: raw.otherCharges,
      notes: raw.notes || undefined,
      items,
    };

    const request = this.editId ? this.service.update(this.editId, input) : this.service.create(input);
    request.pipe(finalize(() => (this.submitting = false))).subscribe({
      next: saved => {
        this.toaster.success(this.isEdit ? '::GoodsReceiptUpdatedSuccessfully' : '::GoodsReceiptCreatedSuccessfully');
        this.router.navigate(['/shop-management/goods-receipts', saved.id]);
      },
      error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
    });
  }

  cancel(): void {
    this.router.navigate(['/shop-management/goods-receipts']);
  }

  private loadPurchaseOrderForReceiving(purchaseOrderId: string): void {
    this.loading = true;
    this.service
      .getPurchaseOrderForReceiving(purchaseOrderId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: po => {
          this.purchaseOrderNumber = po.purchaseOrderNumber;
          this.supplierCode = po.supplierCode;
          this.supplierName = po.supplierName;
          this.orderDate = po.orderDate;
          this.expectedDeliveryDate = po.expectedDeliveryDate;

          po.items.forEach(item => {
            this.items.push(
              this.createItemRow({
                purchaseOrderItemId: item.purchaseOrderItemId,
                productName: item.productName,
                productCode: item.productCode,
                unitName: item.unitName,
                unitShortName: item.unitShortName,
                unitAllowDecimal: item.unitAllowDecimal,
                trackBatch: item.trackBatch,
                trackExpiry: item.trackExpiry,
                trackSerialNumber: item.trackSerialNumber,
                orderedQuantity: item.orderedQuantity,
                previouslyReceivedQuantity: item.previouslyReceivedQuantity,
                remainingQuantity: item.remainingQuantity,
                included: false,
                receivedQuantity: 0,
                bonusQuantity: 0,
                purchasePrice: item.defaultPurchasePrice ?? 0,
                salePrice: item.defaultSalePrice ?? 0,
              }),
            );
          });
        },
        error: e => {
          this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError');
          this.router.navigate(['/shop-management/purchase-orders']);
        },
      });
  }

  private loadForEdit(id: string): void {
    this.loading = true;
    this.service
      .get(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => {
        if (dto.status !== ShopGoodsReceiptStatus.Draft) {
          this.toaster.error('::GoodsReceiptCannotBeEdited');
          this.router.navigate(['/shop-management/goods-receipts', id]);
          return;
        }

        this.purchaseOrderId = dto.purchaseOrderId;
        this.purchaseOrderNumber = dto.purchaseOrderNumber;
        this.supplierCode = dto.supplierCode;
        this.supplierName = dto.supplierName;

        this.form.patchValue({
          receiptDate: dto.receiptDate.substring(0, 10),
          supplierInvoiceNumber: dto.supplierInvoiceNumber || '',
          shippingCharges: dto.shippingCharges ?? 0,
          otherCharges: dto.otherCharges ?? 0,
          notes: dto.notes || '',
        });

        dto.items.forEach(item => {
          this.items.push(
            this.createItemRow({
              purchaseOrderItemId: item.purchaseOrderItemId,
              productName: item.productName,
              productCode: item.productCode,
              unitName: item.unitName,
              unitShortName: item.unitShortName,
              unitAllowDecimal: item.unitAllowDecimal,
              trackBatch: item.trackBatch,
              trackExpiry: item.trackExpiry,
              trackSerialNumber: item.trackSerialNumber,
              orderedQuantity: item.orderedQuantity,
              previouslyReceivedQuantity: item.previouslyReceivedQuantity,
              remainingQuantity: item.remainingQuantity,
              included: true,
              receivedQuantity: item.receivedQuantity,
              bonusQuantity: item.bonusQuantity,
              purchasePrice: item.purchasePrice ?? 0,
              salePrice: item.salePrice,
              batchNumber: item.batchNumber || '',
              manufacturingDate: item.manufacturingDate ? item.manufacturingDate.substring(0, 10) : null,
              expiryDate: item.expiryDate ? item.expiryDate.substring(0, 10) : null,
              discountPercentage: item.discountPercentage,
              taxPercentage: item.taxPercentage,
            }),
          );
        });
      });
  }

  private createItemRow(value: Record<string, any>): FormGroup {
    return this.fb.group(
      {
        purchaseOrderItemId: [value.purchaseOrderItemId],
        productName: [value.productName],
        productCode: [value.productCode],
        unitName: [value.unitName],
        unitShortName: [value.unitShortName],
        unitAllowDecimal: [value.unitAllowDecimal],
        trackBatch: [value.trackBatch],
        trackExpiry: [value.trackExpiry],
        trackSerialNumber: [value.trackSerialNumber],
        orderedQuantity: [value.orderedQuantity],
        previouslyReceivedQuantity: [value.previouslyReceivedQuantity],
        remainingQuantity: [value.remainingQuantity],
        included: [value.included ?? false],
        receivedQuantity: [value.receivedQuantity ?? 0],
        bonusQuantity: [value.bonusQuantity ?? 0],
        purchasePrice: [value.purchasePrice ?? 0],
        salePrice: [value.salePrice ?? 0],
        batchNumber: [value.batchNumber ?? '', Validators.maxLength(128)],
        manufacturingDate: [value.manufacturingDate ?? null],
        expiryDate: [value.expiryDate ?? null],
        discountPercentage: [value.discountPercentage ?? 0, [Validators.min(0), Validators.max(100)]],
        taxPercentage: [value.taxPercentage ?? 0, [Validators.min(0), Validators.max(100)]],
      },
      { validators: rowValidator() },
    );
  }

  private todayIso(): string {
    return new Date().toISOString().substring(0, 10);
  }
}

function round2(value: number): number {
  return Math.round((value + Number.EPSILON) * 100) / 100;
}
