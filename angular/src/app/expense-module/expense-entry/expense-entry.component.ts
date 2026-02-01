import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, ToasterService, Confirmation } from '@abp/ng.theme.shared';
import { Component, inject, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import {
  ExpenseCategoryLookupDto,
  ExpenseCategoryService,
} from 'src/app/proxy/expenses/expense-categories';
import {
  CreateUpdateExpenseEntryDto,
  ExpenseEntryDto,
  ExpenseService,
  GetExpenseEntryListInput,
} from 'src/app/proxy/expenses/expense-entries';

@Component({
  selector: 'app-expense-entry',
  standalone: false,
  templateUrl: './expense-entry.component.html',
  styleUrl: './expense-entry.component.scss',
  providers: [ListService],
})
export class ExpenseEntryComponent implements OnInit {
  expenses = { items: [], totalCount: 0 } as PagedResultDto<ExpenseEntryDto>;
  showFilter = false;

  filters = {} as GetExpenseEntryListInput;

  isModalOpen = false;
  form!: FormGroup;
  selected = {} as ExpenseEntryDto;

  categoryOptions: ExpenseCategoryLookupDto[] = [];

  public readonly list = inject(ListService);
  private readonly service = inject(ExpenseService);
  private readonly categoryService = inject(ExpenseCategoryService);
  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);

  ngOnInit(): void {
    const streamCreator = (query: GetExpenseEntryListInput) =>
      this.service.getList({ ...query, ...this.filters });

    this.list.hookToQuery(streamCreator).subscribe(res => (this.expenses = res));

    this.loadCategories();
  }

  private loadCategories(): void {
    this.categoryService.getExpenseCategoryLookup().subscribe(res => {
      this.categoryOptions = res || [];
    });
  }

  create(): void {
    this.selected = {} as ExpenseEntryDto;
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

    const input = this.form.value as CreateUpdateExpenseEntryDto;

    if (this.selected.id) {
      this.service.update(this.selected.id, input).subscribe(() => {
        this.toaster.success('::UpdatedSuccessfully');
        this.close();
      });
    } else {
      this.service.create(input).subscribe(() => {
        this.toaster.success('::CreatedSuccessfully');
        this.close();
      });
    }
  }

  delete(id: string): void {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.service.delete(id).subscribe(() => {
          this.toaster.success('::DeletedSuccessfully');
          this.list.get();
        });
      }
    });
  }

  clearFilters(): void {
    this.filters = {} as GetExpenseEntryListInput;
    this.list.get();
  }

  private buildForm(): void {
    this.form = this.fb.group({
      expenseDate: [
        this.selected.expenseDate || new Date().toISOString().substring(0, 10),
        Validators.required,
      ],
      expenseCategoryId: [this.selected.expenseCategoryId || null, Validators.required],
      title: [this.selected.title || '', Validators.required],
      amount: [this.selected.amount || 0, [Validators.required, Validators.min(0.01)]],
      paidTo: [this.selected.paidTo || null],
      remarks: [this.selected.remarks || null],
    });
  }

  private close(): void {
    this.isModalOpen = false;
    this.form.reset();
    this.list.get();
  }
}
