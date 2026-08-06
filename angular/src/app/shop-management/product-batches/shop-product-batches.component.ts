import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  ShopProductBatchDto,
  ShopProductBatchService,
  ShopProductBatchStatus,
  shopProductBatchStatusOptions,
} from '../../proxy/shop-management/product-batches';
import { ShopProductService } from '../../proxy/shop-management/products';
import { ShopSupplierService } from '../../proxy/shop-management/suppliers';

@Component({ selector: 'app-shop-product-batches', standalone: false, templateUrl: './shop-product-batches.component.html', styleUrl: './shop-product-batches.component.scss' })
export class ShopProductBatchesComponent implements OnInit {
  readonly Math = Math;
  readonly ShopProductBatchStatus = ShopProductBatchStatus;
  readonly statusOptions = shopProductBatchStatusOptions;

  private readonly service = inject(ShopProductBatchService);
  private readonly productService = inject(ShopProductService);
  private readonly supplierService = inject(ShopSupplierService);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);

  readonly canBlock = this.permissions.getGrantedPolicy('ShopManagement.ProductBatches.Block');
  readonly canUnblock = this.permissions.getGrantedPolicy('ShopManagement.ProductBatches.Unblock');
  readonly canViewExpired = this.permissions.getGrantedPolicy('ShopManagement.ProductBatches.ViewExpired');
  readonly canViewTransactions = this.permissions.getGrantedPolicy('ShopManagement.ProductBatches.ViewTransactions');

  items: ShopProductBatchDto[] = [];
  products: { id: string; code: string; name: string }[] = [];
  suppliers: { id: string; name: string }[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;

  filters: { filter?: string } = {};
  productFilter = '';
  supplierFilter = '';
  statusFilter: ShopProductBatchStatus | '' = '';
  batchNumberFilter = '';
  expiryFrom: string | null = null;
  expiryTo: string | null = null;
  nearExpiryOnly = false;
  expiredOnly = false;
  hasAvailableStock = false;

  ngOnInit(): void {
    this.productService.getList({ maxResultCount: 1000, sorting: 'name' } as any).subscribe(result => {
      this.products = (result.items || []).map((x: any) => ({ id: x.id, code: x.code, name: x.name }));
    });
    this.supplierService.getList({ maxResultCount: 1000, sorting: 'name' } as any).subscribe(result => {
      this.suppliers = (result.items || []).map((x: any) => ({ id: x.id, name: x.name }));
    });
    this.load();
  }

  load(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;
    this.service
      .getList({
        filter: this.filters.filter || undefined,
        productId: this.productFilter || undefined,
        supplierId: this.supplierFilter || undefined,
        status: this.statusFilter === '' ? undefined : this.statusFilter,
        batchNumber: this.batchNumberFilter || undefined,
        expiryFrom: this.expiryFrom || undefined,
        expiryTo: this.expiryTo || undefined,
        nearExpiryOnly: this.nearExpiryOnly || undefined,
        expiredOnly: this.expiredOnly || undefined,
        hasAvailableStock: this.hasAvailableStock || undefined,
        sorting: 'expiryDate asc, creationTime desc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });
  }

  statusLabel(status?: ShopProductBatchStatus): string {
    return status == null ? '' : '::' + ShopProductBatchStatus[status];
  }

  statusClass(status?: ShopProductBatchStatus): string {
    switch (status) {
      case ShopProductBatchStatus.Active: return 'active';
      case ShopProductBatchStatus.NearExpiry: return 'near-expiry';
      case ShopProductBatchStatus.Expired: return 'expired';
      case ShopProductBatchStatus.Exhausted: return 'exhausted';
      case ShopProductBatchStatus.Blocked: return 'blocked';
      default: return 'active';
    }
  }

  view(row: ShopProductBatchDto): void {
    this.router.navigate(['/shop-management/product-batches', row.id]);
  }

  viewStockTransactions(row: ShopProductBatchDto): void {
    this.router.navigate(['/shop-management/product-batches', row.id], { queryParams: { tab: 'transactions' } });
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
}
