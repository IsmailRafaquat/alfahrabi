using EHub.Students;
using System;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.FeeModule.LateFeePolicies;

[Audited]
public class LateFeePolicy : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public GradeLevel? GradeLevel { get; private set; }
    public Section? Section { get; private set; }
    public Shift? Shift { get; private set; }
    public Term? Term { get; private set; }

    public int GraceDays { get; private set; }
    public LateFeeType Type { get; private set; }
    public decimal Value { get; private set; }

    public bool IsActive { get; private set; } = true;

    private LateFeePolicy()
    {
        // ORM
    }

    public LateFeePolicy(
        Guid id,
        int graceDays,
        LateFeeType type,
        decimal value,
        GradeLevel? gradeLevel = null,
        Section? section = null,
        Shift? shift = null,
        Term? term = null,
        bool isActive = true
    ) : base(id)
    {
        SetGradeLevel(gradeLevel);
        SetSection(section);
        SetShift(shift);
        SetTerm(term);
        SetGraceDays(graceDays);
        SetTypeAndValue(type, value);
        IsActive = isActive;
    }

    public LateFeePolicy ChangeGradeLevel(GradeLevel? gradeLevel)
    {
        SetGradeLevel(gradeLevel);
        return this;
    }

    public LateFeePolicy ChangeSection(Section? section)
    {
        SetSection(section);
        return this;
    }

    public LateFeePolicy ChangeShift(Shift? shift)
    {
        SetShift(shift);
        return this;
    }

    public LateFeePolicy ChangeTerm(Term? term)
    {
        SetTerm(term);
        return this;
    }

    public LateFeePolicy ChangeGraceDays(int graceDays)
    {
        SetGraceDays(graceDays);
        return this;
    }

    public LateFeePolicy ChangeTypeAndValue(LateFeeType type, decimal value)
    {
        SetTypeAndValue(type, value);
        return this;
    }

    public LateFeePolicy Activate()
    {
        IsActive = true;
        return this;
    }

    public LateFeePolicy Deactivate()
    {
        IsActive = false;
        return this;
    }

    private void SetGradeLevel(GradeLevel? gradeLevel)
    {
        GradeLevel = gradeLevel;
    }

    private void SetSection(Section? section)
    {
        Section = section;
    }

    private void SetShift(Shift? shift)
    {
        Shift = shift;
    }

    private void SetTerm(Term? term)
    {
        Term = term;
    }

    private void SetGraceDays(int graceDays)
    {
        if (graceDays < 0)
            throw new UserFriendlyException("Grace days cannot be negative.");

        GraceDays = graceDays;
    }

    private void SetTypeAndValue(LateFeeType type, decimal value)
    {
        if (value < 0)
            throw new UserFriendlyException("Late fee value cannot be negative.");

        Type = type;
        Value = value;
    }

    /// <summary>
    /// Checks if this policy is global (applies to all grades/sections/shifts/terms)
    /// </summary>
    public bool IsGlobal()
    {
        return !GradeLevel.HasValue &&
               !Section.HasValue &&
               !Shift.HasValue &&
               !Term.HasValue;
    }

    /// <summary>
    /// Checks if this policy matches the given criteria
    /// </summary>
    public bool Matches(GradeLevel? gradeLevel, Section? section, Shift? shift, Term? term)
    {
        // If global, it matches everything
        if (IsGlobal()) return true;

        // Otherwise, all specified criteria must match
        if (GradeLevel.HasValue && GradeLevel != gradeLevel) return false;
        if (Section.HasValue && Section != section) return false;
        if (Shift.HasValue && Shift != shift) return false;
        if (Term.HasValue && Term != term) return false;

        return true;
    }
}
