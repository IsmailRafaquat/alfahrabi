import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, ToasterService, Confirmation } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { attendanceStatusOptions, AttendanceStatus } from '../proxy/attendance-statuss';
import {
  StudentAttendanceDto,
  GetStudentAttendanceListDto,
  StudentAttendanceService,
  MarkStudentAttendanceDto,
} from '../proxy/student-attendances';
import {
  StudentService,
  StudentLookupDto,
  gradeLevelOptions,
  sectionOptions,
  Section,
} from '../proxy/students';
import { HttpClient } from '@angular/common/http';
import { StudentAttendanceDownloadService } from 'src/custom-services/student-attendance-template/student-attendance-download-service';
import {
  ImportStudentAttendanceResultDto,
  StudentAttendanceImportApi,
} from 'src/custom-services/import-student-attendance';

@Component({
  selector: 'app-student-attendance',
  standalone: false,
  templateUrl: './student-attendance.component.html',
  styleUrl: './student-attendance.component.scss',
  providers: [ListService],
})
export class StudentAttendanceComponent implements OnInit {
  attendances = { items: [], totalCount: 0 } as PagedResultDto<StudentAttendanceDto>;

  form!: FormGroup;
  templateForm!: FormGroup;
  isModalOpen = false;
  isViewModalOpen = false;
  isTemplateModalOpen = false;
  showFilter = false;

  selectedAttendance = {} as StudentAttendanceDto;
  filters = {} as GetStudentAttendanceListDto;

  attendanceStatuses = attendanceStatusOptions;

  attendanceStatusPresent = AttendanceStatus.Present;
  attendanceStatusAbsent = AttendanceStatus.Absent;
  attendanceStatusLate = AttendanceStatus.Late;

  gradeLevels = gradeLevelOptions;
  sections = sectionOptions;

  selectedExcelFile: File | null = null;
  importingExcel = false;
  importResult: ImportStudentAttendanceResultDto | null = null;
  // Student lookup for modal
  studentOptions: StudentLookupDto[] = [];
  studentsLoading = false;

  constructor(
    public readonly list: ListService,
    private attendanceService: StudentAttendanceService,
    private importApi: StudentAttendanceImportApi,
    private downloadService: StudentAttendanceDownloadService,
    private studentService: StudentService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
  ) {
    this.templateForm = this.fb.group({
      gradeLevel: [null, Validators.required],
      section: [null, Validators.required],
      dateFrom: [null, Validators.required],
      dateTo: [null, Validators.required],
    });
  }

  ngOnInit(): void {
    const streamCreator = (query: any) =>
      this.attendanceService.getList({ ...query, ...this.filters });

    this.list.hookToQuery(streamCreator).subscribe(res => (this.attendances = res));
    this.getStudents();
  }

  getStudents() {
    this.studentService.getStudentLookup().subscribe(res => {
      this.studentOptions = res;
    });
  }

  private buildForm(): void {
    const today = new Date();
    const yyyyMmDd = new Date(today.getFullYear(), today.getMonth(), today.getDate())
      .toISOString()
      .split('T')[0];

    this.form = this.fb.group({
      studentId: [this.selectedAttendance.studentId ?? null, Validators.required],
      attendanceDate: [
        this.toDateOnly(this.selectedAttendance.attendanceDate) ?? yyyyMmDd,
        Validators.required,
      ],
      status: [this.selectedAttendance.status ?? AttendanceStatus.Present, Validators.required],
      remarks: [this.selectedAttendance.remarks ?? null],
    });
  }

