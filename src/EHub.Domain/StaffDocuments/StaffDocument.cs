using EHub.FileAttachments;
using EHub.Staffs;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.StaffDocuments;

public class StaffDocument : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid StaffId { get; set; }
    public StaffDocumentType StaffDT { get; set; }

    public DateTime? IssueDate { get; set; }
    public DateTime? ExpireDate { get; set; }

    public string? Description { get; set; }
    public bool IsVerified { get; set; }

    public FileAttachment? FileAttachments { get; set; }

    public virtual Staff? Staff { get; set; }

    private StaffDocument() { }

    internal StaffDocument(
        Guid id,
        Guid staffId,
        StaffDocumentType staffDT,
        DateTime? issueDate,
        DateTime? expireDate,
        string? description,
        bool isVerified,
        FileAttachment? fileAttachments) : base(id)
    {
        StaffId = Check.NotNull(staffId, nameof(staffId));
        StaffDT = staffDT;

        SetDates(issueDate, expireDate);
        SetDescription(description);

        IsVerified = isVerified;
        FileAttachments = fileAttachments;
    }

    public StaffDocument ChangeDescription(string? description)
    {
        SetDescription(description);
        return this;
    }

    public StaffDocument SetVerification(bool isVerified)
    {
        IsVerified = isVerified;
        return this;
    }

    public StaffDocument ChangeDates(DateTime? issueDate, DateTime? expireDate)
    {
        SetDates(issueDate, expireDate);
        return this;
    }

    public StaffDocument ChangeAttachment(FileAttachment? fileAttachment)
    {
        FileAttachments = fileAttachment;
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
            StaffDocumentConsts.DescriptionMaxLength,
            0
        );
    }

    private void SetDates(DateTime? issueDate, DateTime? expireDate)
    {
        if (issueDate.HasValue && expireDate.HasValue && expireDate.Value.Date < issueDate.Value.Date)
        {
            throw new StaffDocumentInvalidDatesException(issueDate.Value, expireDate.Value);
        }

        IssueDate = issueDate?.Date;
        ExpireDate = expireDate?.Date;
    }
}
