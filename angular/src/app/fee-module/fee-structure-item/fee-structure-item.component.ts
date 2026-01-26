import { PagedResultDto, ListService } from '@abp/ng.core';
import { ConfirmationService, ToasterService, Confirmation } from '@abp/ng.theme.shared';
import { Component, inject, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { FeeHeadDto, FeeHeadLookupDto, FeeHeadService, GetFeeHeadListInput } from 'src/app/proxy/fee-module/fee-heads';
import { FeeStructureItemDto, FeeStructureItemService, GetFeeStructureItemListInput, CreateUpdateFeeStructureItemDto } from 'src/app/proxy/fee-module/fee-structure-items';
import { FeeStructureDto, FeeStructureLookupDto, FeeStructureService, GetFeeStructureListInput } from 'src/app/proxy/fee-module/fee-structures';

@Component({
  selector: 'app-fee-structure-item',
  standalone: false,
  templateUrl: './fee-structure-item.component.html',
  styleUrl: './fee-structure-item.component.scss',
  providers: [ListService]
})
export class FeeStructureItemComponent implements OnInit{
items = { items: [], totalCount: 0 } as PagedResultDto<FeeStructureItemDto>;

  isModalOpen = false;
  form!: FormGroup;
  selected = {} as FeeStructureItemDto;

  feeHeadOptions: FeeHeadLookupDto[] = [];
  feeStructureOptions: FeeStructureLookupDto[] = [];


  public readonly list = inject(ListService);

  private readonly fb = inject(FormBuilder);
  private readonly service = inject(FeeStructureItemService);
  private readonly feeHeadService = inject(FeeHeadService);
  private readonly feeStructureService = inject(FeeStructureService);

  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);

  ngOnInit(): void {
    const streamCreator = (query: GetFeeStructureItemListInput) => this.service.getList(query);
    this.list.hookToQuery(streamCreator).subscribe((res) => (this.items = res));

    this.loadFeeHeads();
    this.loadFeeStructures();
  }

  private loadFeeHeads(): void {


    this.feeHeadService.getFeeLookup().subscribe((res) => {
      this.feeHeadOptions = res || [];
    });
  }

  private loadFeeStructures(): void {
    this.feeStructureService.getFeeStructureLookup().subscribe((res) => {
      this.feeStructureOptions = res || [];
    });
  }

  create(): void {
    this.selected = {} as FeeStructureItemDto;
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
      feeStructureId: [this.selected.feeStructureId || null, Validators.required],
      feeHeadId: [this.selected.feeHeadId || null, Validators.required],
      monthlyAmount: [
        this.selected.monthlyAmount ?? 0,
        [Validators.required, Validators.min(0)],
      ],
      isMandatory: [this.selected.isMandatory ?? false],
    });
  }

  save(): void {
    if (this.form.invalid) return;

    const input = this.form.value as CreateUpdateFeeStructureItemDto;

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
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.service.delete(id).subscribe(() => {
          this.toaster.success('::DeletedSuccessfully');
          this.list.get();
        });
      }
    });
  }

  toggleActive(row: FeeStructureItemDto): void {
    // this.service.s(row.id, !row.isActive).subscribe(() => {
    //   this.toaster.success(row.isActive ? '::DeactivatedSuccessfully' : '::ActivatedSuccessfully');
    //   this.list.get();
    // });
  }

  private closeModalAndRefresh(): void {
    this.isModalOpen = false;
    this.form.reset();
    this.list.get();
  }

}
