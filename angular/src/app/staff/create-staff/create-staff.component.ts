import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CustomStudentDocumentService } from 'src/custom-services/upload-documents/student-document-service';
import {
  StaffDto,
  nationalityOptions,
  relationshipStatusOptions,
  disabilityStatusOptions,
  departmentOptions,
  employmentTypeOptions,
  jobStatusOptions,
  StaffService,
  Nationality,
  RelationshipStatus,
  DisabilityStatus,
  Department,
  EmploymentType,
  JobStatus,
  UpdateStaffDto,
  CreateStaffDto,
} from 'src/app/proxy/staffs';
import {
  genderOptions,
  cityOptions,
  provinceOptions,
  shiftOptions,
  Gender,
  City,
  Province,
  Shift,
} from 'src/app/proxy/students';
import { staffDocumentTypeOptions, StaffDocumentDto, StaffDocumentService } from 'src/app/proxy/staff-documents';
import { CustomStaffDocumentService } from 'src/custom-services/upload-documents/staff-document-service';

@Component({
  selector: 'app-create-staff',
  standalone: false,
  templateUrl: './create-staff.component.html',
  styleUrl: './create-staff.component.scss',
})
export class CreateStaffComponent implements OnInit {
  form: FormGroup;
  documentForm!: FormGroup;

  staffDocumentTypes = staffDocumentTypeOptions;

  uploadedDocuments: StaffDocumentDto[] = [];
  editingInlineDocId: string | null = null;
  private inlineEditBackup: Record<string, StaffDocumentDto> = {};

  selectedFile: File | null = null;
  docAccept = '.pdf,.doc,.docx,.jpg,.jpeg,.png';

  private readonly lastStepIndex = 4;

  id: string | null = null;
  isViewMode = false;
  currentStep = 0;

  selectedStaff = {} as StaffDto;

  // dropdowns
  genders = genderOptions;
  nationalities = nationalityOptions;
  relationships = relationshipStatusOptions;
  disabilities = disabilityStatusOptions;
  cities = cityOptions;
  provinces = provinceOptions;
  departments = departmentOptions;
  employments = employmentTypeOptions;
  statuses = jobStatusOptions;
  shifts = shiftOptions;

  constructor(
    private fb: FormBuilder,
    private staffService: StaffService,
    private staffDocumentService: StaffDocumentService,
    private customStaffDocumentService: CustomStaffDocumentService,
    private toaster: ToasterService,
    private router: Router,
    private route: ActivatedRoute,
    private confirmation: ConfirmationService
  ) {}


  ngOnInit(): void {
    this.buildForm();
    this.route.queryParamMap.subscribe(pm => {
      const newId = pm.get('id');
      const newView = pm.get('view') === 'true';

      const idChanged = this.id !== newId;

      this.id = newId;
      this.isViewMode = newView;
      this.setDefaultsForCreate();

      if (this.id && (idChanged || !this.selectedStaff?.id)) {
        this.staffService.get(this.id).subscribe(s => {
          this.selectedStaff = s;

          this.uploadedDocuments = (s.staffDocuments ?? []);

          this.form.patchValue({
            ...s,
            dob: this.toDateOnly(s.dob),
            joiningDate: this.toDateOnly(s.joiningDate),
            contractStartDate: this.toDateOnly(s.contractStartDate),
            contractEndDate: this.toDateOnly(s.contractEndDate),
          });

          if (this.isViewMode) {
            this.form.disable({ emitEvent: false });
          } else {
            this.form.enable({ emitEvent: false });
          }
        });
      } else {
        if (this.isViewMode) {
          this.form.disable({ emitEvent: false });
        } else {
          this.form.enable({ emitEvent: false });
        }
      }
    });
  }

