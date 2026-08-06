import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  ShopSupplierPaymentDto,
  ShopSupplierPaymentMethod,
  ShopSupplierPaymentService,
  ShopSupplierPaymentStatus,
  ShopSupplierPaymentType,
  shopSupplierPaymentStatusOptions,
  shopSupplierPaymentTypeOptions,
} from '../../proxy/shop-management/supplier-payments';
import { ShopSupplierLookupDto, ShopSupplierService } from '../../proxy/shop-management/suppliers';

@Component({ selector: 'app-shop-supplier-payments', standalone: false, templateUrl: './shop-supplier-payments.component.html', styleUrl: './shop-supplier-payments.component.scss' })
export class ShopSupplierPaymentsComponent implements OnInit {
  readonly Math = Math;
  readonly ShopSupplierPaymentStatus = ShopSupplierPaymentStatus;
  readonly statusOptions = shopSupplierPaymentStatusOptions;
  readonly typeOptions = shopSupplierPaymentTypeOptions;

  private readonly service = inject(ShopSupplierPaymentService);
  private readonly supplierService = inject(ShopSupplierService);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);

  readonly canCreate = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.Create');
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.Delete');
  readonly canPost = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.Post');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.Cancel');
  readonly canViewAmount = this.permissions.getGrantedPolicy('ShopManagement.SupplierPayments.ViewAmount');

  items: ShopSupplierPaymentDto[] = [];
  suppliers: ShopSupplierLookupDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;

  filters: { filter?: string } = {};
  supplierFilter = '';
  statusFilter: ShopSupplierPaymentStatus | '' = '';
  typeFilter: ShopSupplierPaymentType | '' = '';
  paymentDateFrom: string | null = null;
  paymentDateTo: string | null = null;

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

  isDraft(row: ShopSupplierPaymentDto): boolean {
    return row.status === ShopSupplierPaymentStatus.Draft;
  }

  isPosted(row: ShopSupplierPaymentDto): boolean {
    return row.status === ShopSupplierPaymentStatus.Posted;
  }

  statusLabel(status?: ShopSupplierPaymentStatus): string {
    return '::' + ShopSupplierPaymentStatus[status ?? ShopSupplierPaymentStatus.Draft];
  }

  typeLabel(type?: ShopSupplierPaymentType): string {
    return '::' + ShopSupplierPaymentType[type ?? ShopSupplierPaymentType.Advance];
  }

  methodLabel(method?: ShopSupplierPaymentMethod): string {
    return '::' + ShopSupplierPaymentMethod[method ?? ShopSupplierPaymentMethod.Cash];
  }

  statusClass(status?: ShopSupplierPaymentStatus): string {
    switch (status) {
      case ShopSupplierPaymentStatus.Draft: return 'draft';
      case ShopSupplierPaymentStatus.Posted: return 'posted';
      case ShopSupplierPaymentStatus.Cancelled: return 'cancelled';
      default: return 'draft';
    }
  }

  create(): void {
    this.router.navigate(['/shop-management/supplier-payments/create']);
  }

  view(row: ShopSupplierPaymentDto): void {
    this.router.navigate(['/shop-management/supplier-payments', row.id]);
  }

  edit(row: ShopSupplierPaymentDto): void {
    this.router.navigate(['/shop-management/supplier-payments', row.id, 'edit']);
  }

  remove(row: ShopSupplierPaymentDto): void {
    this.confirmation.warn('::ConfirmDeleteSupplierPayment', row.paymentNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(row.id).subscribe({
        next: () => {
          this.toaster.success('::SupplierPaymentDeletedSuccessfully');
          this.load();
        },
        error: e => this.showError(e),
      });
    });
  }

  post(row: ShopSupplierPaymentDto): void {
    this.confirmation.warn('::ConfirmPostSupplierPayment', row.paymentNumber).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.post(row.id).subscribe({
        next: () => {
          this.toaster.success('::SupplierPaymentPostedSuccessfully');
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
