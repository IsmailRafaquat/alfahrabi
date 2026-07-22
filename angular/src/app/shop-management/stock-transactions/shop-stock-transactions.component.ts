import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { finalize } from 'rxjs';
import { ShopStockTransactionDto, ShopStockTransactionService, ShopStockTransactionType, shopStockTransactionTypeOptions } from '../../proxy/shop-management/stock-transactions';
import { ShopProductLookupDto, ShopProductService } from '../../proxy/shop-management/products';

@Component({ selector: 'app-shop-stock-transactions', standalone: false, templateUrl: './shop-stock-transactions.component.html', styleUrl: './shop-stock-transactions.component.scss' })
export class ShopStockTransactionsComponent implements OnInit {
  readonly Math = Math;
  readonly typeOptions = shopStockTransactionTypeOptions;

  private readonly service = inject(ShopStockTransactionService);
  private readonly productService = inject(ShopProductService);
  private readonly route = inject(ActivatedRoute);
  private readonly permissions = inject(PermissionService);

  readonly canViewCost = this.permissions.getGrantedPolicy('ShopManagement.StockTransactions.ViewCost');

  items: ShopStockTransactionDto[] = [];
  products: ShopProductLookupDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;

  search = '';
  productFilter = '';
  typeFilter: ShopStockTransactionType | '' = '';
  dateFrom: string | null = null;
  dateTo: string | null = null;

  tooltipLang: 'en' | 'ur' = 'en';

  ngOnInit(): void {
    this.productService.getLookup().subscribe(result => (this.products = result.items || []));
    const referenceNumber = this.route.snapshot.queryParamMap.get('referenceNumber');
    if (referenceNumber) this.search = referenceNumber;
    this.load();
  }

  load(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;
    this.service
      .getList({
        filter: this.search || undefined,
        productId: this.productFilter || undefined,
        transactionType: this.typeFilter === '' ? undefined : this.typeFilter,
        dateFrom: this.dateFrom || undefined,
        dateTo: this.dateTo || undefined,
        sorting: 'transactionDate desc',
        skipCount: this.page * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.items = result.items || [];
        this.totalCount = result.totalCount;
      });
  }

  typeLabel(type: ShopStockTransactionType): string {
    return '::' + ShopStockTransactionType[type];
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
