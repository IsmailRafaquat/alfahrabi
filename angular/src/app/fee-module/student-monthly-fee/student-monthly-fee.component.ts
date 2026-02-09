import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  StudentMonthlyFeeDto,
  GetStudentMonthlyFeeListInput,
  StudentMonthlyFeeService,
  CreateUpdateStudentMonthlyFeeDto,
  BulkGenerateStudentMonthlyFeeResultDto,
  BulkGenerateStudentMonthlyFeeDto,
} from 'src/app/proxy/fee-module/student-monthly-fees';
import {
  gradeLevelOptions,
  sectionOptions,
  shiftOptions,
  StudentLookupDto,
  StudentService,
  termOptions,
} from 'src/app/proxy/students';
import { ConfirmationHelperService } from 'src/app/shared/services/confirmation-helper.service';

@Component({
  selector: 'app-student-monthly-fee',
  standalone: false,
  templateUrl: './student-monthly-fee.component.html',
  styleUrl: './student-monthly-fee.component.scss',
  providers: [ListService],
})
export class StudentMonthlyFeeComponent implements OnInit {
  fees = { items: [], totalCount: 0 } as PagedResultDto<StudentMonthlyFeeDto>;

  showFilter = false;

  isModalOpen = false;
  form!: FormGroup;
  selected = {} as StudentMonthlyFeeDto;

  // Keep these as strings for inputs (type="date"/"month")
  filters = {} as GetStudentMonthlyFeeListInput;

  studentOptions: StudentLookupDto[] = [];

  isBulkModalOpen = false;
  bulkForm!: FormGroup;
  bulkLoading = false;

  bulkResult: BulkGenerateStudentMonthlyFeeResultDto | null = null;

  // dropdown option arrays
  gradeLevels = gradeLevelOptions;
  sections = sectionOptions;
  shifts = shiftOptions;
  terms = termOptions;

  // because your backend bulk dto uses enum fields, keep them outside reactive form
  bulkFilters: any = {
    gradeLevel: null,
    section: null,
    shift: null,
    term: null,
  };

