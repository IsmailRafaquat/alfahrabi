using EHub.FeeModule.FeeHeads;
using EHub.Staffs;
using EHub.Students;
using System;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.FeeModule.StudentFeeDiscounts;

[Audited]
public class StudentFeeDiscount : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid StudentId { get; private set; }
    public virtual Student Student { get; private set; } = default!;

    public Guid? FeeHeadId { get; private set; }
    public virtual FeeHead? FeeHead { get; private set; }

    public DiscountType DiscountType { get; private set; }
    public decimal Value { get; private set; }

    public int? StartMonth { get; set; }
    public int? EndMonth { get;  set; }

    public string Reason { get; private set; } = string.Empty;

    public Guid? ApprovedByStaffId { get; private set; }
    public virtual Staff? ApprovedByStaff { get; private set; }

    public bool IsActive { get; private set; } = true;

    private StudentFeeDiscount()
    {
        // ORM
    }

    public StudentFeeDiscount(
        Guid id,
        Guid studentId,
        Guid? feeHeadId,
        DiscountType discountType,
        decimal value,
        string reason,
        int? startMonth = null,
        int? endMonth = null,
        Guid? approvedByStaffId = null,
        bool isActive = true
    ) : base(id)
    {
        SetStudentId(studentId);
        SetFeeHeadId(feeHeadId);
        SetDiscountTypeAndValue(discountType, value);
        SetMonthRange(startMonth, endMonth);
        SetReason(reason);
        ApprovedByStaffId = approvedByStaffId;
        IsActive = isActive;
    }

    public StudentFeeDiscount ChangeDiscountTypeAndValue(DiscountType discountType, decimal value)
    {
        SetDiscountTypeAndValue(discountType, value);
        return this;
    }

    public StudentFeeDiscount ChangeMonthRange(int? startMonth, int? endMonth)
    {
        SetMonthRange(startMonth, endMonth);
        return this;
    }

    public StudentFeeDiscount ChangeReason(string reason)
    {
        SetReason(reason);
        return this;
    }

    public StudentFeeDiscount Approve(Guid staffId)
    {
        ApprovedByStaffId = staffId;
        return this;
    }

    public StudentFeeDiscount Activate()
    {
        IsActive = true;
        return this;
    }

    public StudentFeeDiscount Deactivate()
    {
        IsActive = false;
        return this;
    }

    private void SetStudentId(Guid studentId)
    {
        if (studentId == Guid.Empty)
            throw new UserFriendlyException("StudentId is required.");
        StudentId = studentId;
    }

    private void SetFeeHeadId(Guid? feeHeadId)
    {
        // Nullable - if null, applies to total
        FeeHeadId = feeHeadId;
    }

    private void SetDiscountTypeAndValue(DiscountType discountType, decimal value)
    {
        if (value < 0)
            throw new UserFriendlyException("Discount value cannot be negative.");

        if (discountType == DiscountType.Percent && value > 100)
            throw new UserFriendlyException("Percentage discount cannot exceed 100%.");

        DiscountType = discountType;
        Value = value;
    }

    private void SetMonthRange(int? startMonth, int? endMonth)
    {
        if (startMonth.HasValue && (startMonth.Value < 1 || startMonth.Value > 12))
            throw new UserFriendlyException("StartMonth must be between 1 and 12.");

        if (endMonth.HasValue && (endMonth.Value < 1 || endMonth.Value > 12))
            throw new UserFriendlyException("EndMonth must be between 1 and 12.");

        if (startMonth.HasValue && endMonth.HasValue && endMonth.Value < startMonth.Value)
            throw new UserFriendlyException("EndMonth must be >= StartMonth.");

        StartMonth = startMonth;
        EndMonth = endMonth;
    }

    private void SetReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new UserFriendlyException("Reason is required.");

        Reason = reason.Trim();
    }
}
