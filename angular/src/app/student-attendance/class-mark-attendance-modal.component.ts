import { ToasterService } from '@abp/ng.theme.shared';
import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { attendanceStatusOptions } from '../proxy/attendance-statuss';
import {
  BulkMarkClassStudentAttendanceDto,
  ClassStudentAttendanceRowDto,
  StudentAttendanceService,
} from '../proxy/student-attendances';
import { gradeLevelOptions, sectionOptions } from '../proxy/students';

@Component({
  selector: 'app-class-mark-attendance-modal',
  standalone: false,
  templateUrl: './class-mark-attendance-modal.component.html',
})
export class ClassMarkAttendanceModalComponent {
  @Output() saved = new EventEmitter<void>();

  isOpen = false;
  form: FormGroup;

  rows: ClassStudentAttendanceRowDto[] = [];

  loadingStudents = false;
  saving = false;
  studentsLoaded = false;

  gradeLevels = gradeLevelOptions;
  sections = sectionOptions;
  attendanceStatuses = attendanceStatusOptions;

  constructor(
    private fb: FormBuilder,
    private attendanceService: StudentAttendanceService,
    private toaster: ToasterService
  ) {
    this.form = this.fb.group({
      gradeLevel: [null, Validators.required],
      section: [null, Validators.required],
      attendanceDate: [this.todayDateOnly(), Validators.required],
    });
  }

  open(): void {
    this.rows = [];

    this.form.reset({
      gradeLevel: null,
      section: null,
      attendanceDate: this.todayDateOnly(),
    });

    this.isOpen = true;
  }

  close(): void {
    this.isOpen = false;
  }

  loadStudents(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loadingStudents = true;

    const input = this.form.getRawValue();

    this.attendanceService.getClassStudentsForAttendance(input).subscribe({
      next: res => {
        this.rows = res ?? [];
        this.loadingStudents = false;
      },
      error: err => {
        this.loadingStudents = false;
        this.toaster.error(err?.error?.error?.message ?? 'Failed to load students.');
      },
    });
  }

  save(): void {
    if (!this.rows.length) return;

    this.saving = true;

    const raw = this.form.getRawValue();

    const input: BulkMarkClassStudentAttendanceDto = {
      attendanceDate: raw.attendanceDate,
      items: this.rows.map(x => ({
        studentId: x.studentId,
        status: x.status,
        remarks: x.remarks,
      })),
    };

    this.attendanceService.bulkMarkClassAttendance(input).subscribe({
      next: () => {
        this.saving = false;
        this.isOpen = false;
        this.rows = [];

        this.toaster.success('::SavedSuccessfully');
        this.saved.emit();
      },
      error: err => {
        this.saving = false;
        this.toaster.error(err?.error?.error?.message ?? 'Failed to save attendance.');
      },
    });
  }

  private todayDateOnly(): string {
    const d = new Date();
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }
}