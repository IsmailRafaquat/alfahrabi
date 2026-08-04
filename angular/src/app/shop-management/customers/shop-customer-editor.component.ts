import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { CreateUpdateShopCustomerDto, ShopCustomerDto, ShopCustomerService, ShopCustomerType } from '../../proxy/shop-management/customers';
import { ShopCustomerBalanceSummaryDto, ShopCustomerLedgerService } from '../../proxy/shop-management/customer-ledger';

const BLANK_VALUE = {
  code: '',
  name: '',
  customerType: ShopCustomerType.Individual,
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
  isWalkInCustomer: false,
  isActive: true,
};

@Component({ selector: 'app-shop-customer-editor', standalone: false, templateUrl: './shop-customer-editor.component.html', styleUrl: './shop-customer-editor.component.scss' })
export class ShopCustomerEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopCustomerService);
  private readonly ledgerService = inject(ShopCustomerLedgerService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);

  readonly ShopCustomerType = ShopCustomerType;
  readonly canViewBalance = this.permissions.getGrantedPolicy('ShopManagement.Customers.ViewBalance');
  readonly canViewLedger = this.permissions.getGrantedPolicy('ShopManagement.CustomerLedger');
  readonly canViewLedgerAmounts = this.permissions.getGrantedPolicy('ShopManagement.CustomerLedger.ViewAmounts');

  id?: string;
  loading = false;
  submitting = false;
  readonlyMode = false;
  helpOpen = false;
  helpLang: 'en' | 'ur' = 'en';
  ledgerSummary?: ShopCustomerBalanceSummaryDto;

  get isEdit(): boolean {
    return !!this.id;
  }

  readonly form = this.fb.group({
    code: [BLANK_VALUE.code, [Validators.required, Validators.maxLength(64)]],
    name: [BLANK_VALUE.name, [Validators.required, Validators.maxLength(200)]],
    customerType: [BLANK_VALUE.customerType, Validators.required],
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
    isWalkInCustomer: [BLANK_VALUE.isWalkInCustomer],
    isActive: [BLANK_VALUE.isActive],
  });

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id') || undefined;
    this.readonlyMode = !!this.id && !this.router.url.endsWith('/edit');

    this.form.controls.isWalkInCustomer.valueChanges.subscribe(isWalkIn => this.onWalkInToggle(!!isWalkIn));

    if (this.id) this.loadForEdit(this.id);
  }

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting || this.readonlyMode) return;

    this.submitting = true;
    const raw = this.form.getRawValue();
    const input: CreateUpdateShopCustomerDto = {
      code: raw.code,
      name: raw.name,
      customerType: raw.customerType,
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
      isWalkInCustomer: raw.isWalkInCustomer,
      isActive: raw.isActive,
    };

    const request = this.id ? this.service.update(this.id, input) : this.service.create(input);
    request.pipe(finalize(() => (this.submitting = false))).subscribe({
      next: () => {
        this.toaster.success(this.isEdit ? '::CustomerUpdatedSuccessfully' : '::CustomerCreatedSuccessfully');
        this.done();
      },
      error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
    });
  }

  done(): void {
    this.router.navigate(['/shop-management/customers']);
  }

  viewLedger(): void {
    if (this.id) this.router.navigate(['/shop-management/customer-ledger', this.id]);
  }

  private onWalkInToggle(isWalkIn: boolean): void {
    if (isWalkIn) {
      this.form.controls.customerType.setValue(ShopCustomerType.WalkIn);
      this.form.controls.customerType.disable({ emitEvent: false });
      if (!this.form.controls.code.value) this.form.controls.code.setValue('WALK-IN');
      if (!this.form.controls.name.value) this.form.controls.name.setValue('Walk-in Customer');
    } else {
      this.form.controls.customerType.enable({ emitEvent: false });
      if (this.form.controls.customerType.value === ShopCustomerType.WalkIn) {
        this.form.controls.customerType.setValue(ShopCustomerType.Individual);
      }
    }
  }

  private patchForm(dto: ShopCustomerDto): void {
    this.form.patchValue({
      code: dto.code,
      name: dto.name,
      customerType: dto.customerType,
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
      isWalkInCustomer: dto.isWalkInCustomer,
      isActive: dto.isActive,
    });
    if (dto.isWalkInCustomer) this.form.controls.customerType.disable({ emitEvent: false });
    if (this.readonlyMode) this.form.disable({ emitEvent: false });
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

  private loadLedgerSummary(customerId: string): void {
    this.ledgerSummary = undefined;
    if (!this.canViewLedger) return;
    this.ledgerService.getBalanceSummary(customerId).subscribe({
      next: summary => (this.ledgerSummary = summary),
      error: () => (this.ledgerSummary = undefined),
    });
  }
}
