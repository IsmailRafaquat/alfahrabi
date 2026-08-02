import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { ShopPurchaseOrderDto, ShopPurchaseOrderService, ShopPurchaseOrderStatus, shopPurchaseOrderStatusOptions } from '../../proxy/shop-management/purchase-orders';
import { ShopSupplierLookupDto, ShopSupplierService } from '../../proxy/shop-management/suppliers';

@Component({ selector: 'app-shop-purchase-orders', standalone: false, templateUrl: './shop-purchase-orders.component.html', styleUrl: './shop-purchase-orders.component.scss' })
export class ShopPurchaseOrdersComponent implements OnInit {
  readonly Math = Math;
  readonly ShopPurchaseOrderStatus = ShopPurchaseOrderStatus;
  readonly statusOptions = shopPurchaseOrderStatusOptions;

  private readonly service = inject(ShopPurchaseOrderService);
  private readonly supplierService = inject(ShopSupplierService);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);

  readonly canCreate = this.permissions.getGrantedPolicy('ShopManagement.PurchaseOrders.Create');
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.PurchaseOrders.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.PurchaseOrders.Delete');
  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.PurchaseOrders.ViewCost');

  items: ShopPurchaseOrderDto[] = [];
  suppliers: ShopSupplierLookupDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;

  filters: { filter?: string } = {};
  supplierFilter = '';
  statusFilter: ShopPurchaseOrderStatus | '' = '';
  orderDateFrom: string | null = null;
  orderDateTo: string | null = null;

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
        filter: this.filters.filter || undefined,
        supplierId: this.supplierFilter || undefined,
        status: this.statusFilter === '' ? undefined : this.statusFilter,
        orderDateFrom: this.orderDateFrom || undefined,
        orderDateTo: this.orderDateTo || undefined,
        sorting: 'orderDate desc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });
  }

  isDraft(row: ShopPurchaseOrderDto): boolean {
    return row.status === ShopPurchaseOrderStatus.Draft;
  }

  statusLabel(status: ShopPurchaseOrderStatus): string {
    return '::' + ShopPurchaseOrderStatus[status];
  }

  statusClass(status: ShopPurchaseOrderStatus): string {
    switch (status) {
      case ShopPurchaseOrderStatus.Draft: return 'draft';
      case ShopPurchaseOrderStatus.PendingApproval: return 'pending';
      case ShopPurchaseOrderStatus.Approved: return 'approved';
      case ShopPurchaseOrderStatus.Rejected: return 'rejected';
      case ShopPurchaseOrderStatus.PartiallyReceived: return 'partial';
      case ShopPurchaseOrderStatus.FullyReceived: return 'received';
      case ShopPurchaseOrderStatus.Cancelled: return 'cancelled';
      case ShopPurchaseOrderStatus.Closed: return 'closed';
      default: return 'draft';
    }
  }

  view(row: ShopPurchaseOrderDto): void {
    this.router.navigate(['/shop-management/purchase-orders', row.id]);
  }

  create(): void {
    this.router.navigate(['/shop-management/purchase-orders/create']);
  }

  edit(row: ShopPurchaseOrderDto): void {
    this.router.navigate(['/shop-management/purchase-orders', row.id, 'edit']);
  }

  remove(row: ShopPurchaseOrderDto): void {
    this.confirmation.warn('::ConfirmDeletePurchaseOrder', row.purchaseOrderNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(row.id).subscribe({
        next: () => {
          this.toaster.success('::PurchaseOrderDeletedSuccessfully');
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
