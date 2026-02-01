import { PagedResultDto, ListService } from '@abp/ng.core';
import { ConfirmationService, ToasterService, Confirmation } from '@abp/ng.theme.shared';
import { Component, inject, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ExpenseCategoryDto, ExpenseCategoryService, GetExpenseCategoryListInput, CreateUpdateExpenseCategoryDto } from 'src/app/proxy/expenses/expense-categories';

@Component({
  selector: 'app-expense-category',
  standalone: false,
  templateUrl: './expense-category.component.html',
  styleUrl: './expense-category.component.scss',
  providers: [ListService]
})
export class ExpenseCategoryComponent implements OnInit{
expenseCategories = { items: [], totalCount: 0 } as PagedResultDto<ExpenseCategoryDto>;

  isModalOpen = false;
  form: FormGroup;
  selected = {} as ExpenseCategoryDto;

  public readonly list = inject(ListService);
  private readonly service = inject(ExpenseCategoryService);
  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);

  ngOnInit(): void {
    const streamCreator = (query: GetExpenseCategoryListInput) => this.service.getList(query);

    this.list.hookToQuery(streamCreator).subscribe((response) => {
      this.expenseCategories = response;
    });
  }

  create(): void {
    this.selected = {} as ExpenseCategoryDto;
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

    const input = this.form.value as CreateUpdateExpenseCategoryDto;

    if (this.selected.id) {
      this.service.update(this.selected.id, input).subscribe({
        next: () => {
          this.toaster.success('::UpdatedSuccessfully');
          this.closeModalAndRefresh();
        },
        error: (err) => this.handleError(err),
      });
    } else {
      this.service.create(input).subscribe({
        next: () => {
          this.toaster.success('::CreatedSuccessfully');
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
            this.toaster.success('::DeletedSuccessfully');
            this.list.get();
          },
          error: (err) => this.handleError(err),
        });
      }
    });
  }

  toggleActive(row: ExpenseCategoryDto): void {
    const newStatus = !row.isActive;

    this.confirmation
      .warn(newStatus ? '::AreYouSureToActivate' : '::AreYouSureToDeactivate', '::AreYouSure')
      .subscribe((status) => {
        if (status !== Confirmation.Status.confirm) return;

        this.service.setActive(row.id, newStatus).subscribe({
          next: () => {
            this.toaster.success(newStatus ? '::ExpenseCategoryActivated' : '::ExpenseCategoryDeactivated');
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
    const msg =
      err?.error?.error?.message ||
      err?.error?.message ||
      err?.message ||
      '::UnexpectedError';

    this.toaster.error(msg);
  }
}

