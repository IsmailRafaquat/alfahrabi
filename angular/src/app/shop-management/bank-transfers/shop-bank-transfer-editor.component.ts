import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs';
import {
  CreateUpdateShopBankTransferDto,
  ShopBankAccountLookupDto,
  ShopBankAccountService,
  ShopBankTransferService,
  ShopBankTransferStatus,
  ShopBankTransferType,
  shopBankTransferTypeOptions,
} from '../../proxy/shop-management/bank-accounts';
import { ShopCashRegisterLookupDto, ShopCashRegisterService } from '../../proxy/shop-management/cash-registers';

@Component({ selector: 'app-shop-bank-transfer-editor', standalone: false, templateUrl: './shop-bank-transfer-editor.component.html', styleUrl: './shop-bank-transfer-editor.component.scss' })
export class ShopBankTransferEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ShopBankTransferService);
  private readonly accountService = inject(ShopBankAccountService);
  private readonly cashRegisterService = inject(ShopCashRegisterService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);

  readonly ShopBankTransferType = ShopBankTransferType;
  readonly typeOptions = shopBankTransferTypeOptions;

  editId?: string;
  accounts: ShopBankAccountLookupDto[] = [];
  cashRegisters: ShopCashRegisterLookupDto[] = [];
  loading = false;
  submitting = false;

  get isEdit(): boolean {
    return !!this.editId;
  }

  get transferType(): ShopBankTransferType {
    return this.form.controls.transferType.value!;
  }

  sourceAccountBalance: number | null = null;

  readonly form = this.fb.group({
    transferDate: [this.today(), Validators.required],
    transferType: [ShopBankTransferType.CashToBank, Validators.required],
    fromBankAccountId: [''],
    toBankAccountId: [''],
    cashRegisterId: [''],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    referenceNumber: ['', Validators.maxLength(128)],
    notes: ['', Validators.maxLength(1000)],
  });

  ngOnInit(): void {
    this.accountService.getLookup().subscribe(result => (this.accounts = result.items || []));
    this.cashRegisterService.getLookup().subscribe(result => (this.cashRegisters = result.items || []));

    this.editId = this.route.snapshot.paramMap.get('id') || undefined;
    if (this.editId) {
      this.loadForEdit(this.editId);
      return;
    }

    const fromBankAccountId = this.route.snapshot.queryParamMap.get('fromBankAccountId');
    if (fromBankAccountId) {
      this.form.patchValue({ transferType: ShopBankTransferType.BankToCash, fromBankAccountId });
    }

    this.form.controls.fromBankAccountId.valueChanges.subscribe(id => this.loadSourceBalance(id));
    this.loadSourceBalance(this.form.controls.fromBankAccountId.value);
  }

  onTypeChange(): void {
    this.form.patchValue({ fromBankAccountId: '', toBankAccountId: '', cashRegisterId: '' });
  }

  private loadSourceBalance(bankAccountId: string | null | undefined): void {
    if (!bankAccountId) {
      this.sourceAccountBalance = null;
      return;
    }
    this.accountService.get(bankAccountId).subscribe(dto => (this.sourceAccountBalance = dto.currentBalance ?? null));
  }

  save(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting) return;

    this.submitting = true;
    const raw = this.form.getRawValue();
    const input: CreateUpdateShopBankTransferDto = {
      transferDate: raw.transferDate!,
      transferType: raw.transferType!,
      fromBankAccountId: raw.fromBankAccountId || undefined,
      toBankAccountId: raw.toBankAccountId || undefined,
      cashRegisterId: raw.cashRegisterId || undefined,
      amount: raw.amount!,
      referenceNumber: raw.referenceNumber || undefined,
      notes: raw.notes || undefined,
    };

    const request = this.editId ? this.service.update(this.editId, input) : this.service.create(input);
    request.pipe(finalize(() => (this.submitting = false))).subscribe({
      next: saved => {
        this.toaster.success(this.isEdit ? '::BankTransferUpdatedSuccessfully' : '::BankTransferCreatedSuccessfully');
        this.router.navigate(['/shop-management/bank-transfers', saved.id]);
      },
      error: e => this.toaster.error(e?.error?.error?.message || e?.message || '::UnexpectedError'),
    });
  }

  cancel(): void {
    if (this.editId) this.router.navigate(['/shop-management/bank-transfers', this.editId]);
    else this.router.navigate(['/shop-management/bank-transfers']);
  }

  private loadForEdit(id: string): void {
    this.loading = true;
    this.service
      .get(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(dto => {
        if (dto.status !== ShopBankTransferStatus.Draft) {
          this.toaster.error('::BankTransferCannotBeEdited');
          this.router.navigate(['/shop-management/bank-transfers', id]);
          return;
        }

        this.form.patchValue({
          transferDate: dto.transferDate ? dto.transferDate.substring(0, 10) : this.today(),
          transferType: dto.transferType,
          fromBankAccountId: dto.fromBankAccountId || '',
          toBankAccountId: dto.toBankAccountId || '',
          cashRegisterId: dto.cashRegisterId || '',
          amount: dto.amount ?? 0,
          referenceNumber: dto.referenceNumber || '',
          notes: dto.notes || '',
        });
      });
  }

  private today(): string {
    return new Date().toISOString().substring(0, 10);
  }
}
