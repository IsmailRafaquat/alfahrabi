import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { ShopProductDto, ShopProductService } from '../../proxy/shop-management/products';
import { ShopProductCategoryLookupDto, ShopProductCategoryService } from '../../proxy/shop-management/product-categories';
import { ShopUnitLookupDto, ShopUnitService } from '../../proxy/shop-management/units';

@Component({ selector: 'app-shop-products', standalone: false, templateUrl: './shop-products.component.html', styleUrl: './shop-products.component.scss' })
export class ShopProductsComponent implements OnInit {
  readonly Math = Math;
  private readonly service = inject(ShopProductService);
  private readonly categoryService = inject(ShopProductCategoryService);
  private readonly unitService = inject(ShopUnitService);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);

  readonly canCreate = this.permissions.getGrantedPolicy('ShopManagement.Products.Create');
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.Products.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.Products.Delete');
  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.Products.ViewCost');

  items: ShopProductDto[] = [];
  categories: ShopProductCategoryLookupDto[] = [];
  units: ShopUnitLookupDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;

  search = '';
  categoryFilter = '';
  unitFilter = '';
  statusFilter = '';
  taxFilter = '';
  lowStockOnly = false;

  ngOnInit(): void {
    this.categoryService.getLookup().subscribe(result => (this.categories = result.items || []));
    this.unitService.getLookup().subscribe(result => (this.units = result.items || []));
    this.load();
  }

  load(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;
    this.service
      .getList({
        filter: this.search || undefined,
        categoryId: this.categoryFilter || undefined,
        unitId: this.unitFilter || undefined,
        isActive: this.statusFilter === '' ? undefined : this.statusFilter === 'active',
        isTaxable: this.taxFilter === '' ? undefined : this.taxFilter === 'taxable',
        lowStockOnly: this.lowStockOnly,
        sorting: 'name asc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });
  }

  stockStatus(row: ShopProductDto): 'out' | 'low' | 'in' {
    if (row.currentStock <= 0) return 'out';
    if (row.currentStock <= row.minimumStockLevel) return 'low';
    return 'in';
  }

  create(): void {
    this.router.navigate(['/shop-management/products/create']);
  }

  edit(row: ShopProductDto): void {
    this.router.navigate(['/shop-management/products/edit', row.id]);
  }

  remove(row: ShopProductDto): void {
    this.confirmation.warn('::ConfirmDeleteProduct', row.name).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(row.id).subscribe({
        next: () => {
          this.toaster.success('::ProductDeletedSuccessfully');
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
