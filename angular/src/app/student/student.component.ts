import { PagedResultDto, ListService } from '@abp/ng.core';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import {
  StudentDto,
  CreateStudentDto,
  UpdateStudentDto,
  Gender,
  GradeLevel,
  Section,
  Term,
  Shift,
  City,
  Province,
  StudentService,
  genderOptions,
  cityOptions,
  gradeLevelOptions,
  sectionOptions,
  shiftOptions,
  termOptions,
  provinceOptions,
  relationShipToStudentOptions,
  statusOptions,
  Status,
  GetStudentListDto,
} from '../proxy/students';
import { Router } from '@angular/router';

@Component({
  selector: 'app-student',
  standalone: false,
  templateUrl: './student.component.html',
  styleUrls: ['./student.component.scss'],
  providers: [ListService],
})
export class StudentComponent implements OnInit {
  students = { items: [], totalCount: 0 } as PagedResultDto<StudentDto>;

  showFilter = false;
 
  selectedStudent = {} as StudentDto;
  filters = {} as GetStudentListDto;

  genders = genderOptions;
  status = statusOptions;
  gradeLevels = gradeLevelOptions;
  sections = sectionOptions;
  terms = termOptions;
  shifts = shiftOptions;
  cities = cityOptions;
  provinces = provinceOptions;
  relationships = relationShipToStudentOptions;

  constructor(
    public readonly list: ListService,
    private studentService: StudentService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const streamCreator = (query) => this.studentService.getList({...query, ...this.filters});
    this.list.hookToQuery(streamCreator).subscribe((res) => (this.students = res));
  }

  delete(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.studentService.delete(id).subscribe(() => {
          this.list.get();
          this.toaster.success('::SuccessfullyDeleted');
        });
      }
    });
  }

  clearFilters() {
    this.filters = {} as GetStudentListDto;
    this.list.get();
  }


openWhatsApp(phone: string) {
  if (!phone) {
    this.toaster.warn('Parent phone number not available');
    return;
  }

  let normalized = phone.replace(/[^0-9]/g, '');

  if (normalized.startsWith('0')) {
    normalized = '92' + normalized.substring(1);
  }

  // Prefilled message
  const message = encodeURIComponent(
    'Hello! This is a message from EHub. We wanted to inform you about your child’s enrollment details.'
  );

  // Open WhatsApp
  window.open(`https://wa.me/${normalized}?text=${message}`, '_blank');
}

createStudent() {
  this.router.navigate(['/create-student']);
}

// editStudent(id: string) {
//   this.router.navigate(['/create-student'], { queryParams: { id } });
// }

viewStudentDetails(id: string) {
  this.router.navigate(['/create-student'], { queryParams: { id, view: true } });
}


}
