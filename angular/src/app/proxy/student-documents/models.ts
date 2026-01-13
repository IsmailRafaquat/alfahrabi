import type { FullAuditedEntityDto } from '@abp/ng.core';
import type { StudentDocumentType } from './student-document-type.enum';
import type { FileAttachmentDto } from '../file-attachments/models';

export interface StudentDocumentDto extends FullAuditedEntityDto<string> {
  studentId?: string;
  documentType?: StudentDocumentType;
  issueDate?: string;
  expireDate?: string;
  description?: string;
  isVerified: boolean;
  fileAttachment: FileAttachmentDto;
}

export interface UpdateStudentDocumentDto {
  studentId: string;
  documentType: StudentDocumentType;
  issueDate?: string;
  expireDate?: string;
  description?: string;
  isVerified: boolean;
}
