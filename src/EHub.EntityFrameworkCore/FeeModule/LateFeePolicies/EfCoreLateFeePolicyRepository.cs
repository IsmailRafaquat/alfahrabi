using EHub.EntityFrameworkCore;
using EHub.Students;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EHub.FeeModule.LateFeePolicies;

public class EfCoreLateFeePolicyRepository
    : EfCoreRepository<EHubDbContext, LateFeePolicy, Guid>,
      ILateFeePolicyRepository
{
    public EfCoreLateFeePolicyRepository(IDbContextProvider<EHubDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<LateFeePolicy?> GetByIdAsync(Guid id)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<long> GetCountAsync(
        string? filter,
        GradeLevel? gradeLevel,
        Section? section,
        Shift? shift,
        Term? term,
        LateFeeType? type,
        bool? isActive,
        bool? isGlobal)
    {
        var data = await GetFiltersAsync(filter, gradeLevel, section, shift, term, type, isActive, isGlobal);
        return await data.LongCountAsync();
    }

    public async Task<List<LateFeePolicy>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filter,
        GradeLevel? gradeLevel,
        Section? section,
        Shift? shift,
        Term? term,
        LateFeeType? type,
        bool? isActive,
        bool? isGlobal)
    {
        var data = await GetFiltersAsync(filter, gradeLevel, section, shift, term, type, isActive, isGlobal);
        return await data
            .OrderBy(sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync();
    }

    private async Task<IQueryable<LateFeePolicy>> GetFiltersAsync(
        string? filter,
        GradeLevel? gradeLevel,
        Section? section,
        Shift? shift,
        Term? term,
        LateFeeType? type,
        bool? isActive,
        bool? isGlobal)
    {
        var queryable = await GetQueryableAsync();
        var query = queryable.AsQueryable()
            .WhereIf(gradeLevel.HasValue, x => x.GradeLevel == gradeLevel)
            .WhereIf(section.HasValue, x => x.Section == section)
            .WhereIf(shift.HasValue, x => x.Shift == shift)
            .WhereIf(term.HasValue, x => x.Term == term)
            .WhereIf(type.HasValue, x => x.Type == type)
            .WhereIf(isActive.HasValue, x => x.IsActive == isActive);

        // Filter for global policies
        if (isGlobal.HasValue)
        {
            if (isGlobal.Value)
            {
                query = query.Where(x =>
                    !x.GradeLevel.HasValue &&
                    !x.Section.HasValue &&
                    !x.Shift.HasValue &&
                    !x.Term.HasValue);
            }
            else
            {
                query = query.Where(x =>
                    x.GradeLevel.HasValue ||
                    x.Section.HasValue ||
                    x.Shift.HasValue ||
                    x.Term.HasValue);
            }
        }

        return query;
    }

    public async Task<bool> HasPolicyWithCriteriaAsync(
        GradeLevel? gradeLevel,
        Section? section,
        Shift? shift,
        Term? term,
        Guid? exceptId = null)
    {
        var dbSet = await GetDbSetAsync();

        var query = dbSet
            .Where(x =>
                x.GradeLevel == gradeLevel &&
                x.Section == section &&
                x.Shift == shift &&
                x.Term == term)
            .WhereIf(exceptId.HasValue, x => x.Id != exceptId!.Value);

        return await query.AnyAsync();
    }

    public async Task<List<LateFeePolicy>> GetGlobalActivePoliciesAsync(Guid? exceptId = null)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x =>
                x.IsActive &&
                !x.GradeLevel.HasValue &&
                !x.Section.HasValue &&
                !x.Shift.HasValue &&
                !x.Term.HasValue)
            .WhereIf(exceptId.HasValue, x => x.Id != exceptId!.Value)
            .ToListAsync();
    }

    public async Task<LateFeePolicy?> FindApplicablePolicyAsync(
        int? gradeLevel,
        int? section,
        int? shift,
        int? term)
    {
        var dbSet = await GetDbSetAsync();

        // Convert int? to enum? for comparison
        GradeLevel? gradeLevelEnum = gradeLevel.HasValue ? (GradeLevel)gradeLevel.Value : null;
        Section? sectionEnum = section.HasValue ? (Section)section.Value : null;
        Shift? shiftEnum = shift.HasValue ? (Shift)shift.Value : null;
        Term? termEnum = term.HasValue ? (Term)term.Value : null;

        // First, try to find exact match
        var exactMatch = await dbSet
            .Where(x => x.IsActive)
            .Where(x => x.GradeLevel == gradeLevelEnum)
            .Where(x => x.Section == sectionEnum)
            .Where(x => x.Shift == shiftEnum)
            .Where(x => x.Term == termEnum)
            .FirstOrDefaultAsync();

        if (exactMatch != null) return exactMatch;

        // If no exact match, find the most specific applicable policy
        var policies = await dbSet
            .Where(x => x.IsActive)
            .Where(x =>
                (!x.GradeLevel.HasValue || x.GradeLevel == gradeLevelEnum) &&
                (!x.Section.HasValue || x.Section == sectionEnum) &&
                (!x.Shift.HasValue || x.Shift == shiftEnum) &&
                (!x.Term.HasValue || x.Term == termEnum))
            .ToListAsync();

        // Calculate specificity and return the most specific one
        return policies
            .OrderByDescending(p => CalculateSpecificity(p))
            .FirstOrDefault();
    }

    private int CalculateSpecificity(LateFeePolicy policy)
    {
        int score = 0;
        if (policy.GradeLevel.HasValue) score += 8;
        if (policy.Section.HasValue) score += 4;
        if (policy.Shift.HasValue) score += 2;
        if (policy.Term.HasValue) score += 1;
        return score;
    }
}
