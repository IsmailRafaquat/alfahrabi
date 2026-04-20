import { ListService, LocalizationService, PagedResultDto } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  StudentMonthlyFeeLineDto,
  GetStudentMonthlyFeeLineListInput,
  StudentMonthlyFeeLineService,
  CreateUpdateStudentMonthlyFeeLineDto,
  CalculateFeeLineAmountsInput,
  CalculatedAmountsDto,
} from 'src/app/proxy/fee-module/student-monthly-fee-lines';
import {
  StudentMonthlyFeeDto,
  StudentMonthlyFeeService,
} from 'src/app/proxy/fee-module/student-monthly-fees';
import { FeeHeadDto, FeeHeadService } from 'src/app/proxy/fee-module/fee-heads';
import {
  gradeLevelOptions,
  sectionOptions,
  StudentLookupDto,
  StudentService,
} from 'src/app/proxy/students';
import { ConfirmationHelperService } from 'src/app/shared/services/confirmation-helper.service';
import { StudentRecentFeeHistoryDto } from 'src/app/proxy/fee-module/student-recent-fee-history/models';

@Component({
  selector: 'app-student-monthly-fee-line',
  standalone: false,
  templateUrl: './student-monthly-fee-line.component.html',
  styleUrl: './student-monthly-fee-line.component.scss',
  providers: [ListService],
})
export class StudentMonthlyFeeLineComponent implements OnInit {
  feeLines = { items: [], totalCount: 0 } as PagedResultDto<StudentMonthlyFeeLineDto>;

  showFilter = false;

  isModalOpen = false;
  form!: FormGroup;
  selected = {} as StudentMonthlyFeeLineDto;

  filters = {} as GetStudentMonthlyFeeLineListInput;

  // Lookup options
  studentOptions: StudentLookupDto[] = [];
  monthlyFeeOptions: StudentMonthlyFeeDto[] = [];
  feeHeadOptions: FeeHeadDto[] = [];

  section = sectionOptions;
  class = gradeLevelOptions;
  // Auto-calculation state
  isCalculating = false;
  calculatedAmounts: CalculatedAmountsDto | null = null;

  //History student
  recentHistory: StudentRecentFeeHistoryDto | null = null;
  historyLoading = false;
  historyPanelActive = true;

  public readonly list = inject(ListService);
  private readonly service = inject(StudentMonthlyFeeLineService);
  private readonly studentService = inject(StudentService);
  private readonly monthlyFeeService = inject(StudentMonthlyFeeService);
  private readonly feeHeadService = inject(FeeHeadService);

  private readonly fb = inject(FormBuilder);
  private readonly customConfirmation = inject(ConfirmationHelperService);
  private readonly toaster = inject(ToasterService);
  private readonly localizationService = inject(LocalizationService);

  ngOnInit(): void {
    const streamCreator = (query: GetStudentMonthlyFeeLineListInput) => {
      return this.service.getList({ ...query, ...this.filters });
    };

    this.list.hookToQuery(streamCreator).subscribe(res => (this.feeLines = res));

    this.loadLookups();
  }

  private loadLookups(): void {
    // Load students for display purposes
    this.studentService.getStudentLookup().subscribe(res => {
      this.studentOptions = res || [];
    });

    // Load monthly fees
    this.monthlyFeeService.getList({ maxResultCount: 1000, skipCount: 0 } as any).subscribe(res => {
      this.monthlyFeeOptions = res.items || [];
    });

    // Load fee heads
    this.feeHeadService.getList({ maxResultCount: 1000, skipCount: 0 } as any).subscribe(res => {
      this.feeHeadOptions = res.items || [];
    });
  }

  clearFilters(): void {
    this.filters = {
      studentMonthlyFeeId: null,
      feeHeadId: null,
      collectedOn: null,
    } as any;

    this.list.get();
  }

  create(): void {
    this.selected = {} as StudentMonthlyFeeLineDto;
    this.calculatedAmounts = null;
    this.resetRecentHistory();
    this.buildForm();
    this.setupFormListeners();
    this.isModalOpen = true;
  }

  edit(id: string): void {
    this.service.get(id).subscribe(dto => {
      this.selected = dto;
      this.calculatedAmounts = null;
      this.buildForm();
      this.setupFormListeners();
      this.isModalOpen = true;
      if (this.selected.studentMonthlyFeeId) {
        this.loadRecentHistory();
      }
    });
  }

  private setupFormListeners(): void {
    this.form.get('month')?.valueChanges.subscribe(() => this.onMonthOrClassChanged());
    this.form.get('gradeLevel')?.valueChanges.subscribe(() => this.onMonthOrClassChanged());

    this.form.get('studentMonthlyFeeId')?.valueChanges.subscribe(() => {
      this.autoCalculateAmounts();
      this.loadRecentHistory();
    });
    this.form.get('feeHeadId')?.valueChanges.subscribe(() => this.autoCalculateAmounts());
  }

