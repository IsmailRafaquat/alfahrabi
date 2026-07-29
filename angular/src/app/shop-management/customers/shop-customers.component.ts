import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { ShopCustomerDto, ShopCustomerService, ShopCustomerType } from '../../proxy/shop-management/customers';
import { ConfirmationHelperService } from '../../shared/services/confirmation-helper.service';

@Component({ selector: 'app-shop-customers', standalone: false, templateUrl: './shop-customers.component.html', styleUrl: './shop-customers.component.scss' })
export class ShopCustomersComponent implements OnInit {
  readonly Math = Math;
  readonly ShopCustomerType = ShopCustomerType;
  private readonly service = inject(ShopCustomerService);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationHelperService);
  private readonly toaster = inject(ToasterService);

  readonly canCreate = this.permissions.getGrantedPolicy('ShopManagement.Customers.Create');
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.Customers.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.Customers.Delete');
  readonly canViewBalance = this.permissions.getGrantedPolicy('ShopManagement.Customers.ViewBalance');

  items: ShopCustomerDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;

  filters: { filter?: string } = {};
  customerTypeFilter: ShopCustomerType | '' = '';
  cityFilter = '';
  countryFilter = '';
  statusFilter = '';

  tooltipLang: 'en' | 'ur' = 'en';

  ngOnInit(): void {
    this.load();
  }

  load(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;
    this.service
      .getList({
        filter: this.filters.filter || undefined,
        customerType: this.customerTypeFilter === '' ? undefined : this.customerTypeFilter,
        city: this.cityFilter || undefined,
        country: this.countryFilter || undefined,
        isActive: this.statusFilter === '' ? undefined : this.statusFilter === 'active',
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

  create(): void {
    this.router.navigate(['/shop-management/customers/create']);
  }

  view(row: ShopCustomerDto): void {
    this.router.navigate(['/shop-management/customers', row.id]);
  }

  edit(row: ShopCustomerDto): void {
    this.router.navigate(['/shop-management/customers', row.id, 'edit']);
  }

  remove(row: ShopCustomerDto): void {
    this.confirmation.confirmDelete().subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(row.id).subscribe({
        next: () => {
          this.toaster.success('::CustomerDeletedSuccessfully');
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
