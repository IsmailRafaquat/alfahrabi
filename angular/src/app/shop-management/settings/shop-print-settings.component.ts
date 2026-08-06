import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { CreateUpdateShopPrintSettingsDto } from '../../proxy/shop-management/print-settings/models';
import {
  ShopPrintPaperSize,
  ShopPrintSettingsService,
  ShopThermalFontSize,
  ShopThermalPrintDensity,
} from '../../proxy/shop-management/print-settings';

@Component({
  selector: 'app-shop-print-settings',
  standalone: false,
  templateUrl: './shop-print-settings.component.html',
  styleUrl: './shop-print-settings.component.scss',
})
export class ShopPrintSettingsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopPrintSettingsService);
  private readonly toaster = inject(ToasterService);
  private readonly permissions = inject(PermissionService);

  loading = true;
  submitting = false;
  readonly canManage = this.permissions.getGrantedPolicy('ShopManagement.Print.ManageSettings');

  readonly ShopPrintPaperSize = ShopPrintPaperSize;
  readonly ShopThermalFontSize = ShopThermalFontSize;
  readonly ShopThermalPrintDensity = ShopThermalPrintDensity;

  readonly form = this.fb.group({
    defaultPrintPaperSize: [ShopPrintPaperSize.Thermal80Mm, Validators.required],
    printHeaderLogo: [false],
    printShopName: [true],
    printShopAddress: [true],
    printShopPhone: [true],
    printShopEmail: [false],
    printTaxNumber: [false],
    printFooterMessage: ['', Validators.maxLength(500)],
    printTermsAndConditions: ['', Validators.maxLength(2000)],
    printQrCode: [false],
    printBarcode: [false],
    printCustomerCopyLabel: ['Customer Copy', [Validators.required, Validators.maxLength(64)]],
    printDuplicateCopyLabel: ['Duplicate Copy', [Validators.required, Validators.maxLength(64)]],
    printItemCode: [false],
    printUnit: [true],
    printBatchNumber: [true],
    printExpiryDate: [true],
    printDiscount: [true],
    printTax: [true],
    printPaymentDetails: [true],
    printCashierName: [true],
    printDateTime: [true],
    printPageNumberForA4: [true],
    thermalFontSize: [ShopThermalFontSize.Medium, Validators.required],
    thermalPrintDensity: [ShopThermalPrintDensity.Normal, Validators.required],
    thermalAutoCut: [false],
    thermalOpenCashDrawer: [false],
  });

  ngOnInit(): void {
    this.service
      .get()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe((dto) => this.form.patchValue(dto));
  }

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting || !this.canManage) return;
    this.submitting = true;
    this.service
      .createOrUpdate(this.form.getRawValue() as CreateUpdateShopPrintSettingsDto)
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe((dto) => {
        this.form.patchValue(dto);
        this.toaster.success('::ShopPrintSettingsSavedSuccessfully');
      });
  }
}
