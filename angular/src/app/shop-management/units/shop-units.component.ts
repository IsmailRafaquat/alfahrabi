import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { CreateUpdateShopUnitDto, ShopUnitDto, ShopUnitService } from '../../proxy/shop-management/units';
import { ConfirmationHelperService } from '../../shared/services/confirmation-helper.service';
@Component({ selector: 'app-shop-units', standalone: false, templateUrl: './shop-units.component.html', styleUrls: ['./shop-units.component.scss', './shop-units-modal.component.scss'] })
export class ShopUnitsComponent implements OnInit {
  private readonly service = inject(ShopUnitService); private readonly fb = inject(FormBuilder); private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationHelperService); private readonly toaster = inject(ToasterService);
  readonly canCreate = this.permissions.getGrantedPolicy('ShopManagement.Units.Create'); readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.Units.Edit'); readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.Units.Delete'); readonly Math = Math;
  items: ShopUnitDto[] = []; totalCount = 0; page = 0; pageSize = 10; loading = false; submitting = false; modalOpen = false; selected?: ShopUnitDto;
  filters: { filter?: string } = {}; quantityFilter: boolean | null = null; statusFilter: boolean | null = null;
  tooltipLang: 'en' | 'ur' = 'en';
  readonly form = this.fb.group({ name: ['', [Validators.required, Validators.maxLength(64)]], shortName: ['', [Validators.required, Validators.maxLength(16)]], allowDecimal: [false], isActive: [true] });
  ngOnInit(): void { this.load(); }
  load(reset = false): void { if (reset) this.page = 0; this.loading = true; this.service.getList({ filter: this.filters.filter || undefined, allowDecimal: this.quantityFilter ?? undefined, isActive: this.statusFilter ?? undefined, sorting: 'name asc', skipCount: this.page * this.pageSize, maxResultCount: this.pageSize }).pipe(finalize(() => this.loading = false)).subscribe(r => { this.items = r.items || []; this.totalCount = r.totalCount; }); }
  create(): void { this.selected = undefined; this.form.reset({ name: '', shortName: '', allowDecimal: false, isActive: true }); this.modalOpen = true; }
  edit(row: ShopUnitDto): void { this.service.get(row.id).subscribe(dto => { this.selected = dto; this.form.patchValue(dto); this.modalOpen = true; }); }
  save(): void { this.form.markAllAsTouched(); if (this.form.invalid || this.submitting) return; this.submitting = true; const input = this.form.getRawValue() as CreateUpdateShopUnitDto; const request = this.selected ? this.service.update(this.selected.id, input) : this.service.create(input); request.pipe(finalize(() => this.submitting = false)).subscribe({ next: () => { this.toaster.success(this.selected ? '::UnitUpdatedSuccessfully' : '::UnitCreatedSuccessfully'); this.modalOpen = false; this.load(); }, error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError') }); }
  remove(row: ShopUnitDto): void { this.confirmation.confirmDelete().subscribe(s => { if (s !== Confirmation.Status.confirm) return; this.service.delete(row.id).subscribe({ next: () => { this.toaster.success('::UnitDeletedSuccessfully'); this.load(); }, error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError') }); }); }
  previous(): void { if (this.page > 0) { this.page--; this.load(); } } next(): void { if ((this.page + 1) * this.pageSize < this.totalCount) { this.page++; this.load(); } }
}
