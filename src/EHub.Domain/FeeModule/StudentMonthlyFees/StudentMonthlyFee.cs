using EHub.Students;
using System;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.FeeModule.StudentMonthlyFees;

[Audited]
public class StudentMonthlyFee : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid StudentId { get; private set; }
    public virtual Student Student { get; private set; } = default!;

    public DateTime Month { get; private set; } // first day of month (yyyy-MM-01)
    public DateTime? DueDate { get; private set; }
    public string? Remarks { get; private set; }

    private StudentMonthlyFee() { /* ORM */ }

    public StudentMonthlyFee(
        Guid id,
        Guid studentId,
        DateTime month,
        DateTime? dueDate,
        string? remarks
    ) : base(id)
    {
        SetStudentId(studentId);
        SetMonth(month);
        SetDueDate(dueDate);
        SetRemarks(remarks);
    }

    public StudentMonthlyFee ChangeMonth(DateTime month)
    {
        SetMonth(month);
        return this;
    }

    public StudentMonthlyFee ChangeDueDate(DateTime? dueDate)
    {
        SetDueDate(dueDate);
        return this;
    }

    public StudentMonthlyFee ChangeRemarks(string? remarks)
    {
        SetRemarks(remarks);
        return this;
    }

    private void SetStudentId(Guid studentId)
    {
        if (studentId == Guid.Empty)
            throw new UserFriendlyException("StudentId is required.");
        StudentId = studentId;
    }

    private void SetMonth(DateTime month)
    {
        // store first day of month only
        var normalized = new DateTime(month.Year, month.Month, 1);
        Month = normalized;
    }

    private void SetDueDate(DateTime? dueDate)
    {
        DueDate = dueDate?.Date;
    }

    private void SetRemarks(string? remarks)
    {
        Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim();
    }
}
