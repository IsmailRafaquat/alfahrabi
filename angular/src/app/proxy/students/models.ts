import type { Gender } from './gender.enum';
import type { City } from './city.enum';
import type { Province } from './province.enum';
import type { RelationShipToStudent } from './relation-ship-to-student.enum';
import type { GradeLevel } from './grade-level.enum';
import type { Section } from './section.enum';
import type { Status } from './status.enum';
import type { Term } from './term.enum';
import type { Shift } from './shift.enum';
import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { StudentDocumentDto } from '../student-documents/models';

export interface CreateStudentDto {
  admissionNo: string;
  rollNo?: string;
  firstName: string;
  lastName: string;
  gender: Gender;
  dob: string;
  email?: string;
  streetAddress: string;
  streetAddressLine2?: string;
  city: City;
  province: Province;
  zipCode: string;
  pFirstName: string;
  pLastName: string;
  pRelatonShipToStudent: RelationShipToStudent;
  pPhone: string;
  pEmail?: string;
  ecFirstName?: string;
  ecLastName?: string;
  ecRelationShipToStudent?: RelationShipToStudent;
  ecPhone?: string;
  ecEmail?: string;
  gradeLevel: GradeLevel;
  section: Section;
  enrollmentDate: string;
  status?: Status;
  term: Term;
  shift: Shift;
  perviousSchool?: string;
  grade?: GradeLevel;
  studentIdNo?: string;
  medicalConditions?: string;
  extracurrucular?: string;
  commnets?: string;
  accommodations?: string;
}

export interface GenerateStudentImportTemplateDto extends EntityDto {
  gradeLevel?: GradeLevel;
  section?: Section;
  extraEmptyRows: number;
  includeExistingStudents: boolean;
}

export interface GetStudentListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  admissionNo?: string;
  firstName?: string;
  lastName?: string;
  gradeLevel?: GradeLevel;
  section?: Section;
  term?: Term;
  shift?: Shift;
  dob?: string;
  gender?: Gender;
  status?: Status;
}

export interface StudentDto extends EntityDto<string> {
  tenantId?: string;
  admissionNo?: string;
  rollNo?: string;
  firstName?: string;
  lastName?: string;
  gender?: Gender;
  dob?: string;
  email?: string;
  streetAddress?: string;
  streetAddressLine2?: string;
  city?: City;
  province?: Province;
  zipCode?: string;
  pFirstName?: string;
  pLastName?: string;
  pRelatonShipToStudent?: RelationShipToStudent;
  pPhone?: string;
  pEmail?: string;
  ecFirstName?: string;
  ecLastName?: string;
  ecRelationShipToStudent?: RelationShipToStudent;
  ecPhone?: string;
  ecEmail?: string;
  gradeLevel?: GradeLevel;
  section?: Section;
  enrollmentDate?: string;
  status?: Status;
  term?: Term;
  shift?: Shift;
  perviousSchool?: string;
  grade?: GradeLevel;
  studentIdNo?: string;
  medicalConditions?: string;
  extracurrucular?: string;
  commnets?: string;
  accommodations?: string;
  studentDocument: StudentDocumentDto[];
}

export interface StudentLookupDto extends EntityDto<string> {
  admissionNo?: string;
  firstName?: string;
  lastName?: string;
  gradeLevel?: GradeLevel;
}

export interface UpdateStudentDto {
  admissionNo: string;
  rollNo?: string;
  firstName: string;
  lastName: string;
  gender: Gender;
  dob: string;
  email?: string;
  streetAddress: string;
  streetAddressLine2?: string;
  city: City;
  province: Province;
  zipCode: string;
  pFirstName: string;
  pLastName: string;
  pRelatonShipToStudent: RelationShipToStudent;
  pPhone: string;
  pEmail?: string;
  ecFirstName?: string;
  ecLastName?: string;
  ecRelationShipToStudent?: RelationShipToStudent;
  ecPhone?: string;
  ecEmail?: string;
  gradeLevel: GradeLevel;
  section: Section;
  enrollmentDate: string;
  status?: Status;
  term: Term;
  shift: Shift;
  perviousSchool?: string;
  grade?: GradeLevel;
  studentIdNo?: string;
  medicalConditions?: string;
  extracurrucular?: string;
  commnets?: string;
  accommodations?: string;
}