  private onMonthOrClassChanged(): void {
    const list = this.filteredMonthlyFeeOptions;
    this.resetRecentHistory();

    // clear monthly fee when month/class changes
    this.form.get('studentMonthlyFeeId')?.setValue(null, { emitEvent: false });

    // auto-select if exactly 1 exists
    if (list.length === 1) {
      this.form.get('studentMonthlyFeeId')?.setValue(list[0].id, { emitEvent: true });
    }
  }

  enableExpectedAmountOverride(): void {
    this.form.get('expectedAmount')?.enable();
  }

  private autoCalculateAmounts(): void {
    const monthlyFeeId = this.form.get('studentMonthlyFeeId')?.value;
    const feeHeadId = this.form.get('feeHeadId')?.value;

    // Only need monthlyFeeId and feeHeadId now
    if (!monthlyFeeId || !feeHeadId) {
      return;
    }

    const input: CalculateFeeLineAmountsInput = {
      studentMonthlyFeeId: monthlyFeeId,
      feeHeadId: feeHeadId,
      // expectedAmount removed - will come from FeeStructureItem
    };

    this.isCalculating = true;

    this.service.calculateAmounts(input).subscribe({
      next: result => {
        this.calculatedAmounts = result;

        // Auto-populate ALL amounts from the calculation
        this.form.patchValue(
          {
            expectedAmount: result.expectedAmount, // ← Now from FeeStructureItem
            discountAmount: result.discountAmount,
            lateFeeAmount: result.lateFeeAmount,
          },
          { emitEvent: false },
        );

        this.isCalculating = false;
      },
      error: err => {
        console.error('Failed to calculate amounts:', err);
        this.isCalculating = false;
      },
    });
  }

  save(): void {
    if (this.form.invalid) return;

    const v = this.form.getRawValue();

    const input: CreateUpdateStudentMonthlyFeeLineDto = {
      studentMonthlyFeeId: v.studentMonthlyFeeId,
      feeHeadId: v.feeHeadId,
      expectedAmount: v.expectedAmount ?? 0,
      discountAmount: v.discountAmount ?? 0,
      adjustmentAmount: v.adjustmentAmount ?? 0,
      lateFeeAmount: v.lateFeeAmount ?? 0,
      paidAmount: v.paidAmount ?? 0,
    };

    if (this.selected.id) {
      this.service.update(this.selected.id, input).subscribe({
        next: () => {
          this.toaster.success('::UpdatedSuccessfully');
          this.closeModalAndRefresh();
        },
        error: err => this.handleError(err),
      });
    } else {
      this.service.create(input).subscribe({
        next: () => {
          this.toaster.success('::CreatedSuccessfully');
          this.closeModalAndRefresh();
        },
        error: err => this.handleError(err),
      });
    }
  }

  delete(id: string): void {
    this.customConfirmation.confirmDelete().subscribe(status => {
      if (status !== 'confirm') return;

      this.service.delete(id).subscribe(() => {
        this.toaster.success('::DeletedSuccessfully');
        this.list.get();
      });
    });
  }

  private buildForm(): void {
    const existingFee = this.monthlyFeeOptions.find(
      x => x.id === this.selected.studentMonthlyFeeId,
    );

    // If editing, infer gradeLevel from the selected monthly fee's student (if possible)
    const existingStudentId = existingFee?.studentId ?? null;
    const existingStudent = existingStudentId
      ? this.studentOptions.find(s => s.id === existingStudentId)
      : null;

    this.form = this.fb.group({
      month: [existingFee?.month ? this.toMonthKey(existingFee.month) : null, Validators.required],

      gradeLevel: [(existingStudent as any)?.gradeLevel ?? null], // <-- add this (adjust property name if different)

      studentMonthlyFeeId: [this.selected.studentMonthlyFeeId || null, Validators.required],
      feeHeadId: [this.selected.feeHeadId || null, Validators.required],

      expectedAmount: [
        { value: this.selected.expectedAmount || 0, disabled: true },
        [Validators.required, Validators.min(0)],
      ],
      discountAmount: [
        { value: this.selected.discountAmount || 0, disabled: true },
        Validators.min(0),
      ],
      adjustmentAmount: [this.selected.adjustmentAmount || 0],
      lateFeeAmount: [
        { value: this.selected.lateFeeAmount || 0, disabled: true },
        Validators.min(0),
      ],
      paidAmount: [this.selected.paidAmount || 0, Validators.min(0)],
    });
  }

  // Allow manual override of discount
  enableDiscountOverride(): void {
    this.form.get('discountAmount')?.enable();
  }

