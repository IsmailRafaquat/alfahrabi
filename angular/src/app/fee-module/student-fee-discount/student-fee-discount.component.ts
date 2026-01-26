import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, ToasterService, Confirmation } from '@abp/ng.theme.shared';
import { Component, inject, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { FeeHeadLookupDto, FeeHeadService } from 'src/app/proxy/fee-module/fee-heads';
import {
  StudentFeeDiscountDto,
  GetStudentFeeDiscountListInput,
  DiscountType,
  BulkAssignStudentFeeDiscountResultDto,
  BulkAssignStudentFeeDiscountDto,
  StudentFeeDiscountService,
  CreateUpdateStudentFeeDiscountDto,
  discountTypeOptions,
} from 'src/app/proxy/fee-module/student-fee-discounts';
import { StaffLookupDto, StaffService } from 'src/app/proxy/staffs';
import {
  StudentLookupDto,
  gradeLevelOptions,
  sectionOptions,
  shiftOptions,
  termOptions,
  StudentService,
} from 'src/app/proxy/students';

@Component({
  selector: 'app-student-fee-discount',
  standalone: false,
  templateUrl: './student-fee-discount.component.html',
  styleUrl: './student-fee-discount.component.scss',
  providers: [ListService],
})
export class StudentFeeDiscountComponent implements OnInit {
  discounts = { items: [], totalCount: 0 } as PagedResultDto<StudentFeeDiscountDto>;

  showFilter = false;

  isModalOpen = false;
  form!: FormGroup;
  selected = {} as StudentFeeDiscountDto;

  filters = {} as GetStudentFeeDiscountListInput;

  studentOptions: StudentLookupDto[] = [];
  feeHeadOptions: FeeHeadLookupDto[] = [];
  staffOptions: StaffLookupDto[] = [];

  discountTypes = discountTypeOptions;

  months = [
    { value: 1, label: '::Month.January' },
    { value: 2, label: '::Month.February' },
    { value: 3, label: '::Month.March' },
    { value: 4, label: '::Month.April' },
    { value: 5, label: '::Month.May' },
    { value: 6, label: '::Month.June' },
    { value: 7, label: '::Month.July' },
    { value: 8, label: '::Month.August' },
    { value: 9, label: '::Month.September' },
    { value: 10, label: '::Month.October' },
    { value: 11, label: '::Month.November' },
    { value: 12, label: '::Month.December' },
  ];

  // ---- BULK ASSIGN ----
  isBulkModalOpen = false;
  bulkForm!: FormGroup;

  gradeLevels = gradeLevelOptions;
  sections = sectionOptions;
  shifts = shiftOptions;
  terms = termOptions;

  bulkResult?: BulkAssignStudentFeeDiscountResultDto;
  selectedBulk = {} as BulkAssignStudentFeeDiscountDto;
  isBulkSubmitting = false;

  public readonly list = inject(ListService);
  private readonly service = inject(StudentFeeDiscountService);
  private readonly studentService = inject(StudentService);
  private readonly feeHeadService = inject(FeeHeadService);
  private readonly staffService = inject(StaffService);

  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);

  ngOnInit(): void {
    const streamCreator = (query: GetStudentFeeDiscountListInput) =>
      this.service.getList({ ...query, ...this.filters });

    this.list.hookToQuery(streamCreator).subscribe(res => (this.discounts = res));

    this.loadStudents();
    this.loadFeeHeads();
    this.loadStaff();
  }

  private loadStudents(): void {
    this.studentService.getStudentLookup().subscribe(res => {
      this.studentOptions = res || [];
    });
  }

  private loadFeeHeads(): void {
    this.feeHeadService.getFeeLookup().subscribe(res => {
      this.feeHeadOptions = res || [];
    });
  }

  private loadStaff(): void {
    this.staffService.getStaffLookup().subscribe(res => {
      this.staffOptions = res || [];
    });
  }

  create(): void {
    this.selected = {} as StudentFeeDiscountDto;
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
    const input = this.form.value as CreateUpdateStudentFeeDiscountDto;

    if (this.selected.id) {
      this.service.update(this.selected.id, input).subscribe(() => {
        this.toaster.success('::UpdatedSuccessfully');
        this.closeModalAndRefresh();
      });
    } else {
      this.service.create(input).subscribe(() => {
        this.toaster.success('::CreatedSuccessfully');
        this.closeModalAndRefresh();
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

  clearFilters(): void {
    this.filters = {} as GetStudentFeeDiscountListInput;
    this.list.get();
  }

  private buildForm(): void {
    this.form = this.fb.group({
      studentId: [this.selected.studentId || null, Validators.required],
      feeHeadId: [this.selected.feeHeadId || null], // nullable - applies to total if null
      discountType: [this.selected.discountType ?? DiscountType.Percent, [Validators.required]],
      value: [this.selected.value || 0, [Validators.required, Validators.min(0)]],
      startMonth: [this.selected.startMonth || null],
      endMonth: [this.selected.endMonth || null],
      reason: [this.selected.reason || '', [Validators.required, Validators.maxLength(500)]],
      approvedByStaffId: [this.selected.approvedByStaffId || null],
      isActive: [this.selected.id ? this.selected.isActive : true],
    });
  }

  private closeModalAndRefresh(): void {
    this.isModalOpen = false;
    this.form.reset();
    this.list.get();
  }

  // ---- UI helpers ----

  studentLabel(s: StudentLookupDto): string {
    const name = `${s.firstName ?? ''} ${s.lastName ?? ''}`.trim();
    return s.admissionNo ? `${name} (${s.admissionNo})` : name;
  }

  feeHeadLabel(f: FeeHeadLookupDto): string {
    return f.name ?? f.id;
  }

  staffLabel(s: StaffLookupDto): string {
    return `${s.firstName ?? ''} ${s.lastName ?? ''}`.trim() || s.id;
  }

  studentNameById(id: string): string {
    const s = this.studentOptions.find(x => x.id === id);
    return s ? this.studentLabel(s) : id;
  }

  feeHeadNameById(id: string | null): string {
    if (!id) return '::TotalFee' as any; // Will be localized
    const f = this.feeHeadOptions.find(x => x.id === id);
    return f ? this.feeHeadLabel(f) : id;
  }

  staffNameById(id: string | null): string {
    if (!id) return '-';
    const s = this.staffOptions.find(x => x.id === id);
    return s ? this.staffLabel(s) : id;
  }

  discountTypeLabel(type: DiscountType): string {
    return type === DiscountType.Fixed
      ? '::Enum:DiscountType.Fixed'
      : '::Enum:DiscountType.Percent';
  }

  monthLabel(month: number | null): string {
    if (!month) return '-';
    const m = this.months.find(x => x.value === month);
    return m ? m.label : month.toString();
  }

  openBulkAssign(): void {
    this.bulkResult = undefined;

    this.bulkForm = this.fb.group({
      gradeLevel: [this.selectedBulk.gradeLevel || null, Validators.required],
      section: [this.selectedBulk.section || null],
      shift: [this.selectedBulk.shift || null],
      term: [this.selectedBulk.term || null],

      feeHeadId: [this.selectedBulk.feeHeadId || null], // nullable
      discountType: [this.selectedBulk.discountType ?? DiscountType.Percent, [Validators.required]],
      value: [this.selectedBulk.value || 0, [Validators.required, Validators.min(0)]],

      startMonth: [this.selectedBulk.startMonth || null],
      endMonth: [this.selectedBulk.endMonth || null],

      reason: [this.selectedBulk.reason || '', [Validators.required, Validators.maxLength(500)]],
      approvedByStaffId: [this.selectedBulk.approvedByStaffId || null],

      isActive: [this.selectedBulk.isActive ?? true],
      skipExisting: [this.selectedBulk.skipExisting ?? true],
    });

    this.isBulkModalOpen = true;
  }

  closeBulkAssign(): void {
    this.isBulkModalOpen = false;
    this.bulkForm?.reset();
    this.bulkResult = undefined;
  }

  bulkAssign(): void {
    if (!this.bulkForm || this.bulkForm.invalid) return;

    this.isBulkSubmitting = true;
    this.bulkResult = undefined;

    const v = this.bulkForm.value;

    const input: BulkAssignStudentFeeDiscountDto = {
      gradeLevel: v.gradeLevel,
      section: v.section,
      shift: v.shift,
      term: v.term,

      feeHeadId: v.feeHeadId,
      discountType: v.discountType,
      value: v.value,

      startMonth: v.startMonth,
      endMonth: v.endMonth,

      reason: v.reason,
      approvedByStaffId: v.approvedByStaffId,

      isActive: !!v.isActive,
      skipExisting: !!v.skipExisting,
    };

    this.service.bulkAssign(input).subscribe({
      next: res => {
        this.bulkResult = res;
        this.toaster.success('::BulkAssignedSuccessfully');
        this.list.get(); // refresh table
      },
      error: err => {
        const msg =
          err?.error?.error?.message || err?.error?.message || err?.message || '::UnexpectedError';
        this.toaster.error(msg);
      },
      complete: () => (this.isBulkSubmitting = false),
    });
  }
}
