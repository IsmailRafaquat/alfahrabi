using EHub.Students;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.Teaching;

public class TeacherSubject : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get;  set; }
    public Guid StaffId { get;  set; }
    public List<Guid> SubjectIds { get;  set; } = new();
    public Section? Section { get;  set; }
    public int? WeeklyHours { get;  set; }
    public DateTime? EffectiveFrom { get;  set; }
    public DateTime? EffectiveTo { get;  set; }
    public bool IsPrimaryTeacher { get;  set; }

    private TeacherSubject()
    {
    }

    internal TeacherSubject(
        Guid id,
        Guid? tenantId,
        Guid staffId,
        List<Guid> subjectIds,
        Section? section,
        int? weeklyHours,
        DateTime? effectiveFrom,
        DateTime? effectiveTo,
        bool isPrimaryTeacher = false
    ) : base(id)
    {
        TenantId = tenantId;
        SetCore(staffId, subjectIds, section, weeklyHours, effectiveFrom, effectiveTo, isPrimaryTeacher);
    }


    internal TeacherSubject ChangeSubjects(List<Guid> subjectIds)
    {
        SubjectIds = ValidateSubjects(subjectIds);
        return this;
    }

    internal TeacherSubject ChangeSection(Section? section)
    {
        Section = section;
        return this;
    }

    internal TeacherSubject ChangeWeeklyHours(int? weeklyHours)
    {
        if (weeklyHours.HasValue)
            Check.Range(weeklyHours.Value, nameof(WeeklyHours), 0, 200);

        WeeklyHours = weeklyHours;
        return this;
    }

    internal TeacherSubject Reschedule(DateTime? effectiveFrom, DateTime? effectiveTo)
    {
        var from = effectiveFrom?.Date;
        var to = effectiveTo?.Date;

        if (from.HasValue && to.HasValue && from > to)
            throw new BusinessException("TeacherSubject.InvalidDateRange")
                .WithData("From", from)
                .WithData("To", to);

        EffectiveFrom = from;
        EffectiveTo = to;
        return this;
    }

    internal TeacherSubject MarkPrimary(bool isPrimary)
    {
        IsPrimaryTeacher = isPrimary;
        return this;
    }

    internal TeacherSubject ChangeStaff(Guid staffId)
    {
        StaffId = Check.NotNull(staffId, nameof(StaffId));
        return this;
    }

    internal TeacherSubject Update(
        List<Guid> subjectIds,
        Section? section,
        int? weeklyHours,
        DateTime? effectiveFrom,
        DateTime? effectiveTo,
        bool isPrimaryTeacher)
    {
        SetCore(StaffId, subjectIds, section, weeklyHours, effectiveFrom, effectiveTo, isPrimaryTeacher);
        return this;
    }

    private void SetCore(
        Guid staffId,
        List<Guid> subjectIds,
        Section? section,
        int? weeklyHours,
        DateTime? effectiveFrom,
        DateTime? effectiveTo,
        bool isPrimaryTeacher)
    {
        StaffId = Check.NotNull(staffId, nameof(StaffId));
        SubjectIds = ValidateSubjects(subjectIds);

        if (weeklyHours.HasValue)
            Check.Range(weeklyHours.Value, nameof(WeeklyHours), 0, 200);

        var from = effectiveFrom?.Date;
        var to = effectiveTo?.Date;

        if (from.HasValue && to.HasValue && from > to)
            throw new BusinessException("TeacherSubject.InvalidDateRange")
                .WithData("From", from)
                .WithData("To", to);

        Section = section;
        WeeklyHours = weeklyHours;
        EffectiveFrom = from;
        EffectiveTo = to;
        IsPrimaryTeacher = isPrimaryTeacher;
    }

    private static List<Guid> ValidateSubjects(List<Guid> subjectIds)
    {
        Check.NotNull(subjectIds, nameof(SubjectIds));
        var cleaned = subjectIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (cleaned.Count == 0)
            throw new BusinessException("TeacherSubject.EmptySubjects");

        return cleaned;
    }
}
