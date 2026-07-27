import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  ShopStockCountDto,
  ShopStockCountItemDto,
  ShopStockCountService,
  ShopStockCountStatus,
} from '../../proxy/shop-management/stock-counts';

interface CountingRow {
  itemId: string;
  productCode?: string;
  productName?: string;
  unitName?: string;
  unitShortName?: string;
  unitAllowDecimal: boolean;
  systemQuantity: number;
  physicalQuantity: number | null;
  differenceQuantity: number;
  isCounted: boolean;
  notes: string;
  saving: boolean;
  savedPhysicalQuantity: number | null;
  savedNotes: string;
}

@Component({ selector: 'app-shop-stock-count-counting', standalone: false, templateUrl: './shop-stock-count-counting.component.html', styleUrl: './shop-stock-count-counting.component.scss' })
export class ShopStockCountCountingComponent implements OnInit {
  private readonly service = inject(ShopStockCountService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);

  readonly ShopStockCountStatus = ShopStockCountStatus;

  id!: string;
  dto?: ShopStockCountDto;
  rows: CountingRow[] = [];
  loading = false;
  starting = false;
  completing = false;

  search = '';
  showOnlyUncounted = false;

  get visibleRows(): CountingRow[] {
    return this.rows.filter(row => {
      if (this.showOnlyUncounted && row.isCounted) return false;
      if (!this.search) return true;
      const term = this.search.toLowerCase();
      return (row.productCode || '').toLowerCase().includes(term) || (row.productName || '').toLowerCase().includes(term);
    });
  }

  get allCounted(): boolean {
    return this.rows.length > 0 && this.rows.every(row => row.isCounted);
  }

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  load(): void {
    this.loading = true;
    this.service
      .get(this.id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => {
        if (dto.status === ShopStockCountStatus.Counted || dto.status === ShopStockCountStatus.Posted || dto.status === ShopStockCountStatus.Cancelled) {
          this.router.navigate(['/shop-management/stock-counts', dto.id]);
          return;
        }
        this.applyDto(dto);
      });
  }

  start(): void {
    this.starting = true;
    this.service
      .start(this.id)
      .pipe(finalize(() => (this.starting = false)))
      .subscribe({
        next: dto => {
          this.toaster.success('::StockCountStartedSuccessfully');
          this.applyDto(dto);
        },
        error: e => this.showError(e),
      });
  }

  saveRow(row: CountingRow, index: number, moveNext: boolean): void {
    if (row.physicalQuantity == null || row.saving) return;

    // Re-rendering the grid after a save can trigger a spurious blur on the input the
    // user just left; skip it if nothing actually changed since the last save.
    if (row.isCounted && row.physicalQuantity === row.savedPhysicalQuantity && row.notes === row.savedNotes) {
      if (moveNext) this.focusRow(index + 1);
      return;
    }

    row.saving = true;
    this.service
      .updateItemQuantity(this.id, { stockCountItemId: row.itemId, physicalQuantity: row.physicalQuantity, notes: row.notes || undefined })
      .pipe(finalize(() => (row.saving = false)))
      .subscribe({
        next: dto => {
          this.applyDto(dto);
          if (moveNext) this.focusRow(index + 1);
        },
        error: e => this.showError(e),
      });
  }

  onQuantityKeydown(event: KeyboardEvent, row: CountingRow, index: number): void {
    if (event.key === 'Enter') {
      event.preventDefault();
      this.saveRow(row, index, true);
    }
  }

  completeCount(): void {
    this.completing = true;
    this.service
      .completeCount(this.id)
      .pipe(finalize(() => (this.completing = false)))
      .subscribe({
        next: () => {
          this.toaster.success('::StockCountCompletedSuccessfully');
          this.router.navigate(['/shop-management/stock-counts', this.id]);
        },
        error: e => this.showError(e),
      });
  }

  back(): void {
    this.router.navigate(['/shop-management/stock-counts']);
  }

  private applyDto(dto: ShopStockCountDto): void {
    this.dto = dto;
    this.rows = dto.items.map(item => this.toRow(item));
  }

  private toRow(item: ShopStockCountItemDto): CountingRow {
    return {
      itemId: item.id,
      productCode: item.productCode,
      productName: item.productName,
      unitName: item.unitName,
      unitShortName: item.unitShortName,
      unitAllowDecimal: item.unitAllowDecimal,
      systemQuantity: item.systemQuantity,
      physicalQuantity: item.physicalQuantity ?? null,
      differenceQuantity: item.differenceQuantity,
      isCounted: item.isCounted,
      notes: item.notes || '',
      saving: false,
      savedPhysicalQuantity: item.physicalQuantity ?? null,
      savedNotes: item.notes || '',
    };
  }

  trackByItemId(_index: number, row: CountingRow): string {
    return row.itemId;
  }

  private focusRow(index: number): void {
    setTimeout(() => {
      const el = document.getElementById(`pq-${index}`) as HTMLInputElement | null;
      el?.focus();
      el?.select();
    });
  }

  private showError(error: any): void {
    this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError');
  }
}
