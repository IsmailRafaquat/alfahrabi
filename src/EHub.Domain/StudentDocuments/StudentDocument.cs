using EHub.FileAttachments;
using EHub.Students;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.StudentDocuments;

public class StudentDocument : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid StudentId { get; set; }
    public StudentDocumentType DocumentType { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpireDate { get; set; }
    public string? Description { get; set; }
    public bool IsVerified { get; set; }
    public FileAttachment? FileAttachment { get; set; }
    public virtual Student? Student { get; set; }

    public Guid? TenantId { get; set; }

    private StudentDocument() { }

    internal StudentDocument(
        Guid id,
        Guid studentId,
        StudentDocumentType documentType,
        DateTime? issueDate,
        DateTime? expireDate,
        string? description,
        bool isVerified,
        FileAttachment? fileAttachment
    ) : base(id)
    {
        StudentId = Check.NotNull(studentId, nameof(studentId));
        DocumentType = documentType;

        IssueDate = issueDate;
        ExpireDate = expireDate;

        SetDescription(description);

        IsVerified = isVerified;

        FileAttachment = fileAttachment;

        ValidateDates();
    }

    public StudentDocument ChangeDescription(string? description)
    {
        SetDescription(description);
        return this;
    }

    public StudentDocument ChangeDates(DateTime? issueDate, DateTime? expireDate)
    {
        IssueDate = issueDate;
        ExpireDate = expireDate;
        ValidateDates();
        return this;
    }

    public StudentDocument ChangeDocumentType(StudentDocumentType documentType)
    {
        DocumentType = documentType;
        return this;
    }

    public StudentDocument SetFileAttachment(FileAttachment? fileAttachment)
    {
        FileAttachment = fileAttachment;
        return this;
    }

    public StudentDocument Verify()
    {
        IsVerified = true;
        return this;
    }

    public StudentDocument Unverify()
    {
        IsVerified = false;
        return this;
    }

    private void SetDescription(string? description)
    {
        if (description.IsNullOrWhiteSpace())
        {
            Description = null;
            return;
        }

        Description = Check.Length(
            description,
            nameof(description),
            StudentDocumentConsts.DescriptionMaxLength,
            0
        );
    }

    private void ValidateDates()
    {
        if (IssueDate.HasValue && ExpireDate.HasValue)
        {
            var issue = IssueDate.Value.Date;
            var exp = ExpireDate.Value.Date;

            if (exp < issue)
            {
                throw new StudentDocumentExpireDateBeforeIssueDateException(issue, exp);
            }
        }
    }
}
