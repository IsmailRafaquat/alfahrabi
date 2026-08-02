import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { PurchaseReturn, PurchaseReturnService } from './purchase-return.service';

@Component({
  selector: 'app-purchase-returns',
  standalone: false,
  templateUrl: './purchase-returns.component.html',
  styleUrls: ['./purchase-returns.component.scss'],
})
export class PurchaseReturnsComponent implements OnInit {
  private s = inject(PurchaseReturnService); private router = inject(Router); private permissions = inject(PermissionService); private confirm = inject(ConfirmationService); private toast = inject(ToasterService); private fb = inject(FormBuilder);
  items: PurchaseReturn[] = []; totalCount = 0; page = 0; pageSize = 10; loading = false; filters: { filter?: string } = {}; status: any = null; tooltipLang: 'en' | 'ur' = 'en'; Math = Math;
  canCreate = this.permissions.getGrantedPolicy('ShopManagement.PurchaseReturns.Create'); canEdit = this.permissions.getGrantedPolicy('ShopManagement.PurchaseReturns.Edit'); canDelete = this.permissions.getGrantedPolicy('ShopManagement.PurchaseReturns.Delete'); canComplete = this.permissions.getGrantedPolicy('ShopManagement.PurchaseReturns.Complete'); canCancel = this.permissions.getGrantedPolicy('ShopManagement.PurchaseReturns.Cancel');

  cancelModalOpen = false; cancelTarget?: PurchaseReturn; cancelSubmitting = false;
  cancelForm = this.fb.group({ cancellationReason: ['', [Validators.required, Validators.maxLength(500)]] });

  ngOnInit() { this.load(); }
  load(reset = false) { if (reset) this.page = 0; this.loading = true; this.s.getList({ filter: this.filters.filter || undefined, status: this.status, skipCount: this.page * this.pageSize, maxResultCount: this.pageSize }).subscribe({ next: r => { this.items = r.items || []; this.totalCount = r.totalCount; this.loading = false; }, error: () => this.loading = false }); }
  previousPage() { if (this.page > 0) { this.page--; this.load(); } }
  nextPage() { if ((this.page + 1) * this.pageSize < this.totalCount) { this.page++; this.load(); } }
  edit(x: PurchaseReturn) { this.router.navigate(['/shop-management/purchase-returns', x.id, 'edit']); }
  view(x: PurchaseReturn) { this.router.navigate(['/shop-management/purchase-returns', x.id]); }
  remove(x: PurchaseReturn) { this.confirm.warn('::ConfirmDeletePurchaseReturn', x.purchaseReturnNumber).subscribe(v => { if (v === Confirmation.Status.confirm) this.s.delete(x.id).subscribe(() => this.load()); }); }
  complete(x: PurchaseReturn) { this.confirm.warn('::ConfirmCompletePurchaseReturn', x.purchaseReturnNumber).subscribe(v => { if (v === Confirmation.Status.confirm) this.s.complete(x.id).subscribe(() => { this.toast.success('::PurchaseReturnCompletedSuccessfully'); this.load(); }); }); }
  cancel(x: PurchaseReturn) { this.cancelTarget = x; this.cancelForm.reset(); this.cancelModalOpen = true; }
  confirmCancel() {
    this.cancelForm.markAllAsTouched();
    if (this.cancelForm.invalid || !this.cancelTarget) return;
    this.cancelSubmitting = true;
    this.s.cancel(this.cancelTarget.id, this.cancelForm.getRawValue() as { cancellationReason: string }).subscribe({
      next: () => { this.cancelSubmitting = false; this.cancelModalOpen = false; this.toast.success('::PurchaseReturnCancelledSuccessfully'); this.load(); },
      error: e => { this.cancelSubmitting = false; this.toast.error(e?.error?.error?.message || e?.message || '::UnexpectedError'); },
    });
  }
  reasonName(v: number) { return ['Damaged', 'Expired', 'Wrong Product', 'Quality Issue', 'Excess Quantity', 'Other'][v] || ''; }
  statusName(v: number) { return ['Draft', 'Completed', 'Cancelled'][v] || ''; }
}