  buildForm(): void {
    this.form = this.fb.group({
      // Personal
      firstName: [null, Validators.required],
      lastName: [null, Validators.required],
      phoneNo: [null, Validators.required],
      email: [null, Validators.email],
      dob: [null, Validators.required],
      nationality: [Nationality.Pakistani, Validators.required],
      relationshipStatus: [RelationshipStatus.Single, Validators.required],
      gender: [Gender.Male, Validators.required],
      languageKnown: [null, Validators.required],
      disabilityStatus: [DisabilityStatus.None, Validators.required],

      // Address
      streetAddress: [null, Validators.required],
      streetAddressLine2: [null],
      city: [City.Islamabad, Validators.required],
      province: [Province.Punjab, Validators.required],
      zipCode: [null, Validators.required],

      // Job
      joiningDate: [null, Validators.required],
      designation: [null],
      department: [Department.Teaching, Validators.required],
      employmentType: [EmploymentType.FullTime, Validators.required],
      jobStatus: [JobStatus.Active, Validators.required],
      salary: [null],
      reportingManager: [null],
      workShift: [Shift.Morning],

      // Contract + Remarks
      contractStartDate: [null],
      contractEndDate: [null],
      remarks: [null],
    });

    this.documentForm = this.fb.group({
      staffDT: [null, Validators.required],
      issueDate: [null],
      expireDate: [null],
      description: ['']
    });
  }

  private setDefaultsForCreate(): void {
    if (this.id) return;

    this.form.patchValue({
      nationality: Nationality.Pakistani,
      relationshipStatus: RelationshipStatus.Single,
      gender: Gender.Male,
      disabilityStatus: DisabilityStatus.None,
      city: City.Islamabad,
      province: Province.Punjab,
      department: Department.Teaching,
      employmentType: EmploymentType.FullTime,
      jobStatus: JobStatus.Active,
      workShift: Shift.Morning,
    });
  }

  // -------------------------
  // Wizard navigation
  // -------------------------
saveAndNext(): void {
  if (this.isViewMode) return;

  // Validate current step (0..3)
  if (this.currentStep <= 3 && !this.validateStep(this.currentStep)) return;

  // Steps 0..2: next
  if (this.currentStep < 3) {
    this.currentStep++;
    return;
  }

  // Step 3: save staff first then go to documents
  if (this.currentStep === 3) {
    this.saveStaffAndGoToDocuments();
    return;
  }

  // Step 4: finish
  if (this.currentStep === 4) {
    this.backToList();
  }
}

private saveStaffAndGoToDocuments(): void {
  if (this.form.invalid) {
    this.form.markAllAsTouched();
    return;
  }

  const dto = this.form.getRawValue();

  if (this.id) {
    this.staffService.update(this.id, dto as UpdateStaffDto).subscribe(() => {
      this.toaster.success('::SuccessfullyUpdated');
      this.currentStep = 4;
    });
  } else {
    this.staffService.create(dto as CreateStaffDto).subscribe((res: any) => {
      this.toaster.success('::SuccessfullyCreated');
      this.id = res.id;
      this.currentStep = 4;
    });
  }
}


  prevStep(): void {
    if (this.isViewMode) return;
    if (this.currentStep > 0) this.currentStep--;
  }

  // Actions
  enableEditMode(): void {
    // Switch UI mode
    this.isViewMode = false;
    this.currentStep = 0;
    this.form.enable({ emitEvent: false });

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { view: null }, // removing param
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  backToList(): void {
    this.router.navigate(['/staffs']);
  }

  private save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const dto = this.form.getRawValue();

    if (this.id) {
      this.staffService.update(this.id, dto as UpdateStaffDto).subscribe(() => {
        this.toaster.success('::SuccessfullyUpdated');
        this.backToList();
      });
    } else {
      this.staffService.create(dto as CreateStaffDto).subscribe(() => {
        this.toaster.success('::SuccessfullyCreated');
        this.backToList();
      });
    }
  }

  private validateStep(step: number): boolean {
    const stepControls: Record<number, string[]> = {
      0: [
        'firstName',
        'lastName',
        'phoneNo',
        'email',
        'dob',
        'nationality',
        'relationshipStatus',
        'gender',
        'languageKnown',
        'disabilityStatus',
      ],
      1: ['streetAddress', 'streetAddressLine2', 'city', 'province', 'zipCode'],
      2: [
        'joiningDate',
        'designation',
        'department',
        'employmentType',
        'jobStatus',
        'salary',
        'reportingManager',
        'workShift',
      ],
      3: ['contractStartDate', 'contractEndDate', 'remarks'],
    };

    const keys = stepControls[step] ?? [];
    keys.forEach(k => this.form.get(k)?.markAsTouched());

    return keys.every(k => this.form.get(k)?.valid !== false);
  }

