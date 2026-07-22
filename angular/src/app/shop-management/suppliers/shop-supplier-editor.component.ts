import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { CreateUpdateShopSupplierDto, ShopSupplierDto, ShopSupplierService } from '../../proxy/shop-management/suppliers';
import { ShopSupplierBalanceSummaryDto, ShopSupplierLedgerService } from '../../proxy/shop-management/supplier-ledger';

const BLANK_VALUE = {
  code: '',
  name: '',
  contactPerson: '',
  phone: '',
  alternatePhone: '',
  email: '',
  taxNumber: '',
  addressLine1: '',
  addressLine2: '',
  city: '',
  stateOrProvince: '',
  postalCode: '',
  country: '',
  openingBalance: 0,
  creditLimit: 0,
  paymentTermsDays: 0,
  notes: '',
  isActive: true,
};

@Component({ selector: 'app-shop-supplier-editor', standalone: false, templateUrl: './shop-supplier-editor.component.html', styleUrl: './shop-supplier-editor.component.scss' })
export class ShopSupplierEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopSupplierService);
  private readonly ledgerService = inject(ShopSupplierLedgerService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly canViewBalance = this.permissions.getGrantedPolicy('ShopManagement.Suppliers.ViewBalance');
  readonly canViewLedger = this.permissions.getGrantedPolicy('ShopManagement.SupplierLedger');
  readonly canViewLedgerAmounts = this.permissions.getGrantedPolicy('ShopManagement.SupplierLedger.ViewAmounts');

  editId?: string;
  loading = false;
  submitting = false;
  savedItems: ShopSupplierDto[] = [];
  helpOpen = false;
  ledgerSummary?: ShopSupplierBalanceSummaryDto;

  get isEdit(): boolean {
    return !!this.editId;
  }

  readonly form = this.fb.group({
    code: [BLANK_VALUE.code, [Validators.required, Validators.maxLength(64)]],
    name: [BLANK_VALUE.name, [Validators.required, Validators.maxLength(200)]],
    contactPerson: [BLANK_VALUE.contactPerson, Validators.maxLength(128)],
    phone: [BLANK_VALUE.phone, Validators.maxLength(32)],
    alternatePhone: [BLANK_VALUE.alternatePhone, Validators.maxLength(32)],
    email: [BLANK_VALUE.email, [Validators.email, Validators.maxLength(256)]],
    taxNumber: [BLANK_VALUE.taxNumber, Validators.maxLength(64)],
    addressLine1: [BLANK_VALUE.addressLine1, Validators.maxLength(256)],
    addressLine2: [BLANK_VALUE.addressLine2, Validators.maxLength(256)],
    city: [BLANK_VALUE.city, Validators.maxLength(100)],
    stateOrProvince: [BLANK_VALUE.stateOrProvince, Validators.maxLength(100)],
    postalCode: [BLANK_VALUE.postalCode, Validators.maxLength(32)],
    country: [BLANK_VALUE.country, Validators.maxLength(100)],
    openingBalance: [BLANK_VALUE.openingBalance, [Validators.required, Validators.min(0)]],
    creditLimit: [BLANK_VALUE.creditLimit, [Validators.required, Validators.min(0)]],
    paymentTermsDays: [BLANK_VALUE.paymentTermsDays, [Validators.required, Validators.min(0)]],
    notes: [BLANK_VALUE.notes, Validators.maxLength(1000)],
    isActive: [BLANK_VALUE.isActive],
  });

  ngOnInit(): void {
    this.editId = this.route.snapshot.paramMap.get('id') || undefined;
    if (this.editId) this.loadForEdit(this.editId);
  }

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting) return;

    this.submitting = true;
    const raw = this.form.getRawValue();
    const input: CreateUpdateShopSupplierDto = {
      code: raw.code,
      name: raw.name,
      contactPerson: raw.contactPerson || undefined,
      phone: raw.phone || undefined,
      alternatePhone: raw.alternatePhone || undefined,
      email: raw.email || undefined,
      taxNumber: raw.taxNumber || undefined,
      addressLine1: raw.addressLine1 || undefined,
      addressLine2: raw.addressLine2 || undefined,
      city: raw.city || undefined,
      stateOrProvince: raw.stateOrProvince || undefined,
      postalCode: raw.postalCode || undefined,
      country: raw.country || undefined,
      openingBalance: raw.openingBalance,
      creditLimit: raw.creditLimit,
      paymentTermsDays: raw.paymentTermsDays,
      notes: raw.notes || undefined,
      isActive: raw.isActive,
    };

    const request = this.editId ? this.service.update(this.editId, input) : this.service.create(input);
    request.pipe(finalize(() => (this.submitting = false))).subscribe({
      next: saved => {
        this.toaster.success(this.isEdit ? '::SupplierUpdatedSuccessfully' : '::SupplierCreatedSuccessfully');
        const existingIndex = this.savedItems.findIndex(x => x.id === saved.id);
        if (existingIndex >= 0) this.savedItems[existingIndex] = saved;
        else this.savedItems = [saved, ...this.savedItems];
        this.editId = undefined;
        this.resetForm();
      },
      error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
    });
  }

  editSaved(dto: ShopSupplierDto): void {
    this.editId = dto.id;
    this.patchForm(dto);
    this.loadLedgerSummary(dto.id);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  cancelEdit(): void {
    this.editId = undefined;
    this.ledgerSummary = undefined;
    this.resetForm();
  }

  viewLedger(): void {
    if (this.editId) this.router.navigate(['/shop-management/supplier-ledger', this.editId]);
  }

  done(): void {
    this.router.navigate(['/shop-management/suppliers']);
  }

  private resetForm(): void {
    this.form.reset(BLANK_VALUE);
  }

  private patchForm(dto: ShopSupplierDto): void {
    this.form.patchValue({
      code: dto.code,
      name: dto.name,
      contactPerson: dto.contactPerson || '',
      phone: dto.phone || '',
      alternatePhone: dto.alternatePhone || '',
      email: dto.email || '',
      taxNumber: dto.taxNumber || '',
      addressLine1: dto.addressLine1 || '',
      addressLine2: dto.addressLine2 || '',
      city: dto.city || '',
      stateOrProvince: dto.stateOrProvince || '',
      postalCode: dto.postalCode || '',
      country: dto.country || '',
      openingBalance: dto.openingBalance ?? 0,
      creditLimit: dto.creditLimit ?? 0,
      paymentTermsDays: dto.paymentTermsDays,
      notes: dto.notes || '',
      isActive: dto.isActive,
    });
  }

  private loadForEdit(id: string): void {
    this.loading = true;
    this.service
      .get(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => {
        this.patchForm(dto);
        this.loadLedgerSummary(id);
      });
  }

  private loadLedgerSummary(supplierId: string): void {
    this.ledgerSummary = undefined;
    if (!this.canViewLedger) return;
    this.ledgerService.getBalanceSummary(supplierId).subscribe({
      next: summary => (this.ledgerSummary = summary),
      error: () => (this.ledgerSummary = undefined),
    });
  }
}
