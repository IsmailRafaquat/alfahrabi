using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace EHub.FeeModule.StudentFeeProfiles;

public class StudentFeeProfileManager : DomainService
{
    private readonly IStudentFeeProfileRepository _repo;

    public StudentFeeProfileManager(IStudentFeeProfileRepository repo)
    {
        _repo = repo;
    }

    public async Task<StudentFeeProfile> CreateAsync(
        Guid studentId,
        Guid feeStructureId,
        DateTime effectiveFrom,
        DateTime? effectiveTo,
        bool isActive)
    {
        ValidateDateRange(effectiveFrom, effectiveTo);

        var exists = await _repo.AnyAsync(x =>
            x.StudentId == studentId &&
            x.FeeStructureId == feeStructureId &&
            x.EffectiveFrom == effectiveFrom.Date);

        if (exists)
        {
            throw new UserFriendlyException("A fee profile already exists for this student with the same fee structure and effective date.");
        }

        if (isActive)
        {
            await DeactivateOtherProfilesAsync(studentId, exceptId: null);
        }



        var entity = new StudentFeeProfile(
            GuidGenerator.Create(),
            studentId,
            feeStructureId,
            effectiveFrom,
            effectiveTo,
            isActive);


        return entity;
    }

    public async Task UpdateAsync(
        StudentFeeProfile entity,
        Guid studentId,
        Guid feeStructureId,
        DateTime effectiveFrom,
        DateTime? effectiveTo,
        bool isActive)
    {
        ValidateDateRange(effectiveFrom, effectiveTo);

        if (isActive)
        {
            await DeactivateOtherProfilesAsync(studentId, exceptId: entity.Id);
        }

        SetStudentId(entity, studentId);

        entity.ChangeFeeStructure(feeStructureId);
        entity.ChangeEffectivePeriod(effectiveFrom, effectiveTo);

        if (isActive) entity.Activate();
        else entity.Deactivate();
    }

    private async Task DeactivateOtherProfilesAsync(Guid studentId, Guid? exceptId)
    {
        var others = await _repo.GetActiveProfilesByStudentAsync(studentId, exceptId);

        if (!others.Any()) return;

        foreach (var p in others)
        {
            p.Deactivate();
        }
    }

    private void ValidateDateRange(DateTime from, DateTime? to)
    {
        if (to.HasValue && to.Value.Date < from.Date)
            throw new UserFriendlyException("Invalid date range. EffectiveTo must be >= EffectiveFrom.");
    }

    private void SetStudentId(StudentFeeProfile entity, Guid studentId)
    {
        if (studentId == Guid.Empty) throw new UserFriendlyException("StudentId is required.");

        var prop = typeof(StudentFeeProfile).GetProperty(nameof(StudentFeeProfile.StudentId));
        prop!.SetValue(entity, studentId);
    }
}
