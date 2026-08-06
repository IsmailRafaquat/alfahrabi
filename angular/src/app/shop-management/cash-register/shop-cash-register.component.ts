import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { PermissionService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  ShopCashClosingDto,
  ShopCashDirection,
  ShopCashRegisterLookupDto,
  ShopCashRegisterService,
} from '../../proxy/shop-management/cash-registers';

@Component({ selector: 'app-shop-cash-register', standalone: false, templateUrl: './shop-cash-register.component.html', styleUrl: './shop-cash-register.component.scss' })
export class ShopCashRegisterComponent implements OnInit {
  private readonly service = inject(ShopCashRegisterService);
  private readonly fb = inject(FormBuilder);
  private readonly permissions = inject(PermissionService);
  private readonly toaster = inject(ToasterService);
  private readonly router = inject(Router);

  readonly canManageRegisters = this.permissions.getGrantedPolicy('ShopManagement.CashRegisters');
  readonly canOpen = this.permissions.getGrantedPolicy('ShopManagement.CashClosings.Open');
  readonly canClose = this.permissions.getGrantedPolicy('ShopManagement.CashClosings.Close');
  readonly canCancel = this.permissions.getGrantedPolicy('ShopManagement.CashClosings.Cancel');
  readonly canManualMovement = this.permissions.getGrantedPolicy('ShopManagement.CashTransactions.ManualMovement');

  loading = false;
  submitting = false;

  registers: ShopCashRegisterLookupDto[] = [];
  selectedRegisterId = '';
  openClosing: ShopCashClosingDto | null = null;
  checkedOpenState = false;

  openModalOpen = false;
  movementModalOpen = false;
  movementDirection: ShopCashDirection = ShopCashDirection.In;
  closeModalOpen = false;
  cancelModalOpen = false;

  readonly ShopCashDirection = ShopCashDirection;

  readonly openForm = this.fb.group({
    businessDate: [this.today(), Validators.required],
    openingCash: [0, [Validators.required, Validators.min(0)]],
    notes: [''],
  });

  readonly movementForm = this.fb.group({
    transactionDate: [this.today(), Validators.required],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    referenceNumber: [''],
    description: [''],
  });

  readonly closeForm = this.fb.group({
    actualClosingCash: [0, [Validators.required, Validators.min(0)]],
    notes: [''],
  });

  readonly cancelForm = this.fb.group({
    cancellationReason: ['', Validators.required],
  });

  ngOnInit(): void {
    this.loading = true;
    this.service.getLookup().pipe(finalize(() => (this.loading = false))).subscribe(result => {
      this.registers = result.items || [];
      const defaultRegister = this.registers.find(r => r.isDefault) || this.registers[0];
      if (defaultRegister) {
        this.selectedRegisterId = defaultRegister.id;
        this.loadOpenClosing();
      } else {
        this.checkedOpenState = true;
      }
    });
  }

  onRegisterChange(): void {
    this.loadOpenClosing();
  }

  loadOpenClosing(): void {
    if (!this.selectedRegisterId) return;
    this.checkedOpenState = false;
    this.service.getOpenClosing(this.selectedRegisterId).subscribe(result => {
      this.openClosing = result || null;
      this.checkedOpenState = true;
    });
  }

  get differencePreview(): number {
    const expected = this.openClosing?.expectedClosingCash ?? 0;
    const actual = this.closeForm.controls.actualClosingCash.value ?? 0;
    return Math.round((actual - expected) * 100) / 100;
  }

  openOpenModal(): void {
    this.openForm.reset({ businessDate: this.today(), openingCash: 0, notes: '' });
    this.openModalOpen = true;
  }

  submitOpen(): void {
    this.openForm.markAllAsTouched();
    if (this.openForm.invalid || this.submitting || !this.selectedRegisterId) return;

    this.submitting = true;
    const value = this.openForm.getRawValue();
    this.service
      .open(this.selectedRegisterId, {
        businessDate: value.businessDate!,
        openingCash: value.openingCash!,
        notes: value.notes || undefined,
      })
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: closing => {
          this.openClosing = closing;
          this.openModalOpen = false;
          this.toaster.success('::RegisterOpenedSuccessfully');
        },
        error: e => this.showError(e),
      });
  }

  openMovementModal(direction: ShopCashDirection): void {
    this.movementDirection = direction;
    this.movementForm.reset({ transactionDate: this.today(), amount: 0, referenceNumber: '', description: '' });
    this.movementModalOpen = true;
  }

  submitMovement(): void {
    this.movementForm.markAllAsTouched();
    if (this.movementForm.invalid || this.submitting || !this.selectedRegisterId) return;

    this.submitting = true;
    const value = this.movementForm.getRawValue();
    this.service
      .createManualMovement({
        cashRegisterId: this.selectedRegisterId,
        transactionDate: value.transactionDate!,
        direction: this.movementDirection,
        amount: value.amount!,
        referenceNumber: value.referenceNumber || undefined,
        description: value.description || undefined,
      })
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: () => {
          this.movementModalOpen = false;
          this.toaster.success('::CashMovementRecordedSuccessfully');
          this.loadOpenClosing();
        },
        error: e => this.showError(e),
      });
  }

  openCloseModal(): void {
    const expected = this.openClosing?.expectedClosingCash ?? 0;
    this.closeForm.reset({ actualClosingCash: Math.max(0, expected), notes: '' });
    this.closeModalOpen = true;
  }

  submitClose(): void {
    this.closeForm.markAllAsTouched();
    if (this.closeForm.invalid || this.submitting || !this.openClosing) return;

    this.submitting = true;
    const value = this.closeForm.getRawValue();
    this.service
      .close(this.openClosing.id, { actualClosingCash: value.actualClosingCash!, notes: value.notes || undefined })
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: () => {
          this.closeModalOpen = false;
          this.toaster.success('::RegisterClosedSuccessfully');
          this.loadOpenClosing();
        },
        error: e => this.showError(e),
      });
  }

  openCancelModal(): void {
    this.cancelForm.reset({ cancellationReason: '' });
    this.cancelModalOpen = true;
  }

  submitCancel(): void {
    this.cancelForm.markAllAsTouched();
    if (this.cancelForm.invalid || this.submitting || !this.openClosing) return;

    this.submitting = true;
    const value = this.cancelForm.getRawValue();
    this.service
      .cancelClosing(this.openClosing.id, { cancellationReason: value.cancellationReason! })
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: () => {
          this.cancelModalOpen = false;
          this.toaster.success('::ClosingCancelledSuccessfully');
          this.loadOpenClosing();
        },
        error: e => this.showError(e),
      });
  }

  manageRegisters(): void {
    this.router.navigate(['/shop-management/cash-registers']);
  }

  viewTransactions(): void {
    this.router.navigate(['/shop-management/cash-register/transactions']);
  }

  viewClosings(): void {
    this.router.navigate(['/shop-management/cash-register/closings']);
  }

  private today(): string {
    return new Date().toISOString().slice(0, 10);
  }

  private showError(error: any): void {
    this.toaster.error(error?.error?.error?.message || error?.message || '::UnexpectedError');
  }
}
