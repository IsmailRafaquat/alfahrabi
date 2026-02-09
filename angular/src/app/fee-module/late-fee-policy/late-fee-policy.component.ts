import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, ToasterService, Confirmation } from '@abp/ng.theme.shared';
import { Component, inject, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import {
  LateFeePolicyDto,
  GetLateFeePolicyListDto,
  LateFeeType,
  LateFeePolicyService,
  CreateUpdateLateFeePolicyDto,
} from 'src/app/proxy/fee-module/late-fee-policies';

import {
  gradeLevelOptions,
  sectionOptions,
  shiftOptions,
  termOptions,
  GradeLevel,
  Section,
  Shift,
  Term,
} from 'src/app/proxy/students';
import { ConfirmationHelperService } from 'src/app/shared/services/confirmation-helper.service';

@Component({
  selector: 'app-late-fee-policy',
  standalone: false,
  templateUrl: './late-fee-policy.component.html',
  styleUrl: './late-fee-policy.component.scss',
  providers: [ListService],
})
export class LateFeePolicyComponent implements OnInit {
  policies = { items: [], totalCount: 0 } as PagedResultDto<LateFeePolicyDto>;

  showFilter = false;

  isModalOpen = false;
  form!: FormGroup;
  selected = {} as LateFeePolicyDto;

  filters = {} as GetLateFeePolicyListDto;

  gradeLevels = gradeLevelOptions;
  sections = sectionOptions;
  shifts = shiftOptions;
  terms = termOptions;

  lateFeeTypes = [
    { value: LateFeeType.FixedOnce, label: '::Enum:LateFeeType.FixedOnce' },
    { value: LateFeeType.FixedPerDay, label: '::Enum:LateFeeType.FixedPerDay' },
  ];

  public readonly list = inject(ListService);
  private readonly service = inject(LateFeePolicyService);

  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);
  private readonly customConfirmation = inject(ConfirmationHelperService);
  private readonly toaster = inject(ToasterService);

  ngOnInit(): void {
    const streamCreator = (query: GetLateFeePolicyListDto) =>
      this.service.getList({ ...query, ...this.filters });

    this.list.hookToQuery(streamCreator).subscribe(res => (this.policies = res));
  }

  create(): void {
    this.selected = {} as LateFeePolicyDto;
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

    const input = this.form.value as CreateUpdateLateFeePolicyDto;

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
    this.customConfirmation.confirmDelete().subscribe(status => {
      if (status !== 'confirm') return;

      this.service.delete(id).subscribe(() => {
        this.toaster.success('::DeletedSuccessfully');
        this.list.get();
      });
    });
  }

  clearFilters(): void {
    this.filters = {} as GetLateFeePolicyListDto;
    this.list.get();
  }

  private buildForm(): void {
    this.form = this.fb.group({
      gradeLevel: [this.selected.gradeLevel ?? null],
      section: [this.selected.section ?? null],
      shift: [this.selected.shift ?? null],
      term: [this.selected.term ?? null],
      graceDays: [this.selected.graceDays || 0, [Validators.required, Validators.min(0)]],
      type: [
        this.selected.id !== undefined ? this.selected.type : LateFeeType.FixedOnce,
        Validators.required,
      ],
      value: [this.selected.value || 0, [Validators.required, Validators.min(0)]],
      isActive: [this.selected.id ? this.selected.isActive : true],
    });
  }

  private closeModalAndRefresh(): void {
    this.isModalOpen = false;
    this.form.reset();
    this.list.get();
  }

  // ---- UI helpers ----

  getGradeLevelLabel(gradeLevel: number | null | undefined): string {
    if (gradeLevel === null || gradeLevel === undefined) return '::Global';
    return GradeLevel[gradeLevel] || gradeLevel.toString();
  }

  getSectionLabel(section: number | null | undefined): string {
    if (section === null || section === undefined) return '-';
    return Section[section] || section.toString();
  }

  getShiftLabel(shift: number | null | undefined): string {
    if (shift === null || shift === undefined) return '-';
    return Shift[shift] || shift.toString();
  }

  getTermLabel(term: number | null | undefined): string {
    if (term === null || term === undefined) return '-';
    return Term[term] || term.toString();
  }

  getLateFeeTypeLabel(type: number): string {
    return type === LateFeeType.FixedOnce
      ? '::Enum:LateFeeType.FixedOnce'
      : '::Enum:LateFeeType.FixedPerDay';
  }

  getApplicabilityLabel(row: LateFeePolicyDto): string {
    if (row.isGlobal) {
      return '::AllStudents';
    }

    const parts: string[] = [];
    if (row.gradeLevel !== null && row.gradeLevel !== undefined) {
      parts.push(GradeLevel[row.gradeLevel]);
    }
    if (row.section !== null && row.section !== undefined) {
      parts.push(Section[row.section]);
    }
    if (row.shift !== null && row.shift !== undefined) {
      parts.push(Shift[row.shift]);
    }
    if (row.term !== null && row.term !== undefined) {
      parts.push(Term[row.term]);
    }

    return parts.join(' / ') || '::Global';
  }
}
