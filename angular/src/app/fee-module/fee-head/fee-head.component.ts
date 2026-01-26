import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { Component, inject, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import {
  FeeHeadDto,
  FeeHeadService,
  GetFeeHeadListInput,
  CreateUpdateFeeHeadDto
} from 'src/app/proxy/fee-module/fee-heads';

@Component({
  selector: 'app-fee-head',
  standalone: false,
  templateUrl: './fee-head.component.html',
  styleUrl: './fee-head.component.scss',
  providers: [ListService],
})
export class FeeHeadComponent implements OnInit {
  feeHeads = { items: [], totalCount: 0 } as PagedResultDto<FeeHeadDto>;

  isModalOpen = false;
  form: FormGroup;
  selected = {} as FeeHeadDto;

  public readonly list = inject(ListService);
  private readonly service = inject(FeeHeadService);
  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);

  ngOnInit(): void {
    const streamCreator = (query: GetFeeHeadListInput) => this.service.getList(query);

    this.list.hookToQuery(streamCreator).subscribe((response) => {
      this.feeHeads = response;
    });
  }

  create(): void {
    this.selected = {} as FeeHeadDto;
    this.buildForm();
    this.isModalOpen = true;
  }

  edit(id: string): void {
    this.service.get(id).subscribe((dto) => {
      this.selected = dto;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  private buildForm(): void {
    this.form = this.fb.group({
      name: [this.selected.name || '', [Validators.required, Validators.maxLength(128)]],
      isActive: [this.selected.id ? this.selected.isActive : true],
    });
  }

  save(): void {
    if (this.form.invalid) return;

    const input = this.form.value as CreateUpdateFeeHeadDto;

    if (this.selected.id) {
      this.service.update(this.selected.id, input).subscribe({
        next: () => {
          this.toaster.success('::UpdatedSuccessfully'); // add localization key
          this.closeModalAndRefresh();
        },
        error: (err) => this.handleError(err),
      });
    } else {
      this.service.create(input).subscribe({
        next: () => {
          this.toaster.success('::CreatedSuccessfully'); // add localization key
          this.closeModalAndRefresh();
        },
        error: (err) => this.handleError(err),
      });
    }
  }

  delete(id: string): void {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.service.delete(id).subscribe({
          next: () => {
            this.toaster.success('::DeletedSuccessfully'); // add localization key
            this.list.get();
          },
          error: (err) => this.handleError(err),
        });
      }
    });
  }

  toggleActive(row: FeeHeadDto): void {
    const newStatus = !row.isActive;

    // Optional confirm (remove if you don't want it)
    this.confirmation
      .warn(
        newStatus ? '::AreYouSureToActivate' : '::AreYouSureToDeactivate',
        '::AreYouSure'
      )
      .subscribe((status) => {
        if (status !== Confirmation.Status.confirm) return;

        this.service.setActive(row.id, newStatus).subscribe({
          next: () => {
            this.toaster.success(newStatus ? '::FeeHeadActivated' : '::FeeHeadDeactivated'); // keys
            this.list.get();
          },
          error: (err) => this.handleError(err),
        });
      });
  }

  private closeModalAndRefresh(): void {
    this.isModalOpen = false;
    this.form.reset();
    this.list.get();
  }

  private handleError(err: any): void {
    // ABP errors typically come as { error: { message, details, ... } }
    const msg =
      err?.error?.error?.message ||
      err?.error?.message ||
      err?.message ||
      '::UnexpectedError';

    // If backend throws UserFriendlyException, message will show here.
    this.toaster.error(msg);
  }
}
