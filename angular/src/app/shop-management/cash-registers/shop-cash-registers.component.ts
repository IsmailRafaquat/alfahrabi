import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { CreateUpdateShopCashRegisterDto, ShopCashRegisterDto, ShopCashRegisterService } from '../../proxy/shop-management/cash-registers';

@Component({ selector: 'app-shop-cash-registers', standalone: false, templateUrl: './shop-cash-registers.component.html', styleUrl: './shop-cash-registers.component.scss' })
export class ShopCashRegistersComponent implements OnInit {
  private readonly service = inject(ShopCashRegisterService);
  private readonly fb = inject(FormBuilder);
  private readonly permissions = inject(PermissionService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);

  readonly Math = Math;
  readonly canCreate = this.permissions.getGrantedPolicy('ShopManagement.CashRegisters.Create');
  readonly canEdit = this.permissions.getGrantedPolicy('ShopManagement.CashRegisters.Edit');
  readonly canDelete = this.permissions.getGrantedPolicy('ShopManagement.CashRegisters.Delete');

  items: ShopCashRegisterDto[] = [];
  totalCount = 0;
  page = 0;
  pageSize = 10;
  loading = false;
  submitting = false;
  modalOpen = false;
  selected?: ShopCashRegisterDto;

  filters: { filter?: string } = {};
  statusFilter: boolean | null = null;

  readonly form = this.fb.group({
    code: ['', [Validators.required, Validators.maxLength(32)]],
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', Validators.maxLength(500)],
    isDefault: [false],
    isActive: [true],
  });

  ngOnInit(): void {
    this.load();
  }

  load(reset = false): void {
    if (reset) this.page = 0;
    this.loading = true;
    this.service
      .getList({
        filter: this.filters.filter || undefined,
        isActive: this.statusFilter ?? undefined,
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
    this.selected = undefined;
    this.form.reset({ code: '', name: '', description: '', isDefault: false, isActive: true });
    this.modalOpen = true;
  }

  edit(row: ShopCashRegisterDto): void {
    this.service.get(row.id).subscribe(dto => {
      this.selected = dto;
      this.form.patchValue({
        code: dto.code,
        name: dto.name,
        description: dto.description || '',
        isDefault: dto.isDefault,
        isActive: dto.isActive,
      });
      this.modalOpen = true;
    });
  }

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting) return;

    this.submitting = true;
    const input = this.form.getRawValue() as CreateUpdateShopCashRegisterDto;
    const request = this.selected ? this.service.update(this.selected.id, input) : this.service.create(input);
    request.pipe(finalize(() => (this.submitting = false))).subscribe({
      next: () => {
        this.toaster.success(this.selected ? '::CashRegisterUpdatedSuccessfully' : '::CashRegisterCreatedSuccessfully');
        this.modalOpen = false;
        this.load();
      },
      error: e => this.showError(e),
    });
  }

  remove(row: ShopCashRegisterDto): void {
    this.confirmation.warn('::ConfirmDeleteCashRegister', row.name).subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.service.delete(row.id).subscribe({
        next: () => {
          this.toaster.success('::CashRegisterDeletedSuccessfully');
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
