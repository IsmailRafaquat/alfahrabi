using EHub.FeeModule.FeeStructures;
using EHub.Students;
using System;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.FeeModule.StudentFeeProfiles;

[Audited]
public class StudentFeeProfile : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid StudentId { get; private set; }
    public virtual Student Student { get; private set; } = default!;
    public Guid FeeStructureId { get; private set; }
    public virtual FeeStructure FeeStructure { get; private set; } = default!;

    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }

    public bool IsActive { get; private set; } = true;

    private StudentFeeProfile()
    {
        // ORM
    }

    public StudentFeeProfile(
        Guid id,
        Guid studentId,
        Guid feeStructureId,
        DateTime effectiveFrom,
        DateTime? effectiveTo,
        bool isActive = true
    ) : base(id)
    {
        SetStudentId(studentId);
        SetFeeStructureId(feeStructureId);
        SetEffectivePeriod(effectiveFrom, effectiveTo);
        IsActive = isActive;
    }

    public StudentFeeProfile ChangeFeeStructure(Guid feeStructureId)
    {
        SetFeeStructureId(feeStructureId);
        return this;
    }

    public StudentFeeProfile ChangeEffectivePeriod(DateTime from, DateTime? to)
    {
        SetEffectivePeriod(from, to);
        return this;
    }

    public StudentFeeProfile Activate()
    {
        IsActive = true;
        return this;
    }

    public StudentFeeProfile Deactivate()
    {
        IsActive = false;
        return this;
    }

    private void SetStudentId(Guid studentId)
    {
        if (studentId == Guid.Empty) throw new UserFriendlyException("StudentId is required.");
        StudentId = studentId;
    }

    private void SetFeeStructureId(Guid feeStructureId)
    {
        if (feeStructureId == Guid.Empty) throw new UserFriendlyException("FeeStructureId is required.");
        FeeStructureId = feeStructureId;
    }

    private void SetEffectivePeriod(DateTime from, DateTime? to)
    {
        EffectiveFrom = from.Date;

        if (to.HasValue)
        {
            var t = to.Value.Date;
            if (t < EffectiveFrom)
                throw new UserFriendlyException("Invalid date range. EffectiveTo must be >= EffectiveFrom.");
            EffectiveTo = t;
        }
        else
        {
            EffectiveTo = null;
        }
    }
}
