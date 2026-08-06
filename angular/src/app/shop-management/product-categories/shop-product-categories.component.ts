import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { Router } from '@angular/router';
import {
  ShopProductCategoryDto, ShopProductCategoryLookupDto,
  ShopProductCategoryService, ShopProductCategoryTreeDto,
} from '../../proxy/shop-management/product-categories';
import { ConfirmationHelperService } from '../../shared/services/confirmation-helper.service';

@Component({ selector: 'app-shop-product-categories', standalone: false, templateUrl: './shop-product-categories.component.html', styleUrl: './shop-product-categories.component.scss' })
export class ShopProductCategoriesComponent implements OnInit {
  readonly Math = Math;
  private readonly service = inject(ShopProductCategoryService);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationHelperService);
  private readonly toaster = inject(ToasterService);
  readonly canCreate = this.permissions.getGrantedPolicy('ShopManagement.ProductCategories.Create');
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.ProductCategories.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.ProductCategories.Delete');
  items: ShopProductCategoryDto[] = [];
  lookup: ShopProductCategoryLookupDto[] = [];
  tree: ShopProductCategoryTreeDto[] = [];
  totalCount = 0; page = 0; pageSize = 10; loading = false; submitting = false;
  filters: { filter?: string } = {}; parentFilter = ''; statusFilter = ''; activeView: 'list' | 'tree' = 'list';
  tooltipLang: 'en' | 'ur' = 'en';

  ngOnInit(): void { this.loadLookup(); this.loadList(); }
  loadList(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;
    this.service.getList({ filter: this.filters.filter || undefined, parentCategoryId: this.parentFilter && this.parentFilter !== 'root' ? this.parentFilter : undefined,
      rootCategoriesOnly: this.parentFilter === 'root', isActive: this.statusFilter === '' ? undefined : this.statusFilter === 'active',
      sorting: 'displayOrder asc, name asc', skipCount: this.page * this.pageSize, maxResultCount: this.pageSize })
      .pipe(finalize(() => this.loading = false)).subscribe(result => { this.items = result.items || []; this.totalCount = result.totalCount; });
  }
  loadLookup(): void { this.service.getLookup().subscribe(result => this.lookup = result.items || []); }
  showTree(): void { this.activeView = 'tree'; this.loading = true; this.service.getTree().pipe(finalize(() => this.loading = false)).subscribe(x => this.tree = x); }
  showList(): void { this.activeView = 'list'; this.loadList(); }
  create(): void { this.router.navigate(['/shop-management/product-categories/create']); }
  edit(row: ShopProductCategoryDto): void { this.router.navigate(['/shop-management/product-categories/edit', row.id]); }
  remove(row: ShopProductCategoryDto): void {
    this.confirmation.confirmDelete().subscribe(status => { if (status !== Confirmation.Status.confirm) return;
      this.service.delete(row.id).subscribe({ next: () => { this.toaster.success('::ProductCategoryDeletedSuccessfully'); this.loadLookup(); this.loadList(); }, error: e => this.showError(e) }); });
  }
  nextPage(): void { if ((this.page + 1) * this.pageSize < this.totalCount) { this.page++; this.loadList(); } }
  previousPage(): void { if (this.page > 0) { this.page--; this.loadList(); } }
  private showError(error: any): void { this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError'); }
}
