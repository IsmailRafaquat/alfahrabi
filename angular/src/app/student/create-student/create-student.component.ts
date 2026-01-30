import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import {
  StudentDocumentDto,
  StudentDocumentService,
  studentDocumentTypeOptions,
} from 'src/app/proxy/student-documents';
import {
  StudentDto,
  genderOptions,
  statusOptions,
  gradeLevelOptions,
  sectionOptions,
  termOptions,
  shiftOptions,
  cityOptions,
  provinceOptions,
  relationShipToStudentOptions,
  StudentService,
  UpdateStudentDto,
  CreateStudentDto,
} from 'src/app/proxy/students';
import { CustomStudentDocumentService } from 'src/custom-services/upload-documents/student-document-service';

@Component({
  selector: 'app-create-student',
  standalone: false,
  templateUrl: './create-student.component.html',
  styleUrl: './create-student.component.scss',
})
export class CreateStudentComponent implements OnInit {
  form: FormGroup;
  documentForm!: FormGroup;

  id: string | null = null;
  isViewMode = false;
  currentStep = 0;

  selectedStudent = {} as StudentDto;

  genders = genderOptions;
  statuses = statusOptions;
  gradeLevels = gradeLevelOptions;
  sections = sectionOptions;
  terms = termOptions;
  shifts = shiftOptions;
  cities = cityOptions;
  provinces = provinceOptions;
  relationships = relationShipToStudentOptions;

  studentDocumentTypes = studentDocumentTypeOptions;

  uploadedDocuments: StudentDocumentDto[] = [];
  editingInlineDocId: string | null = null;
  private inlineEditBackup: Record<string, StudentDocumentDto> = {};


  selectedFile: File | null = null;
  selectedDocumentType: number | null = null;
  description = '';
  issueDate: string | null = null;
  expireDate: string | null = null;
  docAccept = '.pdf,.doc,.docx,.jpg,.jpeg,.png'; // same as consumer hint (adjust if needed)
  maxFileSizeMb = 10; 
  

  constructor(
    private fb: FormBuilder,
    private studentService: StudentService,
    private studentDocumentService: StudentDocumentService,
    private customStudentDocumentService: CustomStudentDocumentService,
    private toaster: ToasterService,
    private router: Router,
    private route: ActivatedRoute,
    private confirmation: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(pm => {
      const newId = pm.get('id');
      const newView = pm.get('view') === 'true';

      const idChanged = this.id !== newId;

      this.id = newId;
      this.isViewMode = newView;

      // build form once
      if (!this.form) {
        this.buildForm();
      }

      if (this.id && (idChanged || !this.selectedStudent?.id)) {
        this.studentService.get(this.id).subscribe(s => {
          this.selectedStudent = s;

          this.uploadedDocuments = (s.studentDocument ?? []).map(d => ({
            ...d,
            issueDate: this.toDateOnly(d.issueDate) as any,
            expireDate: this.toDateOnly(d.expireDate) as any,
          }));

          const dob = this.toDateOnly(s.dob);
          const enrollmentDate = this.toDateOnly(s.enrollmentDate);

          this.form.patchValue({
            ...s,
            dob,
            enrollmentDate,
          });

          if (this.isViewMode) {
            this.form.disable({ emitEvent: false });
          } else {
            this.form.enable({ emitEvent: false });
          }
        });
      } else {
        // create mode
        if (this.isViewMode) {
          this.form.disable({ emitEvent: false });
        } else {
          this.form.enable({ emitEvent: false });
        }
      }
    });
  }

