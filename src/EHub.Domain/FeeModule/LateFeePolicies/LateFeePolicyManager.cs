using EHub.Students;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace EHub.FeeModule.LateFeePolicies;

public class LateFeePolicyManager : DomainService
{
    private readonly ILateFeePolicyRepository _repo;

    public LateFeePolicyManager(ILateFeePolicyRepository repo)
    {
        _repo = repo;
    }

    public async Task<LateFeePolicy> CreateAsync(
        int graceDays,
        LateFeeType type,
        decimal value,
        GradeLevel? gradeLevel,
        Section? section,
        Shift? shift,
        Term? term,
        bool isActive)
    {
        var exists = await _repo.HasPolicyWithCriteriaAsync(
            gradeLevel,
            section,
            shift,
            term,
            exceptId: null);

        if (exists)
        {
            throw new UserFriendlyException(
                "A late fee policy already exists with the same criteria (GradeLevel, Section, Shift, Term).");
        }

        // If creating an active global policy, deactivate other global policies
        if (isActive && IsGlobalPolicy(gradeLevel, section, shift, term))
        {
            await DeactivateGlobalPoliciesAsync(exceptId: null);
        }

        var entity = new LateFeePolicy(
            GuidGenerator.Create(),
            graceDays,
            type,
            value,
            gradeLevel,
            section,
            shift,
            term,
            isActive);

        return entity;
    }

    public async Task UpdateAsync(
        LateFeePolicy entity,
        int graceDays,
        LateFeeType type,
        decimal value,
        GradeLevel? gradeLevel,
        Section? section,
        Shift? shift,
        Term? term,
        bool isActive)
    {
        // Check for duplicate policy
        var exists = await _repo.HasPolicyWithCriteriaAsync(
            gradeLevel,
            section,
            shift,
            term,
            exceptId: entity.Id);

        if (exists)
        {
            throw new UserFriendlyException(
                "A late fee policy already exists with the same criteria (GradeLevel, Section, Shift, Term).");
        }

        // If activating a global policy, deactivate other global policies
        if (isActive && IsGlobalPolicy(gradeLevel, section, shift, term))
        {
            await DeactivateGlobalPoliciesAsync(exceptId: entity.Id);
        }

        entity.ChangeGradeLevel(gradeLevel);
        entity.ChangeSection(section);
        entity.ChangeShift(shift);
        entity.ChangeTerm(term);
        entity.ChangeGraceDays(graceDays);
        entity.ChangeTypeAndValue(type, value);

        if (isActive)
            entity.Activate();
        else
            entity.Deactivate();
    }

    private async Task DeactivateGlobalPoliciesAsync(Guid? exceptId)
    {
        var globalPolicies = await _repo.GetGlobalActivePoliciesAsync(exceptId);
        if (!globalPolicies.Any()) return;

        foreach (var p in globalPolicies)
        {
            p.Deactivate();
        }
    }

    private bool IsGlobalPolicy(GradeLevel? gradeLevel, Section? section, Shift? shift, Term? term)
    {
        return !gradeLevel.HasValue &&
               !section.HasValue &&
               !shift.HasValue &&
               !term.HasValue;
    }
}