  openMarkModal(): void {
    this.selectedAttendance = {} as StudentAttendanceDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  editAttendance(id: string): void {
    this.attendanceService.get(id).subscribe(a => {
      this.selectedAttendance = a;

      const student = (a as any).student;
      if (student?.id) {
        const label = `${student.firstName} ${student.lastName} (${student.admissionNo})`;
        this.studentOptions = [{ id: student.id, firstName: label }];
      }

      this.buildForm();
      this.isModalOpen = true;
    });
  }

  save(): void {
    if (this.form.invalid) return;

    const raw = this.form.getRawValue();

    const dto: MarkStudentAttendanceDto = {
      studentId: raw.studentId,
      attendanceDate: raw.attendanceDate, // yyyy-MM-dd
      status: raw.status,
      remarks: raw.remarks,
    } as any;

    this.attendanceService.mark(dto).subscribe(() => {
      this.isModalOpen = false;
      this.form.reset();
      this.list.get();
      this.toaster.success('::SavedSuccessfully');
    });
  }

  delete(id: string): void {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.attendanceService.delete(id).subscribe(() => {
          this.list.get();
          this.toaster.success('::SuccessfullyDeleted');
        });
      }
    });
  }

  viewDetails(id: string): void {
    this.attendanceService.get(id).subscribe(a => {
      this.selectedAttendance = a;
      this.isViewModalOpen = true;
    });
  }

  clearFilters(): void {
    this.filters = {} as GetStudentAttendanceListDto;
    this.list.get();
  }

  private toDateOnly(value: any): string | null {
    if (!value) return null;
    const s = String(value);
    return s.includes('T') ? s.split('T')[0] : s;
  }

  openTemplateModal(): void {
    const today = new Date();
    const yyyyMmDd = new Date(today.getFullYear(), today.getMonth(), today.getDate())
      .toISOString()
      .split('T')[0];

    this.templateForm.reset({
      gradeLevel: null,
      section: null,
      dateFrom: yyyyMmDd,
      dateTo: yyyyMmDd,
    });

    this.isTemplateModalOpen = true;
  }

  private getSectionName(value: number | null | undefined): string {
    if (value === null || value === undefined) return '';
    return Section[value] ?? String(value);
  }

  generateTemplate(): void {
    if (this.templateForm.invalid) {
      this.templateForm.markAllAsTouched();
      return;
    }

    const input = this.templateForm.getRawValue();

    this.downloadService.downloadTemplate(input).subscribe({
      next: (blob: Blob) => {
        const secrionName = this.getSectionName(input.section);
        const fileName = `Attendance_${input.gradeLevel}_${secrionName}_${input.dateFrom}_${input.dateTo}.xlsx`;
        this.downloadBlob(blob, fileName);
        this.toaster.success('::TemplateGenerated');
        this.isTemplateModalOpen = false;
      },
      error: err => {
        const msg =
          err?.error?.error?.message ?? err?.error?.message ?? '::TemplateGenerationFailed';
        this.toaster.error(msg);
      },
    });
  }

  private downloadBlob(blob: Blob, fileName: string): void {
    const a = document.createElement('a');
    const objectUrl = URL.createObjectURL(blob);

    a.href = objectUrl;
    a.download = fileName;
    a.click();

    URL.revokeObjectURL(objectUrl);
  }

  private getFileNameFromContentDisposition(cd: string | null): string | null {
    if (!cd) return null;

    // Handles: filename="abc.xlsx" or filename*=UTF-8''abc.xlsx
    const utf8Match = /filename\*\=UTF-8''([^;]+)/i.exec(cd);
    if (utf8Match?.[1]) return decodeURIComponent(utf8Match[1].replace(/"/g, ''));

    const match = /filename\=([^;]+)/i.exec(cd);
    if (!match?.[1]) return null;

    return match[1].trim().replace(/"/g, '');
  }



  onExcelSelectedAndImport(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    input.value = '';

    if (!file) return;

    if (!file.name.toLowerCase().endsWith('.xlsx')) {
      this.toaster.error('Please select a valid .xlsx file.');
      return;
    }

    this.selectedExcelFile = file;
    this.importExcel(); // auto-import after selecting
  }

  importExcel() {
    if (!this.selectedExcelFile) return;

    this.importingExcel = true;

    this.importApi.importFromExcel(this.selectedExcelFile).subscribe({
      next: res => {
        this.importResult = res;
        this.toaster.success('Imported successfully.');
        this.list.get(); // refresh table
        this.selectedExcelFile = null;
        this.importingExcel = false;
      },
      error: err => {
        const msg = err?.error?.error?.message ?? err?.error?.message ?? 'Import failed.';
        this.toaster.error(msg);
        this.importingExcel = false;
      },
    });
  }
}
