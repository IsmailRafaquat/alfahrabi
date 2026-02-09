import { ListService, mapEnumToOptions, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, ToasterService, Confirmation } from '@abp/ng.theme.shared';
import { Component, inject, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import {
  FeeStructureDto,
  FeeStructureService,
  GetFeeStructureListInput,
  CreateUpdateFeeStructureDto,
} from 'src/app/proxy/fee-module/fee-structures';
import { GradeLevel, Shift, Term } from 'src/app/proxy/students';
import { ConfirmationHelperService } from 'src/app/shared/services/confirmation-helper.service';

@Component({
  selector: 'app-fee-structure',
  standalone: false,
  templateUrl: './fee-structure.component.html',
  styleUrl: './fee-structure.component.scss',
  providers: [ListService],
})
export class FeeStructureComponent implements OnInit {
  feeStructures = { items: [], totalCount: 0 } as PagedResultDto<FeeStructureDto>;

  isModalOpen = false;
  form: FormGroup;
  selected = {} as FeeStructureDto;

  // Enum options for selects
  gradeLevelOptions = mapEnumToOptions(GradeLevel);
  shiftOptions = mapEnumToOptions(Shift);
  termOptions = mapEnumToOptions(Term);

  public readonly list = inject(ListService);
  private readonly service = inject(FeeStructureService);
  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);
  private readonly customConfirmation = inject(ConfirmationHelperService);
  private readonly toaster = inject(ToasterService);

  ngOnInit(): void {
    const streamCreator = (query: GetFeeStructureListInput) => this.service.getList(query);

    this.list.hookToQuery(streamCreator).subscribe(res => {
      this.feeStructures = res;
    });
  }

  create(): void {
    this.selected = {} as FeeStructureDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  edit(id: string): void {
    this.service.get(id).subscribe({
      next: dto => {
        this.selected = dto;
        this.buildForm();
        this.isModalOpen = true;
      },
      error: err => this.handleError(err),
    });
  }

  private buildForm(): void {
    this.form = this.fb.group({
      gradeLevel: [this.selected.gradeLevel ?? null, Validators.required],
      shift: [this.selected.shift ?? null, Validators.required],
      term: [this.selected.term ?? null, Validators.required],

      effectiveFrom: [
        this.toDateInputValue(this.selected.effectiveFrom) || '',
        Validators.required,
      ],
      effectiveTo: [this.toDateInputValue(this.selected.effectiveTo) || ''],

      isActive: [this.selected.id ? this.selected.isActive : true],
    });
  }

  save(): void {
    if (this.form.invalid) return;

    const v = this.form.value;

    const input: CreateUpdateFeeStructureDto = {
      gradeLevel: v.gradeLevel,
      shift: v.shift,
      term: v.term,
      effectiveFrom: this.toIsoStringFromDateInput(v.effectiveFrom), // string
      effectiveTo: v.effectiveTo ? this.toIsoStringFromDateInput(v.effectiveTo) : null, // string|null
      isActive: v.isActive,
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

  toggleActive(row: FeeStructureDto): void {
    const newStatus = !row.isActive;

    this.confirmation
      .warn(newStatus ? '::AreYouSureToActivate' : '::AreYouSureToDeactivate', '::AreYouSure')
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) return;

        this.service.setActive(row.id, newStatus).subscribe({
          next: () => {
            this.toaster.success(
              newStatus ? '::FeeStructureActivated' : '::FeeStructureDeactivated',
            );
            this.list.get();
          },
          error: err => this.handleError(err),
        });
      });
  }

  private closeModalAndRefresh(): void {
    this.isModalOpen = false;
    this.form.reset();
    this.list.get();
  }

  private handleError(err: any): void {
    const msg =
      err?.error?.error?.message || err?.error?.message || err?.message || '::UnexpectedError';

    this.toaster.error(msg);
  }
  private toIsoStringFromDateInput(value: string): string {
    // value is 'yyyy-MM-dd' from <input type="date">
    // Send midnight UTC to avoid timezone shifting on server/client
    return new Date(`${value}T00:00:00.000Z`).toISOString();
  }

  private toDateInputValue(value?: string | null): string {
    if (!value) return '';
    // value could be '2026-01-18T00:00:00Z' etc.
    return value.slice(0, 10); // 'yyyy-MM-dd'
  }
}
