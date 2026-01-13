using EHub.FileAttachments;
using System;
using Volo.Abp.Application.Dtos;

namespace EHub.StudentDocuments;

public class StudentDocumentDto : FullAuditedEntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public StudentDocumentType DocumentType { get; set; }

    public DateTime? IssueDate { get; set; }
    public DateTime? ExpireDate { get; set; }

    public string? Description { get; set; }
    public bool IsVerified { get; set; }
    public FileAttachmentDto FileAttachment { get; set; }
}