  private toDateOnly(value: any): string | null {
    if (!value) return null;
    const s = String(value);
    return s.includes('T') ? s.split('T')[0] : s;
  }

  onFileSelected(file: File | null) {
  this.selectedFile = file;
}

upload(): void {
  if (!this.selectedFile) {
    this.toaster.warn('::Pleaseselectafilefirst');
    return;
  }

  if (!this.id) {
    this.toaster.error('::StaffNotSavedYet');
    return;
  }

  const staffDT = this.documentForm.get('staffDT')?.value as number | null;
  const issueDate = this.documentForm.get('issueDate')?.value as string | null;
  const expireDate = this.documentForm.get('expireDate')?.value as string | null;
  const description = (this.documentForm.get('description')?.value as string) ?? '';

  if (staffDT === null || staffDT === undefined) {
    this.toaster.warn('::PleaseSelectDocumentType');
    return;
  }

  const formData = new FormData();
  formData.append('file', this.selectedFile);

  formData.append('staffId', this.id);
  formData.append('staffDT', staffDT.toString()); // <-- matches your domain naming

  if (description?.trim()) formData.append('description', description.trim());
  if (issueDate) formData.append('issueDate', issueDate);
  if (expireDate) formData.append('expireDate', expireDate);

  formData.append('isVerified', 'false');

  this.customStaffDocumentService.uploadFormData(formData).subscribe({
    next: (res: any) => {
      this.toaster.success('::Fileuploadedsuccessfully');
      this.uploadedDocuments.push(res);

      this.selectedFile = null;
      this.documentForm.reset({
        staffDT: null,
        issueDate: null,
        expireDate: null,
        description: '',
      });
    },
    error: err => {
      this.toaster.error('::UploadFailed');
      console.error('Staff upload error:', err);
      console.log('Response body:', err.error);
    },
  });
}

uploadAndFinish(): void {
  if (!this.selectedFile) {
    this.toaster.success('::SavedSuccessfully');
    this.backToList();
    return;
  }

  this.upload();
}

startInlineEdit(doc: StaffDocumentDto): void {
  this.inlineEditBackup[doc.id] = JSON.parse(JSON.stringify(doc));

  (doc as any).issueDate = this.toDateOnly((doc as any).issueDate) as any;
  (doc as any).expireDate = this.toDateOnly((doc as any).expireDate) as any;

  this.editingInlineDocId = doc.id;
}

cancelInlineEdit(docId: string): void {
  const old = this.inlineEditBackup[docId];
  if (old) {
    const index = this.uploadedDocuments.findIndex(d => d.id === docId);
    if (index > -1) this.uploadedDocuments[index] = old;
  }

  delete this.inlineEditBackup[docId];
  this.editingInlineDocId = null;
}

deleteDocument(id: string, showConfirm: boolean = true): void {
  const doDelete = () => {
    this.staffDocumentService.delete(id).subscribe(() => {
      this.uploadedDocuments = this.uploadedDocuments.filter(d => d.id !== id);
      this.toaster.success('::SuccessfullyDeleted');
    });
  };

  if (!showConfirm) {
    doDelete();
    return;
  }

  this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
    if (status === Confirmation.Status.confirm) {
      doDelete();
    }
  });
}

saveInlineEdit(doc: StaffDocumentDto): void {
  if (!(doc as any).staffDT) {
    this.toaster.warn('::PleaseCompleteTheFields');
    return;
  }

  if (!this.id) {
    this.toaster.error('::StaffNotSavedYet');
    return;
  }

  this.staffDocumentService
    .update(doc.id, {
      staffId: this.id,
      staffDT: (doc as any).staffDT,
      description: (doc as any).description,
      issueDate: (doc as any).issueDate,
      expireDate: (doc as any).expireDate,
      isVerified: (doc as any).isVerified ?? false,
    } as any)
    .subscribe(updated => {
      const index = this.uploadedDocuments.findIndex(d => d.id === doc.id);
      if (index > -1) this.uploadedDocuments[index] = updated;

      delete this.inlineEditBackup[doc.id];
      this.editingInlineDocId = null;
      this.toaster.success('::SuccessfullyUpdated');
    });
}


}
