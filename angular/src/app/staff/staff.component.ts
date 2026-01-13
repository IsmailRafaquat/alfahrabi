import { PagedResultDto, ListService } from '@abp/ng.core';
import { ConfirmationService, ToasterService, Confirmation } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import {
  StaffDto,
  GetStaffListDto,
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
} from '../proxy/staffs';
import {
  genderOptions,
  cityOptions,
  provinceOptions,
  shiftOptions,
  Gender,
  City,
  Province,
  Shift,
} from '../proxy/students';
import { Router } from '@angular/router';

@Component({
  selector: 'app-staff',
  standalone: false,
  templateUrl: './staff.component.html',
  styleUrl: './staff.component.scss',
  providers: [ListService],
})
export class StaffComponent implements OnInit {
  staffs = { items: [], totalCount: 0 } as PagedResultDto<StaffDto>;
  form: FormGroup;
  isModalOpen = false;
  isViewModalOpen = false;
  showFilter = false;
  selectedStaff = {} as StaffDto;
  filters = {} as GetStaffListDto;

  // --- ENUM DROPDOWNS ---
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
  maritalStatus = relationshipStatusOptions;

  constructor(
    public readonly list: ListService,
    private staffService: StaffService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const streamCreator = query => this.staffService.getList({ ...query, ...this.filters });
    this.list.hookToQuery(streamCreator).subscribe(res => (this.staffs = res));
  }

  delete(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.staffService.delete(id).subscribe(() => {
          this.list.get();
          this.toaster.success('::SuccessfullyDeleted');
        });
      }
    });
  }

  clearFilters() {
    this.filters = {} as GetStaffListDto;
    this.list.get();
  }

  openWhatsApp(phone: string) {
    if (!phone) {
      this.toaster.warn('Phone number not available');
      return;
    }

    let normalized = phone.replace(/[^0-9]/g, '');
    if (normalized.startsWith('0')) {
      normalized = '92' + normalized.substring(1);
    }

    const message = encodeURIComponent(
      'Hello! This is a message from EHub regarding your employment information.'
    );
    window.open(`https://wa.me/${normalized}?text=${message}`, '_blank');
  }

  createStaff() {
    this.router.navigate(['/create-staff']);
  }

  viewStaffDetails(id: string) {
    this.router.navigate(['/create-staff'], { queryParams: { id, view: true } });
  }
}
