using EHub.FeeModule.FeeStructureItems;
using EHub.Students;
using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.FeeModule.FeeStructures;

[Audited]
public class FeeStructure : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public GradeLevel GradeLevel { get; private set; }
    public Shift Shift { get; private set; }
    public Term Term { get; private set; }

    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }

    public bool IsActive { get; private set; } = true;

    //public virtual ICollection<FeeStructureItem> FeeStructureItems { get; set; } = new List<FeeStructureItem>();

    private FeeStructure()
    {
    }

    public FeeStructure(
        Guid id,
        GradeLevel gradeLevel,
        Shift shift,
        Term term,
        DateTime effectiveFrom,
        DateTime? effectiveTo,
        bool isActive = true
    ) : base(id)
    {
        GradeLevel = gradeLevel;
        Shift = shift;
        Term = term;

        SetEffectivePeriod(effectiveFrom, effectiveTo);

        IsActive = isActive;
    }

    public FeeStructure ChangeCore(
        GradeLevel gradeLevel,
        Shift shift,
        Term term
    )
    {
        GradeLevel = gradeLevel;
        Shift = shift;
        Term = term;
        return this;
    }

    public FeeStructure ChangeEffectivePeriod(DateTime effectiveFrom, DateTime? effectiveTo)
    {
        SetEffectivePeriod(effectiveFrom, effectiveTo);
        return this;
    }

    public FeeStructure Activate()
    {
        IsActive = true;
        return this;
    }

    public FeeStructure Deactivate()
    {
        IsActive = false;
        return this;
    }

    private void SetEffectivePeriod(DateTime from, DateTime? to)
    {
        EffectiveFrom = from.Date;

        if (to.HasValue)
        {
            var t = to.Value.Date;

            if (t < EffectiveFrom)
            {
                throw new UserFriendlyException(
                    "Invalid date range. Effective To must be greater than or equal to Effective From."
                );
            }

            EffectiveTo = t;
        }
        else
        {
            EffectiveTo = null;
        }
    }
}
