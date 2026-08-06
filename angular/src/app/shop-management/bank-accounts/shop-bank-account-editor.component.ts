import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import { CreateUpdateShopBankAccountDto, ShopBankAccountService } from '../../proxy/shop-management/bank-accounts';

@Component({ selector: 'app-shop-bank-account-editor', standalone: false, templateUrl: './shop-bank-account-editor.component.html', styleUrl: './shop-bank-account-editor.component.scss' })
export class ShopBankAccountEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopBankAccountService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);

  editId?: string;
  loading = false;
  submitting = false;

  get isEdit(): boolean {
    return !!this.editId;
  }

  readonly form = this.fb.group({
    code: ['', [Validators.required, Validators.maxLength(32)]],
    accountName: ['', [Validators.required, Validators.maxLength(200)]],
    bankName: ['', [Validators.required, Validators.maxLength(200)]],
    accountNumber: ['', Validators.maxLength(64)],
    iban: ['', Validators.maxLength(50)],
    branchName: ['', Validators.maxLength(200)],
    openingBalance: [0, [Validators.required, Validators.min(0)]],
    isDefault: [false],
    isActive: [true],
    notes: ['', Validators.maxLength(1000)],
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
    const input: CreateUpdateShopBankAccountDto = {
      code: raw.code!,
      accountName: raw.accountName!,
      bankName: raw.bankName!,
      accountNumber: raw.accountNumber || undefined,
      iban: raw.iban || undefined,
      branchName: raw.branchName || undefined,
      openingBalance: raw.openingBalance!,
      isDefault: raw.isDefault!,
      isActive: raw.isActive!,
      notes: raw.notes || undefined,
    };

    const request = this.editId ? this.service.update(this.editId, input) : this.service.create(input);
    request.pipe(finalize(() => (this.submitting = false))).subscribe({
      next: saved => {
        this.toaster.success(this.isEdit ? '::BankAccountUpdatedSuccessfully' : '::BankAccountCreatedSuccessfully');
        this.router.navigate(['/shop-management/bank-accounts', saved.id]);
      },
      error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
    });
  }

  cancel(): void {
    if (this.editId) this.router.navigate(['/shop-management/bank-accounts', this.editId]);
    else this.router.navigate(['/shop-management/bank-accounts']);
  }

  private loadForEdit(id: string): void {
    this.loading = true;
    this.service
      .get(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => {
        this.form.patchValue({
          code: dto.code,
          accountName: dto.accountName,
          bankName: dto.bankName,
          accountNumber: dto.accountNumber || '',
          iban: dto.iban || '',
          branchName: dto.branchName || '',
          openingBalance: dto.openingBalance ?? 0,
          isDefault: dto.isDefault,
          isActive: dto.isActive,
          notes: dto.notes || '',
        });
        // Opening balance can only be set at creation; once posted it is controlled entirely
        // through bank transactions, so editing it here would silently desync CurrentBalance.
        this.form.controls.openingBalance.disable();
      });
  }
}
