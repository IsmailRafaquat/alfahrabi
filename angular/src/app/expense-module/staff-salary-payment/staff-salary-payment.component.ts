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
import { ConfirmationHelperService } from 'src/app/shared/services/confirmation-helper.service';

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
  private readonly customConfirmation = inject(ConfirmationHelperService);
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
    this.filters.salaryMonth = this.monthUiToApiDate(this.salaryMonthUi) as any;
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

    const input: CreateUpdateStaffSalaryPaymentDto = {
      staffId: v.staffId,
      salaryMonth: this.monthUiToApiDate(v.salaryMonthUi) as any,
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
    this.customConfirmation.confirmDelete().subscribe(status => {
      if (status !== 'confirm') return;

      this.service.delete(id).subscribe(() => {
        this.list.get();
        this.toaster.success('::DeletedSuccessfully');
      });
    });
  }

  private buildForm(): void {
    const monthUi = this.apiDateToMonthUi(this.selected.salaryMonth);
    const payDate = this.apiDateToDateInput(this.selected.paymentDate);

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
        const salary = staff?.salary ?? 0;
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

  private monthUiToApiDate(value: string | null | undefined): string | undefined {
    if (!value) return undefined;

    // noon avoids timezone shifting to previous day/month
    return `${value}-01T12:00:00`;
  }

  private apiDateToMonthUi(value: string | Date | null | undefined): string {
    if (!value) {
      const now = new Date();
      return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
    }

    if (typeof value === 'string') {
      const match = value.match(/^(\d{4})-(\d{2})/);
      if (match) {
        return `${match[1]}-${match[2]}`;
      }
    }

    const d = new Date(value);
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
  }

  private apiDateToDateInput(value: string | Date | null | undefined): string {
    if (!value) {
      return this.formatDateInput(new Date());
    }

    if (typeof value === 'string') {
      const match = value.match(/^(\d{4})-(\d{2})-(\d{2})/);
      if (match) {
        return `${match[1]}-${match[2]}-${match[3]}`;
      }
    }

    return this.formatDateInput(new Date(value));
  }

  private formatDateInput(date: Date): string {
    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }
}
