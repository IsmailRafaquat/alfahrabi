using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace EHub.FeeModule.StudentFeeDiscounts;

public class StudentFeeDiscountManager : DomainService
{
    private readonly IStudentFeeDiscountRepository _repo;

    public StudentFeeDiscountManager(IStudentFeeDiscountRepository repo)
    {
        _repo = repo;
    }

    public async Task<StudentFeeDiscount> CreateAsync(
        Guid studentId,
        Guid? feeHeadId,
        DiscountType discountType,
        decimal value,
        string reason,
        int? startMonth,
        int? endMonth,
        Guid? approvedByStaffId,
        bool isActive)
    {
        // Check for duplicate discount (same student, feeHead, and active in overlapping period)
        var exists = await _repo.HasOverlappingDiscountAsync(
            studentId,
            feeHeadId,
            startMonth,
            endMonth,
            exceptId: null);

        if (exists)
        {
            throw new UserFriendlyException(
                "A discount already exists for this student and fee head with overlapping month range.");
        }

        var entity = new StudentFeeDiscount(
            GuidGenerator.Create(),
            studentId,
            feeHeadId,
            discountType,
            value,
            reason,
            startMonth,
            endMonth,
            approvedByStaffId,
            isActive);

        return entity;
    }

    public async Task UpdateAsync(
        StudentFeeDiscount entity,
        Guid studentId,
        Guid? feeHeadId,
        DiscountType discountType,
        decimal value,
        string reason,
        int? startMonth,
        int? endMonth,
        Guid? approvedByStaffId,
        bool isActive)
    {
        // Check for overlapping discounts
        var exists = await _repo.HasOverlappingDiscountAsync(
            studentId,
            feeHeadId,
            startMonth,
            endMonth,
            exceptId: entity.Id);

        if (exists)
        {
            throw new UserFriendlyException(
                "A discount already exists for this student and fee head with overlapping month range.");
        }

        SetStudentId(entity, studentId);
        SetFeeHeadId(entity, feeHeadId);
        entity.ChangeDiscountTypeAndValue(discountType, value);
        entity.ChangeMonthRange(startMonth, endMonth);
        entity.ChangeReason(reason);

        if (approvedByStaffId.HasValue)
        {
            entity.Approve(approvedByStaffId.Value);
        }

        if (isActive)
            entity.Activate();
        else
            entity.Deactivate();
    }

    private void SetStudentId(StudentFeeDiscount entity, Guid studentId)
    {
        if (studentId == Guid.Empty)
            throw new UserFriendlyException("StudentId is required.");

        var prop = typeof(StudentFeeDiscount).GetProperty(nameof(StudentFeeDiscount.StudentId));
        prop!.SetValue(entity, studentId);
    }

    private void SetFeeHeadId(StudentFeeDiscount entity, Guid? feeHeadId)
    {
        var prop = typeof(StudentFeeDiscount).GetProperty(nameof(StudentFeeDiscount.FeeHeadId));
        prop!.SetValue(entity, feeHeadId);
    }
}
