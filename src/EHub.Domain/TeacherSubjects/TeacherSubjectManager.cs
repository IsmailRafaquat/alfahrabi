using EHub.Students;
using EHub.TeacherSubjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace EHub.Teaching;

public class TeacherSubjectManager : DomainService
{
    private readonly ITeacherSubjectRepository _teacherSubjectRepository;

    public TeacherSubjectManager(ITeacherSubjectRepository teacherSubjectRepository)
    {
        _teacherSubjectRepository = teacherSubjectRepository;
    }

    public async Task<TeacherSubject> CreateAsync(
        Guid staffId,
        List<Guid> subjectIds,
        Section? section,
        int? weeklyHours,
        DateTime? effectiveFrom,
        DateTime? effectiveTo,
        bool isPrimaryTeacher = false)
    {
        Check.NotNull(staffId, nameof(staffId));
        Check.NotNullOrEmpty(subjectIds, nameof(subjectIds));

        var existing = await _teacherSubjectRepository.FindByStaffAndSectionAsync(staffId, section);
        if (existing != null)
        {
            throw new SubjectAlreadyAssignedException(staffId, section);
        }

        return new TeacherSubject(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            staffId,
            subjectIds,
            section,
            weeklyHours,
            effectiveFrom,
            effectiveTo,
            isPrimaryTeacher
        );
    }

    public async Task ChangeSubjectsAsync(
        TeacherSubject teacherSubject,
        List<Guid> newSubjectIds)
    {
        Check.NotNull(teacherSubject, nameof(teacherSubject));
        Check.NotNullOrEmpty(newSubjectIds, nameof(newSubjectIds));

        teacherSubject.ChangeSubjects(newSubjectIds);
        await Task.CompletedTask;
    }

    public async Task ChangeSectionAsync(
        TeacherSubject teacherSubject,
        Section? newSection)
    {
        Check.NotNull(teacherSubject, nameof(teacherSubject));
        teacherSubject.ChangeSection(newSection);
        await Task.CompletedTask;
    }

    public async Task RescheduleAsync(
        TeacherSubject teacherSubject,
        DateTime? effectiveFrom,
        DateTime? effectiveTo)
    {
        Check.NotNull(teacherSubject, nameof(teacherSubject));
        teacherSubject.Reschedule(effectiveFrom, effectiveTo);
        await Task.CompletedTask;
    }

    public async Task ChangeWeeklyHoursAsync(
        TeacherSubject teacherSubject,
        int? weeklyHours)
    {
        Check.NotNull(teacherSubject, nameof(teacherSubject));
        teacherSubject.ChangeWeeklyHours(weeklyHours);
        await Task.CompletedTask;
    }

    public async Task MarkPrimaryAsync(
        TeacherSubject teacherSubject,
        bool isPrimary)
    {
        Check.NotNull(teacherSubject, nameof(teacherSubject));
        teacherSubject.MarkPrimary(isPrimary);
        await Task.CompletedTask;
    }
}
