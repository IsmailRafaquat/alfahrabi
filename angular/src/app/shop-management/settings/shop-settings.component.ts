import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { CreateUpdateShopSettingDto, ShopSettingService } from '../../proxy/shop-management/settings';

@Component({ selector: 'app-shop-settings', standalone: false, templateUrl: './shop-settings.component.html', styleUrl: './shop-settings.component.scss' })
export class ShopSettingsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopSettingService);
  private readonly toaster = inject(ToasterService);
  private readonly permissions = inject(PermissionService);
  loading = true;
  submitting = false;
  readonly canManage = this.permissions.getGrantedPolicy('ShopManagement.Settings.Manage');
  readonly requiredValidator = Validators.required;
  readonly form = this.fb.group({
    shopDisplayName: ['', [Validators.required, Validators.maxLength(256)]],
    logoFileId: [null as string | null], phone: ['', Validators.maxLength(32)], alternatePhone: ['', Validators.maxLength(32)],
    email: ['', [Validators.email, Validators.maxLength(256)]], website: ['', Validators.maxLength(256)],
    addressLine1: ['', Validators.maxLength(256)], addressLine2: ['', Validators.maxLength(256)],
    city: ['', Validators.maxLength(128)], stateOrProvince: ['', Validators.maxLength(128)], postalCode: ['', Validators.maxLength(32)], country: ['', Validators.maxLength(128)],
    currencyCode: ['PKR', [Validators.required, Validators.minLength(3), Validators.maxLength(3)]],
    currencySymbol: ['₨', [Validators.required, Validators.maxLength(8)]], taxNumber: ['', Validators.maxLength(64)],
    defaultTaxPercentage: [0, [Validators.required, Validators.min(0), Validators.max(100)]],
    invoicePrefix: ['INV', [Validators.required, Validators.maxLength(20)]], purchaseOrderPrefix: ['PO', [Validators.required, Validators.maxLength(20)]],
    receiptFooter: ['', Validators.maxLength(2000)], returnPolicy: ['', Validators.maxLength(2000)],
    allowNegativeStock: [false], autoGenerateProductBarcode: [true], autoGenerateInvoiceQrCode: [true],
    defaultLowStockLevel: [5, [Validators.required, Validators.min(0)]], decimalPlaces: [2, [Validators.required, Validators.min(0), Validators.max(4)]],
  });

  ngOnInit(): void {
    this.service.get().pipe(finalize(() => this.loading = false)).subscribe(dto => this.form.patchValue(dto));
  }

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting || !this.canManage) return;
    this.submitting = true;
    this.service.createOrUpdate(this.form.getRawValue() as CreateUpdateShopSettingDto)
      .pipe(finalize(() => this.submitting = false))
      .subscribe(dto => { this.form.patchValue(dto); this.toaster.success('::ShopSettingsSavedSuccessfully'); });
  }
}
