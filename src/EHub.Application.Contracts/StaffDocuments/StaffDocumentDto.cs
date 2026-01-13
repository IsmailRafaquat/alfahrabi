using EHub.FileAttachments;
using System;
using Volo.Abp.Application.Dtos;

namespace EHub.StaffDocuments;

public class StaffDocumentDto : FullAuditedEntityDto<Guid>
{
    public Guid StaffId { get; set; }
    public StaffDocumentType StaffDT { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpireDate { get; set; }
    public string? Description { get; set; }
    public bool IsVerified { get; set; }

    public FileAttachmentDto? FileAttachments { get; set; }
}
