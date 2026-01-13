import { PagedResultDto, ListService } from '@abp/ng.core';
import { ConfirmationService, ToasterService, Confirmation } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { gradeLevelOptions } from '../proxy/students';
import { SubjectDto, GetSubjectListDto, SubjectService, UpdateSubjectDto, CreateSubjectDto } from '../proxy/subjects';

@Component({
  selector: 'app-subject',
  standalone: false,
  templateUrl: './subject.component.html',
  styleUrl: './subject.component.scss',
  providers: [ListService]
})
export class SubjectComponent implements OnInit{
 subjects = { items: [], totalCount: 0 } as PagedResultDto<SubjectDto>;
  form: FormGroup;
  isModalOpen = false;
  isViewModalOpen = false;
  showFilter = false;
  selectedSubject = {} as SubjectDto;
  filters = {} as GetSubjectListDto;

  // ENUM / DROPDOWNS
  gradeLevels = gradeLevelOptions;

  constructor(
    public readonly list: ListService,
    private subjectService: SubjectService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private toaster: ToasterService
  ) {}

  ngOnInit(): void {
    const streamCreator = (query) => this.subjectService.getList({ ...query, ...this.filters });
    this.list.hookToQuery(streamCreator).subscribe((res) => (this.subjects = res));
  }

  // -----------------------------
  // FORM
  // -----------------------------
  private buildForm() {
    this.form = this.fb.group({
      code: [this.selectedSubject.code || null], // optional => auto-generate on server if empty
      name: [this.selectedSubject.name || '', Validators.required],
      shortName: [this.selectedSubject.shortName || null],
      description: [this.selectedSubject.description || null],
      gradeLevel: [this.selectedSubject.gradeLevel ?? null],
      creditHours: [
        this.selectedSubject.creditHours ?? null,
        [Validators.min(0)],
      ],
      isActive: [this.selectedSubject.isActive ?? true],
    });
  }

  // -----------------------------
  // ACTIONS
  // -----------------------------
  createSubject() {
    this.selectedSubject = {} as SubjectDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  editSubject(id: string) {
    this.subjectService.get(id).subscribe((s) => {
      this.selectedSubject = s;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  save() {
    if (this.form.invalid) return;

    const dto = this.form.getRawValue();

    if (this.selectedSubject.id) {
      this.subjectService.update(this.selectedSubject.id, dto as UpdateSubjectDto).subscribe(() => {
        this.isModalOpen = false;
        this.form.reset();
        this.list.get();
        this.toaster.success('::SuccessfullyUpdated');
      });
    } else {
      this.subjectService.create(dto as CreateSubjectDto).subscribe(() => {
        this.isModalOpen = false;
        this.form.reset();
        this.list.get();
        this.toaster.success('::SuccessfullyCreated');
      });
    }
  }

  delete(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.subjectService.delete(id).subscribe(() => {
          this.list.get();
          this.toaster.success('::SuccessfullyDeleted');
        });
      }
    });
  }

  viewSubjectDetails(id: string) {
    this.subjectService.get(id).subscribe((s) => {
      this.selectedSubject = s;
      this.isViewModalOpen = true;
    });
  }

  clearFilters() {
    this.filters = {} as GetSubjectListDto;
    this.list.get();
  }
}
