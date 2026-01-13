import type { FullAuditedEntityDto } from '@abp/ng.core';
import type { StaffDocumentType } from './staff-document-type.enum';
import type { FileAttachmentDto } from '../file-attachments/models';

export interface StaffDocumentDto extends FullAuditedEntityDto<string> {
  staffId?: string;
  staffDT?: StaffDocumentType;
  issueDate?: string;
  expireDate?: string;
  description?: string;
  isVerified: boolean;
  fileAttachments: FileAttachmentDto;
}

export interface UpdateStaffDocumentDto {
  staffId: string;
  staffDT: StaffDocumentType;
  issueDate?: string;
  expireDate?: string;
  description?: string;
  isVerified: boolean;
}
