import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  ShopCashClosingDto,
  ShopCashClosingStatus,
  ShopCashRegisterLookupDto,
  ShopCashRegisterService,
} from '../../proxy/shop-management/cash-registers';

@Component({ selector: 'app-shop-cash-closings', standalone: false, templateUrl: './shop-cash-closings.component.html', styleUrl: './shop-cash-closings.component.scss' })
export class ShopCashClosingsComponent implements OnInit {
  private readonly service = inject(ShopCashRegisterService);
  private readonly permissions = inject(PermissionService);
  private readonly router = inject(Router);

  readonly Math = Math;
  readonly ShopCashClosingStatus = ShopCashClosingStatus;
  readonly canViewAmounts = this.permissions.getGrantedPolicy('ShopManagement.CashClosings.ViewAmounts');

  registers: ShopCashRegisterLookupDto[] = [];
  items: ShopCashClosingDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 20;
  loading = false;

  registerFilter = '';
  dateFrom: string | null = null;
  dateTo: string | null = null;

  ngOnInit(): void {
    this.service.getLookup().subscribe(result => (this.registers = result.items || []));
    this.load();
  }

  load(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;
    this.service
      .getClosings({
        cashRegisterId: this.registerFilter || undefined,
        transactionDateFrom: this.dateFrom || undefined,
        transactionDateTo: this.dateTo || undefined,
        sorting: 'businessDate desc, creationTime desc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });
  }

  statusLabel(status: ShopCashClosingStatus): string {
    return '::' + ShopCashClosingStatus[status];
  }

  statusClass(status: ShopCashClosingStatus): string {
    switch (status) {
      case ShopCashClosingStatus.Open: return 'open';
      case ShopCashClosingStatus.Closed: return 'closed';
      case ShopCashClosingStatus.Cancelled: return 'cancelled';
      default: return 'open';
    }
  }

  view(row: ShopCashClosingDto): void {
    this.router.navigate(['/shop-management/cash-register/closings', row.id]);
  }

  back(): void {
    this.router.navigate(['/shop-management/cash-register']);
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
