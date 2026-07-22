import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  ShopGoodsReceiptDto,
  ShopGoodsReceiptPaymentStatus,
  ShopGoodsReceiptService,
  ShopGoodsReceiptStatus,
  shopGoodsReceiptStatusOptions,
} from '../../proxy/shop-management/goods-receipts';
import { ShopSupplierLookupDto, ShopSupplierService } from '../../proxy/shop-management/suppliers';

@Component({ selector: 'app-shop-goods-receipts', standalone: false, templateUrl: './shop-goods-receipts.component.html', styleUrl: './shop-goods-receipts.component.scss' })
export class ShopGoodsReceiptsComponent implements OnInit {
  readonly Math = Math;
  readonly ShopGoodsReceiptStatus = ShopGoodsReceiptStatus;
  readonly statusOptions = shopGoodsReceiptStatusOptions;

  private readonly service = inject(ShopGoodsReceiptService);
  private readonly supplierService = inject(ShopSupplierService);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);

  readonly canCreate = this.permissions.getGrantedPolicy('ShopManagement.GoodsReceipts.Create');
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.GoodsReceipts.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.GoodsReceipts.Delete');
  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.GoodsReceipts.ViewCost');
  readonly canViewPaymentAmount = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.ViewAmount');

  items: ShopGoodsReceiptDto[] = [];
  suppliers: ShopSupplierLookupDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;

  search = '';
  supplierFilter = '';
  statusFilter: ShopGoodsReceiptStatus | '' = '';
  receiptDateFrom: string | null = null;
  receiptDateTo: string | null = null;

  tooltipLang: 'en' | 'ur' = 'en';

  ngOnInit(): void {
    this.supplierService.getLookup().subscribe(result => (this.suppliers = result.items || []));
    this.load();
  }

  load(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;
    this.service
      .getList({
        filter: this.search || undefined,
        supplierId: this.supplierFilter || undefined,
        status: this.statusFilter === '' ? undefined : this.statusFilter,
        receiptDateFrom: this.receiptDateFrom || undefined,
        receiptDateTo: this.receiptDateTo || undefined,
        sorting: 'receiptDate desc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });
  }

  isDraft(row: ShopGoodsReceiptDto): boolean {
    return row.status === ShopGoodsReceiptStatus.Draft;
  }

  statusLabel(status: ShopGoodsReceiptStatus): string {
    return '::' + ShopGoodsReceiptStatus[status];
  }

  statusClass(status: ShopGoodsReceiptStatus): string {
    switch (status) {
      case ShopGoodsReceiptStatus.Draft: return 'draft';
      case ShopGoodsReceiptStatus.Completed: return 'completed';
      case ShopGoodsReceiptStatus.Cancelled: return 'cancelled';
      default: return 'draft';
    }
  }

  paymentStatusLabel(status?: ShopGoodsReceiptPaymentStatus): string {
    return '::' + ShopGoodsReceiptPaymentStatus[status ?? ShopGoodsReceiptPaymentStatus.Unpaid];
  }

  paymentStatusClass(status?: ShopGoodsReceiptPaymentStatus): string {
    switch (status) {
      case ShopGoodsReceiptPaymentStatus.Unpaid: return 'unpaid';
      case ShopGoodsReceiptPaymentStatus.PartiallyPaid: return 'partially-paid';
      case ShopGoodsReceiptPaymentStatus.Paid: return 'paid';
      default: return 'unpaid';
    }
  }

  createFromPurchaseOrder(): void {
    this.toaster.info('::SelectPurchaseOrderToReceive');
    this.router.navigate(['/shop-management/purchase-orders']);
  }

  view(row: ShopGoodsReceiptDto): void {
    this.router.navigate(['/shop-management/goods-receipts', row.id]);
  }

  edit(row: ShopGoodsReceiptDto): void {
    this.router.navigate(['/shop-management/goods-receipts', row.id, 'edit']);
  }

  remove(row: ShopGoodsReceiptDto): void {
    this.confirmation.warn('::ConfirmDeleteGoodsReceipt', row.goodsReceiptNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(row.id).subscribe({
        next: () => {
          this.toaster.success('::GoodsReceiptDeletedSuccessfully');
          this.load();
        },
        error: e => this.showError(e),
      });
    });
  }

  previousPage(): void {
    if (this.page > 0) {
      this.page--;
      this.load();
    }
  }

  nextPage(): void {
    if ((this.page + 1) * this.pageSize < this.totalCount) {
      this.page++;
      this.load();
    }
  }

  private showError(error: any): void {
    this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError');
  }
}
