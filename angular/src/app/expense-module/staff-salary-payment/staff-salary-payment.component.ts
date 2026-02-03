import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, ToasterService, Confirmation } from '@abp/ng.theme.shared';
import { Component, inject, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import {
  StaffSalaryPaymentDto,
  GetStaffSalaryPaymentListInput,
  StaffSalaryPaymentService,
  CreateUpdateStaffSalaryPaymentDto,
} from 'src/app/proxy/expenses/staff-salary-payments';
import { StaffLookupDto, StaffService } from 'src/app/proxy/staffs';

@Component({
  selector: 'app-staff-salary-payment',
  standalone: false,
  templateUrl: './staff-salary-payment.component.html',
  styleUrl: './staff-salary-payment.component.scss',
  providers: [ListService],
})
export class StaffSalaryPaymentComponent implements OnInit {
  payments = { items: [], totalCount: 0 } as PagedResultDto<StaffSalaryPaymentDto>;

  showFilter = false;
  filters = {} as GetStaffSalaryPaymentListInput;

  // For <input type="month"> UI (yyyy-MM)
  salaryMonthUi: string | null = null;

  isModalOpen = false;
  form!: FormGroup;
  selected = {} as StaffSalaryPaymentDto;

  staffOptions: StaffLookupDto[] = [];

  public readonly list = inject(ListService);
  private readonly service = inject(StaffSalaryPaymentService);
  private readonly staffService = inject(StaffService);

  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);

  ngOnInit(): void {
    const streamCreator = (query: GetStaffSalaryPaymentListInput) =>
      this.service.getList({ ...query, ...this.filters });

    this.list.hookToQuery(streamCreator).subscribe(res => (this.payments = res));

    this.loadStaff();
  }

  private loadStaff(): void {
    this.staffService.getStaffLookup().subscribe(res => {
      this.staffOptions = res || [];
    });
  }

  // --- Filters ---
  applyFilters(): void {
    // Convert UI month "yyyy-MM" -> Date for API
    if (this.salaryMonthUi) {
      const [y, m] = this.salaryMonthUi.split('-').map(x => parseInt(x, 10));
      this.filters.salaryMonth = new Date(y, m - 1, 1) as any;
    } else {
      this.filters.salaryMonth = undefined as any;
    }

    this.list.get();
  }

  clearFilters(): void {
    this.filters = {} as GetStaffSalaryPaymentListInput;
    this.salaryMonthUi = null;
    this.list.get();
  }

  // --- CRUD ---
  create(): void {
    this.selected = {} as StaffSalaryPaymentDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  edit(id: string): void {
    this.service.get(id).subscribe(dto => {
      this.selected = dto;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  save(): void {
    if (this.form.invalid) return;

    const v = this.form.value;

    // Convert month ui string to Date
    const [y, m] = (v.salaryMonthUi as string).split('-').map((x: string) => parseInt(x, 10));
    const input: CreateUpdateStaffSalaryPaymentDto = {
      staffId: v.staffId,
      salaryMonth: new Date(y, m - 1, 1) as any,
      salaryAmount: v.salaryAmount,
      paymentDate: v.paymentDate,
      remarks: v.remarks,
    };

    if (this.selected.id) {
      this.service.update(this.selected.id, input).subscribe(() => {
        this.toaster.success('::UpdatedSuccessfully');
        this.close();
      });
    } else {
      this.service.create(input).subscribe(() => {
        this.toaster.success('::CreatedSuccessfully');
        this.close();
      });
    }
  }

  delete(id: string): void {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.service.delete(id).subscribe(() => {
          this.toaster.success('::DeletedSuccessfully');
          this.list.get();
        });
      }
    });
  }

  private buildForm(): void {
    const month = this.selected.salaryMonth
      ? new Date(this.selected.salaryMonth as any)
      : new Date();
    const monthUi = `${month.getFullYear()}-${String(month.getMonth() + 1).padStart(2, '0')}`;
    const payDate = this.selected.paymentDate
      ? new Date(this.selected.paymentDate as any).toISOString().substring(0, 10)
      : new Date().toISOString().substring(0, 10);

    this.form = this.fb.group({
      staffId: [this.selected.staffId || null, Validators.required],
      salaryMonthUi: [monthUi, Validators.required],
      paymentDate: [payDate, Validators.required],
      salaryAmount: [this.selected.salaryAmount || 0, [Validators.required, Validators.min(0.01)]],
      remarks: [this.selected.remarks || null],
    });

    this.form.get('staffId')!.valueChanges.subscribe((staffId: string) => {
      if (!staffId) return;

      this.staffService.get(staffId).subscribe(staff => {
        // staff.salary is decimal? in backend
        const salary = staff?.salary ?? 0;

        // only auto-fill if amount is empty or zero (so user manual override is respected)
        const currentAmount = this.form.get('salaryAmount')!.value;
        if (!currentAmount || Number(currentAmount) === 0) {
          this.form.get('salaryAmount')!.setValue(salary);
        }
      });
    });
  }

  private close(): void {
    this.isModalOpen = false;
    this.form.reset();
    this.list.get();
  }

  // --- helpers ---
  staffLabel(s: StaffLookupDto): string {
    const name = `${s.firstName ?? ''} ${s.lastName ?? ''}`.trim();
    return name || s.id;
  }
}
