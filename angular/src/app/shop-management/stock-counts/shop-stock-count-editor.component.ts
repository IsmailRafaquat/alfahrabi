import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  CreateShopStockCountDto,
  ShopStockCountProductLookupDto,
  ShopStockCountScope,
  ShopStockCountService,
  ShopStockCountStatus,
  UpdateShopStockCountDto,
  shopStockCountScopeOptions,
} from '../../proxy/shop-management/stock-counts';
import { ShopProductCategoryLookupDto, ShopProductCategoryService } from '../../proxy/shop-management/product-categories';

@Component({ selector: 'app-shop-stock-count-editor', standalone: false, templateUrl: './shop-stock-count-editor.component.html', styleUrl: './shop-stock-count-editor.component.scss' })
export class ShopStockCountEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopStockCountService);
  private readonly categoryService = inject(ShopProductCategoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);

  readonly ShopStockCountScope = ShopStockCountScope;
  readonly scopeOptions = shopStockCountScopeOptions;

  categories: ShopProductCategoryLookupDto[] = [];
  products: ShopStockCountProductLookupDto[] = [];
  editId?: string;
  stockCountNumber = '';
  loading = false;
  submitting = false;
  helpOpen = false;

  get isEdit(): boolean {
    return !!this.editId;
  }

  readonly form = this.fb.group({
    countDate: [this.todayIso(), Validators.required],
    scope: [ShopStockCountScope.AllProducts, Validators.required],
    productCategoryId: [null as string | null],
    selectedProductIds: [[] as string[]],
    notes: ['', Validators.maxLength(1000)],
  });

  get eligibleProductCount(): number {
    const raw = this.form.getRawValue();
    const countable = this.products.filter(p => !p.trackBatch && !p.trackSerialNumber);

    switch (raw.scope) {
      case ShopStockCountScope.AllProducts:
        return countable.length;
      case ShopStockCountScope.Category:
        return raw.productCategoryId ? countable.filter(p => p.categoryId === raw.productCategoryId).length : 0;
      case ShopStockCountScope.SelectedProducts:
        return (raw.selectedProductIds || []).length;
      default:
        return 0;
    }
  }

  ngOnInit(): void {
    this.editId = this.route.snapshot.paramMap.get('id') || undefined;

    this.categoryService.getLookup().subscribe(result => (this.categories = result.items || []));
    this.service.getProductLookup().subscribe(result => (this.products = result.items || []));

    if (this.editId) this.loadForEdit(this.editId);
  }

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting) return;

    this.submitting = true;
    const raw = this.form.getRawValue();
    const input: CreateShopStockCountDto | UpdateShopStockCountDto = {
      countDate: raw.countDate!,
      scope: raw.scope,
      productCategoryId: raw.scope === ShopStockCountScope.Category ? raw.productCategoryId || undefined : undefined,
      selectedProductIds: raw.scope === ShopStockCountScope.SelectedProducts ? raw.selectedProductIds : undefined,
      notes: raw.notes || undefined,
    };

    const request = this.editId ? this.service.update(this.editId, input) : this.service.create(input);
    request.pipe(finalize(() => (this.submitting = false))).subscribe({
      next: saved => {
        this.toaster.success(this.isEdit ? '::StockCountUpdatedSuccessfully' : '::StockCountCreatedSuccessfully');
        this.router.navigate(['/shop-management/stock-counts', saved.id]);
      },
      error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
    });
  }

  cancel(): void {
    if (this.editId) this.router.navigate(['/shop-management/stock-counts', this.editId]);
    else this.router.navigate(['/shop-management/stock-counts']);
  }

  private loadForEdit(id: string): void {
    this.loading = true;
    this.service
      .get(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => {
        if (dto.status !== ShopStockCountStatus.Draft) {
          this.toaster.error('::StockCountCannotBeEdited');
          this.router.navigate(['/shop-management/stock-counts', id]);
          return;
        }

        this.stockCountNumber = dto.stockCountNumber || '';
        this.form.patchValue({
          countDate: dto.countDate!.substring(0, 10),
          scope: dto.scope,
          productCategoryId: dto.productCategoryId || null,
          selectedProductIds: dto.scope === ShopStockCountScope.SelectedProducts ? dto.items.map(x => x.productId!) : [],
          notes: dto.notes || '',
        });
      });
  }

  private todayIso(): string {
    return new Date().toISOString().substring(0, 10);
  }
}
