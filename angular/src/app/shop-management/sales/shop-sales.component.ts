import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { ShopSaleDto, ShopSaleService, ShopSaleStatus, ShopSaleType, shopSaleStatusOptions, shopSaleTypeOptions } from '../../proxy/shop-management/sales';
import { ShopCustomerLookupDto, ShopCustomerService } from '../../proxy/shop-management/customers';

@Component({ selector: 'app-shop-sales', standalone: false, templateUrl: './shop-sales.component.html', styleUrl: './shop-sales.component.scss' })
export class ShopSalesComponent implements OnInit {
  readonly Math = Math;
  readonly ShopSaleStatus = ShopSaleStatus;
  readonly statusOptions = shopSaleStatusOptions;
  readonly saleTypeOptions = shopSaleTypeOptions;

  private readonly service = inject(ShopSaleService);
  private readonly customerService = inject(ShopCustomerService);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);

  readonly canCreate = this.permissions.getGrantedPolicy('ShopManagement.Sales.Create');
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.Sales.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.Sales.Delete');
  readonly canViewPrice = this.permissions.getGrantedPolicy('ShopManagement.Sales.ViewPrice');

  items: ShopSaleDto[] = [];
  customers: ShopCustomerLookupDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;

  search = '';
  customerFilter = '';
  statusFilter: ShopSaleStatus | '' = '';
  saleTypeFilter: ShopSaleType | '' = '';
  saleDateFrom: string | null = null;
  saleDateTo: string | null = null;

  tooltipLang: 'en' | 'ur' = 'en';

  ngOnInit(): void {
    this.customerService.getLookup().subscribe(result => (this.customers = result.items || []));
    this.load();
  }

  load(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;
    this.service
      .getList({
        filter: this.search || undefined,
        customerId: this.customerFilter || undefined,
        status: this.statusFilter === '' ? undefined : this.statusFilter,
        saleType: this.saleTypeFilter === '' ? undefined : this.saleTypeFilter,
        saleDateFrom: this.saleDateFrom || undefined,
        saleDateTo: this.saleDateTo || undefined,
        sorting: 'saleDate desc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });
  }

  isDraft(row: ShopSaleDto): boolean {
    return row.status === ShopSaleStatus.Draft;
  }

  statusLabel(status: ShopSaleStatus): string {
    return '::' + ShopSaleStatus[status];
  }

  statusClass(status: ShopSaleStatus): string {
    switch (status) {
      case ShopSaleStatus.Draft: return 'draft';
      case ShopSaleStatus.Completed: return 'completed';
      case ShopSaleStatus.Cancelled: return 'cancelled';
      default: return 'draft';
    }
  }

  view(row: ShopSaleDto): void {
    this.router.navigate(['/shop-management/sales', row.id]);
  }

  create(): void {
    this.router.navigate(['/shop-management/sales/create']);
  }

  edit(row: ShopSaleDto): void {
    this.router.navigate(['/shop-management/sales', row.id, 'edit']);
  }

  remove(row: ShopSaleDto): void {
    this.confirmation.warn('::ConfirmDeleteSale', row.saleNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(row.id).subscribe({
        next: () => {
          this.toaster.success('::SaleDeletedSuccessfully');
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
