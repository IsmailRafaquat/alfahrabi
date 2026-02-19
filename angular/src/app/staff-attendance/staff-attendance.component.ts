import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, ToasterService, Confirmation } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { attendanceStatusOptions, AttendanceStatus } from '../proxy/attendance-statuss';

import {
  StaffService,
  StaffLookupDto,
  departmentOptions, // adjust if your proxy exports different name
  Department, // adjust if your proxy exports different enum name
} from '../proxy/staffs';
import { StaffAttendanceDownloadService } from 'src/custom-services/staff-attendance-template/staff-attendance-download-service';
import {
  StaffAttendanceDto,
  GetStaffAttendanceListDto,
  StaffAttendanceService,
  MarkStaffAttendanceDto,
} from '../proxy/staff-attendances';
import {
  ImportStaffAttendanceResultDto,
  StaffAttendanceImportApi,
} from 'src/custom-services/import-staff-attendance';
import { ConfirmationHelperService } from '../shared/services/confirmation-helper.service';

@Component({
  selector: 'app-staff-attendance',
  standalone: false,
  templateUrl: './staff-attendance.component.html',
  styleUrl: './staff-attendance.component.scss',
  providers: [ListService],
})
export class StaffAttendanceComponent implements OnInit {
  attendances = { items: [], totalCount: 0 } as PagedResultDto<StaffAttendanceDto>;

  form!: FormGroup;
  templateForm!: FormGroup;
  isModalOpen = false;
  isViewModalOpen = false;
  isTemplateModalOpen = false;
  showFilter = false;

  selectedAttendance = {} as StaffAttendanceDto;
  filters = {} as GetStaffAttendanceListDto;

  attendanceStatuses = attendanceStatusOptions;

  attendanceStatusPresent = AttendanceStatus.Present;
  attendanceStatusAbsent = AttendanceStatus.Absent;
  attendanceStatusLate = AttendanceStatus.Late;
  attendanceStatusExcused = AttendanceStatus.Excused;
  attendanceStatusSick = AttendanceStatus.Sick;
  attendanceStatusLeave = AttendanceStatus.Leave;
  attendanceStatusHoliday = AttendanceStatus.Holiday;

  departments = departmentOptions;

  selectedExcelFile: File | null = null;
  importingExcel = false;
  importResult: ImportStaffAttendanceResultDto | null = null;

  // Staff lookup for modal
  staffOptions: StaffLookupDto[] = [];
  staffLoading = false;

  constructor(
    public readonly list: ListService,
    private attendanceService: StaffAttendanceService,
    private importApi: StaffAttendanceImportApi,
    private downloadService: StaffAttendanceDownloadService,
    private staffService: StaffService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private customConfirmation: ConfirmationHelperService,
    private toaster: ToasterService,
  ) {
    this.templateForm = this.fb.group({
      department: [null, Validators.required],
      dateFrom: [null, Validators.required],
      dateTo: [null, Validators.required],
    });
  }

  ngOnInit(): void {
    const streamCreator = (query: any) =>
      this.attendanceService.getList({ ...query, ...this.filters });

    this.list.hookToQuery(streamCreator).subscribe(res => (this.attendances = res));
    this.getStaff();
  }

  getStaff() {
    this.staffService.getStaffLookup().subscribe(res => {
      this.staffOptions = res;
    });
  }

  private buildForm(): void {
    const today = new Date();
    const yyyyMmDd = new Date(today.getFullYear(), today.getMonth(), today.getDate())
      .toISOString()
      .split('T')[0];

    this.form = this.fb.group({
      staffId: [this.selectedAttendance.staffId ?? null, Validators.required],
      attendanceDate: [
        this.toDateOnly(this.selectedAttendance.attendanceDate) ?? yyyyMmDd,
        Validators.required,
      ],
      status: [this.selectedAttendance.status ?? AttendanceStatus.Present, Validators.required],
      remarks: [this.selectedAttendance.remarks ?? null],
    });
  }

  openMarkModal(): void {
    this.selectedAttendance = {} as StaffAttendanceDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  editAttendance(id: string): void {
    if (!id) {
      this.toaster.error('Missing attendance id in row.');
      return;
    }

    this.attendanceService.get(id).subscribe(a => {
      this.selectedAttendance = a;

      const staff = (a as any).staff;
      if (staff?.id) {
        const label = `${staff.firstName} ${staff.lastName} (${staff.employeeCode})`;
        this.staffOptions = [{ id: staff.id, firstName: label } as any];
      }

      this.buildForm();
      this.isModalOpen = true;
    });
  }

  save(): void {
    if (this.form.invalid) return;

    const raw = this.form.getRawValue();

    const dto: MarkStaffAttendanceDto = {
      staffId: raw.staffId,
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
    this.customConfirmation.confirmDelete().subscribe(status => {
      if (status !== 'confirm') return;

      this.attendanceService.delete(id).subscribe(() => {
        this.list.get();
        this.toaster.success('::SuccessfullyDeleted');
      });
    });
  }

  viewDetails(id: string): void {
    this.attendanceService.get(id).subscribe(a => {
      this.selectedAttendance = a;
      this.isViewModalOpen = true;
    });
  }

  clearFilters(): void {
    this.filters = {} as GetStaffAttendanceListDto;
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
      department: null,
      dateFrom: yyyyMmDd,
      dateTo: yyyyMmDd,
    });

    this.isTemplateModalOpen = true;
  }

  private getDepartmentName(value: number | null | undefined): string {
    if (value === null || value === undefined) return '';
    return (Department as any)[value] ?? String(value);
  }

  generateTemplate(): void {
    if (this.templateForm.invalid) {
      this.templateForm.markAllAsTouched();
      return;
    }

    const input = this.templateForm.getRawValue();

    this.downloadService.downloadTemplate(input).subscribe({
      next: (blob: Blob) => {
        const deptName = this.getDepartmentName(input.department);
        const fileName = `StaffAttendance_${deptName}_${input.dateFrom}_${input.dateTo}.xlsx`;

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
    this.importExcel();
  }

  importExcel() {
    if (!this.selectedExcelFile) return;

    this.importingExcel = true;

    this.importApi.importFromExcel(this.selectedExcelFile).subscribe({
      next: res => {
        this.importResult = res;
        this.toaster.success('Imported successfully.');
        this.list.get();
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

  getAttendanceStatusBadgeClass(status: number): string {
    switch (status) {
      case 1:
        return 'status--active'; // Present
      case 2:
        return 'status--blocked'; // Absent
      case 3:
        return 'status--withdrawn'; // Late
      case 4:
        return 'status--graduated'; // Excused
      case 5:
        return 'status--transferred'; // Sick
      case 6:
        return 'status--pending'; // Leave
      case 7:
        return 'status--alumni'; // Holiday
      default:
        return 'soft-badge--secondary';
    }
  }
}