  // Allow manual override of late fee
  enableLateFeeOverride(): void {
    this.form.get('lateFeeAmount')?.enable();
  }

  private closeModalAndRefresh(): void {
    this.isModalOpen = false;
    this.form.reset();
    this.calculatedAmounts = null;
    this.resetRecentHistory();
    this.list.get();
  }

  // -------- Helper methods --------

  studentLabel(s: StudentLookupDto): string {
    const name = `${s.firstName ?? ''} ${s.lastName ?? ''}`.trim();
    return s.admissionNo ? `${name} (${s.admissionNo})` : name;
  }

  studentNameById(id: string): string {
    const s = this.studentOptions.find(x => x.id === id);
    return s ? this.studentLabel(s) : id;
  }

  monthlyFeeLabel(fee: StudentMonthlyFeeDto): string {
    if (!fee) return '';

    const studentName = fee.studentName || fee.studentName || '';
    const admissionNo = fee.admissionNo ? ` (${fee.admissionNo})` : '';
    const className =
      fee.gradeLevel != null
        ? ` - ${this.localizationService.instant('::Enum:GradeLevel.' + fee.gradeLevel)}`
        : '';
    const month = fee.month ? ` - ${this.formatMonthLabel(fee.month)}` : '';

    return `${studentName}${admissionNo}${className}${month}`;
  }
  getGradeLevelLabel(value: number): string {
    return this.localizationService.instant(`::Enum:GradeLevel.${value}`);
  }

  monthlyFeeById(id: string): string {
    const fee = this.monthlyFeeOptions.find(x => x.id === id);
    return fee ? this.monthlyFeeLabel(fee) : id;
  }

  feeHeadNameById(id: string): string {
    const feeHead = this.feeHeadOptions.find(x => x.id === id);
    return feeHead?.name || id;
  }

  calculateNetAmount(line: StudentMonthlyFeeLineDto): number {
    return (
      (line.expectedAmount || 0) -
      (line.discountAmount || 0) +
      (line.adjustmentAmount || 0) +
      (line.lateFeeAmount || 0)
    );
  }

  // Calculate net amount from form values
  calculateFormNetAmount(): number {
    if (!this.form) return 0;

    return (
      (this.form.get('expectedAmount')?.value || 0) -
      (this.form.get('discountAmount')?.value || 0) +
      (this.form.get('adjustmentAmount')?.value || 0) +
      (this.form.get('lateFeeAmount')?.value || 0)
    );
  }

  calculateBalance(line: StudentMonthlyFeeLineDto): number {
    return this.calculateNetAmount(line) - (line.paidAmount || 0);
  }

  private handleError(err: any): void {
    const msg =
      err?.error?.error?.message || err?.error?.message || err?.message || '::UnexpectedError';
    this.toaster.error(msg);
  }

  private toMonthKey(value: any): string {
    const d = value instanceof Date ? value : new Date(value);
    if (Number.isNaN(d.getTime())) return '';
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
  }

  get filteredMonthlyFeeOptions(): StudentMonthlyFeeDto[] {
    const m = this.form?.get('month')?.value;
    if (!m) return [];

    const mk = this.toMonthKey(m);

    const gradeLevel = this.form?.get('gradeLevel')?.value;

    // 1) month filter
    let list = (this.monthlyFeeOptions || []).filter(x => this.toMonthKey(x.month) === mk);

    // 2) class filter (optional)
    if (gradeLevel != null) {
      const allowedStudentIds = new Set(
        (this.studentOptions || [])
          .filter(s => (s as any).gradeLevel === gradeLevel) // adjust if property name differs
          .map(s => s.id),
      );

      list = list.filter(f => allowedStudentIds.has(f.studentId));
    }

    return list;
  }
  private resetRecentHistory(): void {
    this.recentHistory = null;
    this.historyLoading = false;
    this.historyPanelActive = true;
  }

  private loadRecentHistory(): void {
    const studentMonthlyFeeId = this.form?.get('studentMonthlyFeeId')?.value;
    if (!studentMonthlyFeeId) {
      this.resetRecentHistory();
      return;
    }
    this.historyLoading = true;
    this.service.getRecentHistory(studentMonthlyFeeId, 6).subscribe({
      next: res => {
        this.recentHistory = res;
        this.historyLoading = false;
      },
      error: err => {
        this.historyLoading = false;
        this.recentHistory = null;
        this.handleError(err);
      },
    });
  }

  monthLabel(value: string | Date): string {
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return '-';
    return d.toLocaleString('en-US', {
      month: 'long',
      year: 'numeric',
    });
  }

  formatMonthLabel(value: string | Date): string {
    if (!value) return '';

    const date = new Date(value);
    if (isNaN(date.getTime())) {
      return String(value).substring(0, 7);
    }

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    return `${year}-${month}`;
  }
}
