import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  ShopCustomerPaymentDto,
  ShopCustomerPaymentMethod,
  ShopCustomerPaymentService,
  ShopCustomerPaymentStatus,
  ShopCustomerPaymentType,
  shopCustomerPaymentStatusOptions,
  shopCustomerPaymentTypeOptions,
} from '../../proxy/shop-management/customer-payments';
import { ShopCustomerLookupDto, ShopCustomerService } from '../../proxy/shop-management/customers';

@Component({ selector: 'app-shop-customer-payments', standalone: false, templateUrl: './shop-customer-payments.component.html', styleUrl: './shop-customer-payments.component.scss' })
export class ShopCustomerPaymentsComponent implements OnInit {
  readonly Math = Math;
  readonly ShopCustomerPaymentStatus = ShopCustomerPaymentStatus;
  readonly statusOptions = shopCustomerPaymentStatusOptions;
  readonly typeOptions = shopCustomerPaymentTypeOptions;

  private readonly service = inject(ShopCustomerPaymentService);
  private readonly customerService = inject(ShopCustomerService);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);

  readonly canCreate = this.permissions.getGrantedPolicy('ShopManagement.CustomerPayments.Create');
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.CustomerPayments.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.CustomerPayments.Delete');
  readonly canPost = this.permissions.getGrantedPolicy('ShopManagement.CustomerPayments.Post');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.CustomerPayments.Cancel');
  readonly canViewAmount = this.permissions.getGrantedPolicy('ShopManagement.CustomerPayments.ViewAmount');

  items: ShopCustomerPaymentDto[] = [];
  customers: ShopCustomerLookupDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;

  search = '';
  customerFilter = '';
  statusFilter: ShopCustomerPaymentStatus | '' = '';
  typeFilter: ShopCustomerPaymentType | '' = '';
  paymentDateFrom: string | null = null;
  paymentDateTo: string | null = null;

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
        paymentType: this.typeFilter === '' ? undefined : this.typeFilter,
        paymentDateFrom: this.paymentDateFrom || undefined,
        paymentDateTo: this.paymentDateTo || undefined,
        sorting: 'paymentDate desc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });
  }

  isDraft(row: ShopCustomerPaymentDto): boolean {
    return row.status === ShopCustomerPaymentStatus.Draft;
  }

  statusLabel(status?: ShopCustomerPaymentStatus): string {
    return '::' + ShopCustomerPaymentStatus[status ?? ShopCustomerPaymentStatus.Draft];
  }

  typeLabel(type?: ShopCustomerPaymentType): string {
    return '::' + ShopCustomerPaymentType[type ?? ShopCustomerPaymentType.Advance];
  }

  methodLabel(method?: ShopCustomerPaymentMethod): string {
    return '::' + ShopCustomerPaymentMethod[method ?? ShopCustomerPaymentMethod.Cash];
  }

  statusClass(status?: ShopCustomerPaymentStatus): string {
    switch (status) {
      case ShopCustomerPaymentStatus.Draft: return 'draft';
      case ShopCustomerPaymentStatus.Posted: return 'posted';
      case ShopCustomerPaymentStatus.Cancelled: return 'cancelled';
      default: return 'draft';
    }
  }

  create(): void {
    this.router.navigate(['/shop-management/customer-payments/create']);
  }

  view(row: ShopCustomerPaymentDto): void {
    this.router.navigate(['/shop-management/customer-payments', row.id]);
  }

  edit(row: ShopCustomerPaymentDto): void {
    this.router.navigate(['/shop-management/customer-payments', row.id, 'edit']);
  }

  remove(row: ShopCustomerPaymentDto): void {
    this.confirmation.warn('::ConfirmDeleteCustomerPayment', row.paymentNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(row.id).subscribe({
        next: () => {
          this.toaster.success('::CustomerPaymentDeletedSuccessfully');
          this.load();
        },
        error: e => this.showError(e),
      });
    });
  }

  post(row: ShopCustomerPaymentDto): void {
    this.confirmation.warn('::ConfirmPostCustomerPayment', row.paymentNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.post(row.id).subscribe({
        next: () => {
          this.toaster.success('::CustomerPaymentPostedSuccessfully');
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
