import { Component, OnInit, inject } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { CreateUpdateShopProductCategoryDto, ShopProductCategoryDto, ShopProductCategoryLookupDto, ShopProductCategoryService } from '../../proxy/shop-management/product-categories';

@Component({ selector: 'app-shop-product-category-editor', standalone: false, templateUrl: './shop-product-category-editor.component.html', styleUrls: ['./shop-product-category-editor.component.scss', './shop-product-category-editor-list.component.scss'] })
export class ShopProductCategoryEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopProductCategoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);
  lookup: ShopProductCategoryLookupDto[] = [];
  submitting = false; loading = false; editId?: string;
  savedItems: ShopProductCategoryDto[] = [];
  helpOpen = false;
  helpLang: 'en' | 'ur' = 'en';
  readonly form = this.fb.group({ categories: this.fb.array<FormGroup>([]) });
  get categories(): FormArray<FormGroup> { return this.form.controls.categories; }
  get isEdit(): boolean { return !!this.editId; }

  ngOnInit(): void {
    this.editId = this.route.snapshot.paramMap.get('id') || undefined;
    this.service.getLookup().subscribe(result => { this.lookup = result.items || []; if (this.editId) this.loadEdit(); else this.addCategory(); });
  }
  addCategory(): void { this.categories.push(this.createRow()); }
  removeCategory(index: number): void { if (this.categories.length > 1) this.categories.removeAt(index); }
  parentOptions(index: number): ShopProductCategoryLookupDto[] {
    const editId = this.editId;
    return this.lookup.filter(x => !x.parentCategoryId && x.id !== editId);
  }
  save(): void {
    this.form.markAllAsTouched(); if (this.form.invalid || this.submitting) return;
    this.submitting = true;
    const input = this.categories.at(0).getRawValue() as CreateUpdateShopProductCategoryDto;
    const request = this.editId ? this.service.update(this.editId, input) : this.service.create(input);
    request.pipe(finalize(() => this.submitting = false)).subscribe({ next: saved => {
      this.toaster.success(this.isEdit ? '::ProductCategoryUpdatedSuccessfully' : '::ProductCategoryCreatedSuccessfully');
      const existingIndex = this.savedItems.findIndex(x => x.id === saved.id);
      if (existingIndex >= 0) this.savedItems[existingIndex] = saved;
      else this.savedItems = [saved, ...this.savedItems];
      this.editId = undefined;
      this.categories.clear(); this.addCategory();
      this.loadLookup();
    }, error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError') });
  }
  cancel(): void { this.router.navigate(['/shop-management/product-categories']); }
  private loadEdit(): void {
    this.loading = true; this.service.get(this.editId!).pipe(finalize(() => this.loading = false)).subscribe(dto => {
      this.categories.push(this.createRow({ name: dto.name, code: dto.code, parentCategoryId: dto.parentCategoryId || null,
        description: dto.description || '', displayOrder: dto.displayOrder, isActive: dto.isActive }));
    });
  }
  editSaved(dto: ShopProductCategoryDto): void {
    this.editId = dto.id;
    this.categories.clear();
    this.categories.push(this.createRow({ name: dto.name, code: dto.code, parentCategoryId: dto.parentCategoryId || null,
      description: dto.description || '', displayOrder: dto.displayOrder, isActive: dto.isActive }));
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
  cancelEdit(): void { this.editId = undefined; this.categories.clear(); this.addCategory(); }
  private loadLookup(): void { this.service.getLookup().subscribe(result => this.lookup = result.items || []); }
  private createRow(value: any = {}): FormGroup { return this.fb.group({
    name: [value.name || '', [Validators.required, Validators.maxLength(128)]],
    code: [value.code || '', [Validators.required, Validators.maxLength(64)]],
    parentCategoryId: [value.parentCategoryId || null], description: [value.description || '', Validators.maxLength(500)],
    displayOrder: [value.displayOrder ?? 0, [Validators.required, Validators.min(0)]], isActive: [value.isActive ?? true],
  }); }
}
