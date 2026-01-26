import type { Nationality } from './nationality.enum';
import type { RelationshipStatus } from './relationship-status.enum';
import type { Gender } from '../students/gender.enum';
import type { DisabilityStatus } from './disability-status.enum';
import type { City } from '../students/city.enum';
import type { Province } from '../students/province.enum';
import type { Department } from './department.enum';
import type { EmploymentType } from './employment-type.enum';
import type { JobStatus } from './job-status.enum';
import type { Shift } from '../students/shift.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { StaffDocumentDto } from '../staff-documents/models';

export interface CreateStaffDto {
  firstName: string;
  lastName: string;
  phoneNo: string;
  email?: string;
  dob: string;
  nationality: Nationality;
  relationshipStatus: RelationshipStatus;
  gender: Gender;
  languageKnown: string;
  disabilityStatus: DisabilityStatus;
  streetAddress: string;
  streetAddressLine2?: string;
  city: City;
  province: Province;
  zipCode: string;
  joiningDate: string;
  designation?: string;
  department: Department;
  employmentType: EmploymentType;
  jobStatus: JobStatus;
  salary?: number;
  reportingManager?: string;
  workShift?: Shift;
  contractStartDate?: string;
  contractEndDate?: string;
  remarks?: string;
}

export interface GetStaffListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  firstName?: string;
  lastName?: string;
  department?: Department;
  jobStatus?: JobStatus;
  shift?: Shift;
}

export interface StaffDto extends EntityDto<string> {
  firstName?: string;
  lastName?: string;
  phoneNo?: string;
  email?: string;
  dob?: string;
  nationality?: Nationality;
  relationshipStatus?: RelationshipStatus;
  gender?: Gender;
  languageKnown?: string;
  disabilityStatus?: DisabilityStatus;
  streetAddress?: string;
  streetAddressLine2?: string;
  city?: City;
  province?: Province;
  zipCode?: string;
  employeeCode?: string;
  joiningDate?: string;
  designation?: string;
  department?: Department;
  employmentType?: EmploymentType;
  jobStatus?: JobStatus;
  salary?: number;
  reportingManager?: string;
  workShift?: Shift;
  contractStartDate?: string;
  contractEndDate?: string;
  remarks?: string;
  staffDocuments: StaffDocumentDto[];
}

export interface StaffLookupDto extends EntityDto<string> {
  employeeCode?: string;
  firstName?: string;
  lastName?: string;
}

export interface UpdateStaffDto {
  firstName: string;
  lastName: string;
  phoneNo: string;
  email?: string;
  dob: string;
  nationality: Nationality;
  relationshipStatus: RelationshipStatus;
  gender: Gender;
  languageKnown: string;
  disabilityStatus: DisabilityStatus;
  streetAddress: string;
  streetAddressLine2?: string;
  city: City;
  province: Province;
  zipCode: string;
  joiningDate: string;
  designation?: string;
  department: Department;
  employmentType: EmploymentType;
  jobStatus: JobStatus;
  salary?: number;
  reportingManager?: string;
  workShift?: Shift;
  contractStartDate?: string;
  contractEndDate?: string;
  remarks?: string;
}
