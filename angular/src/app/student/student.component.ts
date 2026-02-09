import { PagedResultDto, ListService } from '@abp/ng.core';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import {
  StudentDto,
  StudentService,
  genderOptions,
  cityOptions,
  gradeLevelOptions,
  sectionOptions,
  shiftOptions,
  termOptions,
  provinceOptions,
  relationShipToStudentOptions,
  statusOptions,
  Status,
  GetStudentListDto,
  Section,
  GradeLevel,
} from '../proxy/students';
import { Router } from '@angular/router';
import { Form, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ImportStudentResultDto, StudentImportApi } from 'src/custom-services/import-student';
import { ConfirmationHelperService } from '../shared/services/confirmation-helper.service';

@Component({
  selector: 'app-student',
  standalone: false,
  templateUrl: './student.component.html',
  styleUrls: ['./student.component.scss'],
  providers: [ListService],
})
export class StudentComponent implements OnInit {
  students = { items: [], totalCount: 0 } as PagedResultDto<StudentDto>;

  showFilter = false;
  templateForm!: FormGroup;
  isTemplateModalOpen = false;

  importingExcel = false;
  importResult: ImportStudentResultDto | null = null;

  selectedStudent = {} as StudentDto;
  filters = {} as GetStudentListDto;

  genders = genderOptions;
  status = statusOptions;
  gradeLevels = gradeLevelOptions;
  sections = sectionOptions;
  terms = termOptions;
  shifts = shiftOptions;
  cities = cityOptions;
  provinces = provinceOptions;
  relationships = relationShipToStudentOptions;

  constructor(
    public readonly list: ListService,
    private studentService: StudentService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private router: Router,
    private fb: FormBuilder,
    private studentImportApi: StudentImportApi,
    private customConfirmation: ConfirmationHelperService
  ) {
    this.templateForm = this.fb.group({
      gradeLevel: [null, Validators.required],
      section: [null, Validators.required],
      includeExistingStudents: [true],
      extraEmptyRows: [30, [Validators.min(0), Validators.max(500)]],
    });
  }

  ngOnInit(): void {
    const streamCreator = query => this.studentService.getList({ ...query, ...this.filters });
    this.list.hookToQuery(streamCreator).subscribe(res => (this.students = res));
  }

  delete(id: string) {
    this.customConfirmation.confirmDelete().subscribe((status) => {
      if (status !== 'confirm') return;

      this.studentService.delete(id).subscribe(() => {
          this.list.get();
          this.toaster.success('::SuccessfullyDeleted');
      })
    })
  }

  clearFilters() {
    this.filters = {} as GetStudentListDto;
    this.list.get();
  }

  openWhatsApp(phone: string) {
    if (!phone) {
      this.toaster.warn('Parent phone number not available');
      return;
    }

    let normalized = phone.replace(/[^0-9]/g, '');

    if (normalized.startsWith('0')) {
      normalized = '92' + normalized.substring(1);
    }

    // Prefilled message
    const message = encodeURIComponent(
      'Hello! This is a message from EHub. We wanted to inform you about your child’s enrollment details.',
    );

    // Open WhatsApp
    window.open(`https://wa.me/${normalized}?text=${message}`, '_blank');
  }

  createStudent() {
    this.router.navigate(['/create-student']);
  }

  // editStudent(id: string) {
  //   this.router.navigate(['/create-student'], { queryParams: { id } });
  // }

  viewStudentDetails(id: string) {
    this.router.navigate(['/create-student'], { queryParams: { id, view: true } });
  }

  openTemplateModal(): void {
    this.templateForm.reset({
      gradeLevel: null,
      section: null,
      includeExistingStudents: true,
      extraEmptyRows: 30,
    });

    this.isTemplateModalOpen = true;
  }

  private getEnumName(enumObj: any, value: number | null | undefined): string {
    if (value === null || value === undefined) return '';
    return enumObj[value] ?? String(value);
  }

  generateTemplate(): void {
    if (this.templateForm.invalid) {
      this.templateForm.markAllAsTouched();
      return;
    }

    const input = this.templateForm.getRawValue();

    this.studentService.downloadImportTemplate(input).subscribe({
      next: (blob: Blob) => {
        const gradeName = this.getEnumName(GradeLevel as any, input.gradeLevel);
        const sectionName = this.getEnumName(Section as any, input.section);

        const fileName = `Students_Import_${gradeName}_${sectionName}.xlsx`;
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

  // ---------------------------
  // Import Excel
  // ---------------------------
  onExcelSelectedAndImport(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    input.value = '';
    if (!file) return;

    if (!file.name.toLowerCase().endsWith('.xlsx')) {
      this.toaster.error('Please select a valid .xlsx file.');
      return;
    }

    this.importExcel(file);
  }

  private importExcel(file: File): void {
    this.importingExcel = true;
    this.importResult = null;

    this.studentImportApi.importExcel(file).subscribe({
      next: res => {
        this.importResult = res;

        const total = res.totalRows ?? 0;
        const skipped = res.skippedRows ?? 0;
        const imported = res.imported ?? 0;

        const msg = `Imported. Total: ${total}, Skipped: ${skipped}, Imported: ${imported}`;
        this.toaster.success(msg);

        if (res.errors?.length) {
          // show first error quickly (optional)
          this.toaster.warn(`Some rows failed. First: ${res.errors[0]}`);
        }

        this.list.get();
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