  enableEditMode(): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { view: null },
      queryParamsHandling: 'merge',
    });

    this.isViewMode = false;
    this.form.enable();
  }

  buildForm(): void {
    this.form = this.fb.group({
      admissionNo: [this.selectedStudent.admissionNo, Validators.required],

      firstName: [this.selectedStudent.firstName, Validators.required],
      lastName: [this.selectedStudent.lastName, Validators.required],
      gender: [this.selectedStudent.gender, Validators.required],
      dob: [this.selectedStudent.dob, Validators.required],
      email: [this.selectedStudent.email, [Validators.email]],

      // Address
      streetAddress: [this.selectedStudent.streetAddress, Validators.required],
      streetAddressLine2: [this.selectedStudent.streetAddressLine2],
      city: [this.selectedStudent.city, Validators.required],
      province: [this.selectedStudent.province, Validators.required],
      zipCode: [this.selectedStudent.zipCode, Validators.required],
      // Parent / Guardian
      pFirstName: [this.selectedStudent.pFirstName, Validators.required],
      pLastName: [this.selectedStudent.pLastName, Validators.required],
      pRelatonShipToStudent: [this.selectedStudent.pRelatonShipToStudent, Validators.required],
      pPhone: [this.selectedStudent.pPhone, Validators.required],
      pEmail: [this.selectedStudent.pEmail, [Validators.email]],

      // Emergency (casing fixed to ec* to match your detail modal usage)
      ecFirstName: [this.selectedStudent.ecFirstName],
      ecLastName: [this.selectedStudent.ecLastName],
      ecRelationShipToStudent: [this.selectedStudent.ecRelationShipToStudent],
      ecPhone: [this.selectedStudent.ecPhone],
      ecEmail: [this.selectedStudent.ecEmail, Validators.email],
      // Education
      gradeLevel: [this.selectedStudent.gradeLevel, Validators.required],
      section: [this.selectedStudent.section, Validators.required],
      term: [this.selectedStudent.term, Validators.required],
      shift: [this.selectedStudent.shift, Validators.required],
      enrollmentDate: [this.selectedStudent.enrollmentDate, Validators.required],
      status: [this.selectedStudent.status, Validators.required],

      // Background
      perviousSchool: [this.selectedStudent.perviousSchool],
      grade: [this.selectedStudent.grade, Validators.required],
      studentIdNo: [this.selectedStudent.studentIdNo],

      // Additional
      medicalConditions: [this.selectedStudent.medicalConditions],
      extracurrucular: [this.selectedStudent.extracurrucular],
      commnets: [this.selectedStudent.commnets],
      accommodations: [this.selectedStudent.accommodations],
    });

    this.documentForm = this.fb.group({
      documentType: [null, Validators.required],
      issueDate: [null],
      expireDate: [null],
      description: [''],
    });
  }

  saveAndNext(): void {
    if (this.isViewMode) return;

    // validate current step (only form steps)
    if (this.currentStep <= 5 && !this.validateStep(this.currentStep)) return;

    // Steps 0..4: just move next
    if (this.currentStep < 5) {
      this.currentStep++;
      return;
    }

    // Step 5: Save student first, then go to Documents step
    if (this.currentStep === 5) {
      this.saveStudentAndGoToDocuments();
      return;
    }

    // Step 6: finish
    if (this.currentStep === 6) {
      this.backToList();
    }
  }

  private saveStudentAndGoToDocuments(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const dto = this.form.getRawValue();

    if (this.id) {
      this.studentService.update(this.id, dto as UpdateStudentDto).subscribe(() => {
        this.toaster.success('::SuccessfullyUpdated');
        this.currentStep = 6; // go to Documents
      });
    } else {
      this.studentService.create(dto as CreateStudentDto).subscribe((res: any) => {
        this.toaster.success('::SuccessfullyCreated');
        this.id = res.id; // IMPORTANT: now you have studentId for upload
        this.currentStep = 6; // go to Documents
      });
    }
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
    this.toaster.error('::StudentNotSavedYet');
    return;
  }

  const docType = this.documentForm.get('documentType')?.value as number | null;
  const issueDate = this.documentForm.get('issueDate')?.value as string | null;
  const expireDate = this.documentForm.get('expireDate')?.value as string | null;
  const description = (this.documentForm.get('description')?.value as string) ?? '';

  if (docType === null || docType === undefined) {
    this.toaster.warn('::PleaseSelectDocumentType');
    return;
  }

  const DOCUMENT_TYPE_OTHER = 8; // adjust if your enum differs
  if (docType === DOCUMENT_TYPE_OTHER && !description?.trim()) {
    this.toaster.warn('::PleaseEnterDescriptionForOtherDocumentType');
    return;
  }

  const formData = new FormData();
  formData.append('file', this.selectedFile);

  formData.append('studentId', this.id);
  formData.append('documentType', docType.toString());

  if (description?.trim()) formData.append('description', description.trim());
  if (issueDate) formData.append('issueDate', issueDate);
  if (expireDate) formData.append('expireDate', expireDate);

  formData.append('isVerified', 'false');

  this.customStudentDocumentService.uploadFormData(formData).subscribe({
    next: (res: any) => {
      this.toaster.success('::Fileuploadedsuccessfully');
      this.uploadedDocuments.push(res);

      // reset UI
      this.selectedFile = null;
      this.documentForm.reset({
        documentType: null,
        issueDate: null,
        expireDate: null,
        description: '',
      });
    },
    error: err => {
      this.toaster.error('::UploadFailed');
      console.error('Student upload error:', err);
      console.log('Response body:', err.error);
    },
  });
}

  uploadAndFinish(): void {
    // allow finish without uploading
    if (!this.selectedFile) {
      this.toaster.success('::SavedSuccessfully');
      this.backToList();
      return;
    }

    this.upload();
  }

  private save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const dto = this.form.getRawValue();

    if (this.id) {
      this.studentService.update(this.id, dto as UpdateStudentDto).subscribe(() => {
        this.toaster.success('::SuccessfullyUpdated');
        this.backToList();
      });
    } else {
      this.studentService.create(dto as CreateStudentDto).subscribe((res: any) => {
        this.toaster.success('::SuccessfullyCreated');
        this.backToList();
      });
    }
  }

  prevStep(): void {
    if (this.currentStep > 0) {
      this.currentStep--;
    }
  }

  backToList(): void {
    this.router.navigate(['/students']);
  }

  private validateStep(step: number): boolean {
    const stepControls: Record<number, string[]> = {
      0: ['admissionNo', 'firstName', 'lastName', 'gender', 'dob', 'email', 'phone'],
      1: ['streetAddress', 'streetAddressLine2', 'city', 'province', 'zipCode'],
      2: ['pFirstName', 'pLastName', 'pRelatonShipToStudent', 'pPhone', 'pEmail'],
      3: ['ecFirstName', 'ecLastName', 'ecRelationShipToStudent', 'ecPhone', 'ecEmail'],
      4: ['gradeLevel', 'section', 'term', 'shift', 'enrollmentDate', 'status'],
      5: [
        'perviousSchool',
        'grade',
        'studentIdNo',
        'medicalConditions',
        'extracurrucular',
        'commnets',
        'accommodations',
      ],
    };

    const keys = stepControls[step] ?? [];
    keys.forEach(k => this.form.get(k)?.markAsTouched());

    return keys.every(k => this.form.get(k)?.valid !== false);
  }

  private toDateOnly(value: any): string | null {
    if (!value) return null;
    const s = String(value);
    return s.includes('T') ? s.split('T')[0] : s; // supports ISO or yyyy-MM-dd
  }

  startInlineEdit(doc: StudentDocumentDto): void {
  // snapshot for cancel
  this.inlineEditBackup[doc.id] = JSON.parse(JSON.stringify(doc));

  // normalize dates for <input type="date">
  doc.issueDate = this.toDateOnly(doc.issueDate) as any;
  doc.expireDate = this.toDateOnly(doc.expireDate) as any;

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
    this.studentDocumentService.delete(id).subscribe(() => {
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

saveInlineEdit(doc: StudentDocumentDto): void {
  if (!doc.documentType) {
    this.toaster.warn('::PleaseCompleteTheFields');
    return;
  }

  // If you have an "Other" rule like consumer, adjust the constant to your real enum value
  const DOCUMENT_TYPE_OTHER = 8;
  if (doc.documentType === DOCUMENT_TYPE_OTHER && !doc.description?.trim()) {
    this.toaster.warn('::PleaseEnterDescriptionForOtherDocumentType');
    return;
  }

  if (!this.id) {
    this.toaster.error('::StudentNotSavedYet');
    return;
  }

  this.studentDocumentService
    .update(doc.id, {
      studentId: this.id,
      documentType: doc.documentType,
      description: doc.description,
      issueDate: doc.issueDate,
      expireDate: doc.expireDate,
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
