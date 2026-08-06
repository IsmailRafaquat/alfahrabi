import { Component, OnInit, inject } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  ShopCashDirection,
  ShopCashRegisterLookupDto,
  ShopCashRegisterService,
  ShopCashRegisterTransactionDto,
  ShopCashTransactionType,
  shopCashTransactionTypeOptions,
} from '../../proxy/shop-management/cash-registers';

@Component({ selector: 'app-shop-cash-transactions', standalone: false, templateUrl: './shop-cash-transactions.component.html', styleUrl: './shop-cash-transactions.component.scss' })
export class ShopCashTransactionsComponent implements OnInit {
  private readonly service = inject(ShopCashRegisterService);
  private readonly permissions = inject(PermissionService);
  private readonly router = inject(Router);

  readonly Math = Math;
  readonly ShopCashDirection = ShopCashDirection;
  readonly typeOptions = shopCashTransactionTypeOptions;
  readonly canViewAmounts = this.permissions.getGrantedPolicy('ShopManagement.CashTransactions.ViewAmounts');

  registers: ShopCashRegisterLookupDto[] = [];
  items: ShopCashRegisterTransactionDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 20;
  loading = false;

  filters: { filter?: string } = {};
  registerFilter = '';
  typeFilter: ShopCashTransactionType | '' = '';
  directionFilter: ShopCashDirection | '' = '';
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
      .getTransactions({
        filter: this.filters.filter || undefined,
        cashRegisterId: this.registerFilter || undefined,
        transactionType: this.typeFilter === '' ? undefined : this.typeFilter,
        direction: this.directionFilter === '' ? undefined : this.directionFilter,
        transactionDateFrom: this.dateFrom || undefined,
        transactionDateTo: this.dateTo || undefined,
        sorting: 'transactionDate desc, creationTime desc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });
  }

  typeLabel(type: ShopCashTransactionType): string {
    return '::' + ShopCashTransactionType[type];
  }

  directionLabel(direction: ShopCashDirection): string {
    return direction === ShopCashDirection.In ? '::DirectionIn' : '::DirectionOut';
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
