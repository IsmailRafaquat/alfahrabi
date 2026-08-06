import { Component, OnInit, inject } from '@angular/core';
import { FormArray, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { PurchaseReturnService } from './purchase-return.service';

@Component({ selector: 'app-purchase-return-editor', standalone: false, templateUrl: './purchase-return-editor.component.html', styleUrls: ['./purchase-return-editor.component.scss'] })
export class PurchaseReturnEditorComponent implements OnInit {
  private fb = inject(FormBuilder); private s = inject(PurchaseReturnService); private route = inject(ActivatedRoute); private router = inject(Router); private toast = inject(ToasterService); private permissions = inject(PermissionService);
  id?: string; receiptId?: string; receipt: any; loading = true; submitting = false; readonlyMode = false; helpOpen = false; helpLang: 'en' | 'ur' = 'en';
  canCost = this.permissions.getGrantedPolicy('ShopManagement.PurchaseReturns.ViewCost');
  form = this.fb.group({ returnDate: [new Date().toISOString().slice(0, 10), Validators.required], reason: [0, Validators.required], reasonDetails: [''], otherCharges: [0, [Validators.required, Validators.min(0)]], notes: [''], items: this.fb.array<any>([]) });
  get items() { return this.form.controls.items as FormArray; }

  ngOnInit() {
    this.id = this.route.snapshot.paramMap.get('id') || undefined;
    this.receiptId = this.route.snapshot.paramMap.get('goodsReceiptId') || undefined;
    this.readonlyMode = !!this.id && !this.router.url.endsWith('/edit');
    if (this.id) {
      this.s.get(this.id).subscribe(x => {
        this.receipt = { id: x.goodsReceiptId, goodsReceiptNumber: x.goodsReceiptNumber, supplierName: x.supplierName };
        this.form.patchValue({ ...x, returnDate: x.returnDate ? x.returnDate.slice(0, 10) : x.returnDate } as any);
        x.items.forEach(i => this.items.push(this.row({ ...i, productName: i.productNameSnapshot, productCode: i.productCodeSnapshot, unitName: i.unitNameSnapshot, unitShortName: i.unitShortNameSnapshot, receivedQuantity: i.receivedQuantitySnapshot, returnableQuantity: (i.receivedQuantitySnapshot || 0) - (i.previouslyReturnedQuantity || 0), purchasePrice: i.unitPurchasePrice })));
        if (this.readonlyMode) this.form.disable({ emitEvent: false });
        this.loading = false;
      });
    } else if (this.receiptId) {
      this.s.getReceipt(this.receiptId).subscribe(x => { this.receipt = x; x.items.forEach((i: any) => this.items.push(this.row(i))); this.loading = false; });
    } else this.done();
  }

  row(x: any) {
    return this.fb.group({
      // Existing return DTOs have both IDs. The update API requires the original receipt-line ID.
      goodsReceiptItemId: [x.goodsReceiptItemId || x.id, Validators.required],
      productName: [x.productName], productCode: [x.productCode], unitName: [x.unitName], unitShortName: [x.unitShortName], receivedQuantity: [x.receivedQuantity], bonusQuantity: [x.bonusQuantity], previouslyReturnedQuantity: [x.previouslyReturnedQuantity], returnableQuantity: [x.returnableQuantity], returnQuantity: [x.returnQuantity || 0, [Validators.min(0)]], purchasePrice: [x.purchasePrice], taxPercentage: [x.taxPercentage], batchNumber: [x.batchNumber], expiryDate: [x.expiryDate], reason: [x.reason ?? 0], notes: [x.notes || ''],
    });
  }

  lineTotal(x: any) { return (x.value.returnQuantity || 0) * (x.value.purchasePrice || 0) * (1 + (x.value.taxPercentage || 0) / 100); }
  save() {
    this.form.markAllAsTouched(); if (this.form.invalid || this.submitting) return;
    const raw = this.form.getRawValue();
    const selected = (raw.items || []).filter((x: any) => x.returnQuantity > 0).map((x: any) => ({ goodsReceiptItemId: x.goodsReceiptItemId, returnQuantity: x.returnQuantity, reason: x.reason, notes: x.notes }));
    if (!selected.length) { this.toast.warn('Enter a return quantity for at least one product.'); return; }
    const body = { ...raw, goodsReceiptId: this.receipt.id, items: selected }; this.submitting = true;
    (this.id ? this.s.update(this.id, body) : this.s.create(body)).subscribe({ next: () => { this.toast.success(this.id ? 'Purchase return updated successfully.' : 'Purchase return created successfully.'); this.done(); }, error: () => this.submitting = false });
  }
  done() { this.router.navigate(['/shop-management/purchase-returns']); }
  reasons = ['Damaged', 'Expired', 'Wrong Product', 'Quality Issue', 'Excess Quantity', 'Other'];
}