  public readonly list = inject(ListService);
  private readonly service = inject(StudentMonthlyFeeService);
  private readonly studentService = inject(StudentService);

  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);
  private readonly customConfirmation = inject(ConfirmationHelperService);
  private readonly toaster = inject(ToasterService);

  ngOnInit(): void {
    const streamCreator = (query: GetStudentMonthlyFeeListInput) => {
      // Convert UI filter strings -> backend expected types.
      const normalized = this.normalizeFilters();
      return this.service.getList({ ...query, ...normalized });
    };

    this.list.hookToQuery(streamCreator).subscribe(res => (this.fees = res));

    this.loadStudents();
  }

  private loadStudents(): void {
    this.studentService.getStudentLookup().subscribe(res => {
      this.studentOptions = res || [];
    });
  }

  clearFilters(): void {
    this.filters = {
      filter: null,
      studentId: null,
      month: null,
      dueDateFrom: null,
      dueDateTo: null,
    } as any;

    this.list.get();
  }

  create(): void {
    this.selected = {} as StudentMonthlyFeeDto;
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

    const raw = this.form.value as any;

    const input: CreateUpdateStudentMonthlyFeeDto = {
      studentId: raw.studentId,
      // input[type=month] gives "YYYY-MM" -> store first day
      month: this.monthToFirstDayIso(raw.month),
      dueDate: raw.dueDate ? this.dateOnlyIso(raw.dueDate) : null,
      remarks: raw.remarks ?? null,
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
        this.list.get();
        this.toaster.success('::DeletedSuccessfully');
      });
    });
  }

  private buildForm(): void {
    this.form = this.fb.group({
      studentId: [this.selected.studentId || null, Validators.required],

      // backend month is DateTime, convert to "YYYY-MM"
      month: [
        this.toMonthInput(this.selected.month) || this.toMonthInput(new Date()),
        Validators.required,
      ],

      // dueDate is optional
      dueDate: [this.toDateInput(this.selected.dueDate)],
      remarks: [this.selected.remarks || null],
    });
  }

  private closeModalAndRefresh(): void {
    this.isModalOpen = false;
    this.form.reset();
    this.list.get();
  }

  // -------- helpers (labels + conversions) --------

  studentLabel(s: StudentLookupDto): string {
    const name = `${s.firstName ?? ''} ${s.lastName ?? ''}`.trim();
    return s.admissionNo ? `${name} (${s.admissionNo})` : name;
  }

  studentNameById(id: string): string {
    const s = this.studentOptions.find(x => x.id === id);
    return s ? this.studentLabel(s) : id;
  }

  private normalizeFilters(): GetStudentMonthlyFeeListInput {
    // Map UI strings -> ISO date strings your backend expects (or DateTime, depending on proxy)
    // Most ABP Angular proxies send Date as string ISO anyway.
    return {
      ...(this.filters as any),
      month: this.filters.month ? this.monthToFirstDayIso(this.filters.month) : null,
      // dueDateFrom: this.filters. ? this.dateOnlyIso(this.filters.dueDateFrom) : null,
      // dueDateTo: this.filters.dueDateTo ? this.dateOnlyIso(this.filters.dueDateTo) : null,
    };
  }

  private toDateInput(value: any): string | null {
    if (!value) return null;
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return null;
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  private toMonthInput(value: any): string | null {
    if (!value) return null;
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return null;
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    return `${yyyy}-${mm}`;
  }

  private monthToFirstDayIso(month: string): string {
    // month: "YYYY-MM" -> "YYYY-MM-01T00:00:00.000Z" (or without Z depending on server handling)
    const [y, m] = month.split('-').map(x => parseInt(x, 10));
    const d = new Date(Date.UTC(y, (m || 1) - 1, 1));
    return d.toISOString();
  }

  private dateOnlyIso(dateStr: string): string {
    // "YYYY-MM-DD" -> ISO at UTC midnight
    const [y, m, d] = dateStr.split('-').map(x => parseInt(x, 10));
    const dt = new Date(Date.UTC(y, (m || 1) - 1, d || 1));
    return dt.toISOString();
  }

  private handleError(err: any): void {
    const msg =
      err?.error?.error?.message || err?.error?.message || err?.message || '::UnexpectedError';
    this.toaster.error(msg);
  }

  openBulkModal(): void {
    this.bulkResult = null;
    this.bulkFilters = { gradeLevel: null, section: null, shift: null, term: null };

    this.bulkForm = this.fb.group({
      month: [this.toMonthInput(new Date()), Validators.required], // "YYYY-MM"
      dueDate: [null],
      remarks: [null],
      skipIfExists: [true],
    });

    this.isBulkModalOpen = true;
  }

  bulkGenerate(): void {
    if (this.bulkForm.invalid) return;

    const raw = this.bulkForm.value;

    const input: BulkGenerateStudentMonthlyFeeDto = {
      month: this.monthToFirstDayIso(raw.month),
      dueDate: raw.dueDate ? this.dateOnlyIso(raw.dueDate) : null,
      remarks: raw.remarks ?? null,
      skipIfExists: !!raw.skipIfExists,

      gradeLevel: this.bulkFilters.gradeLevel ?? null,
      section: this.bulkFilters.section ?? null,
      shift: this.bulkFilters.shift ?? null,
      term: this.bulkFilters.term ?? null,
    };

    this.bulkLoading = true;

    this.service.bulkGenerate(input).subscribe({
      next: res => {
        this.bulkResult = res;
        this.toaster.success('::BulkGenerateCompleted');
        // refresh table to show newly created fees
        this.list.get();
      },
      error: err => this.handleError(err),
      complete: () => (this.bulkLoading = false),
    });
  }
}
