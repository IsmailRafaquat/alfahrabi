using EHub.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EHub.FeeModule.StudentFeeProfiles;

public class EfCoreStudentFeeProfileRepository
    : EfCoreRepository<EHubDbContext, StudentFeeProfile, Guid>,
      IStudentFeeProfileRepository
{
    public EfCoreStudentFeeProfileRepository(IDbContextProvider<EHubDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<StudentFeeProfile?> FindActiveByStudentAsync(Guid studentId)
    {
        var db = await GetDbContextAsync();
        return await db.Set<StudentFeeProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.StudentId == studentId && x.IsActive);
    }

    public async Task<StudentFeeProfile?> GetByIdAsync(Guid id)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<long> GetCountAsync(
        string? filter,
        Guid? studentId,
        Guid? feeStructureId,
        bool? isActive,
        DateTime? effectiveFrom,
        DateTime? effectiveTo)
    {
        var data = await GetFiltersAsync(filter, studentId, feeStructureId, isActive, effectiveFrom, effectiveTo);
        return await data.LongCountAsync();
    }

    public async Task<List<StudentFeeProfile>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filter,
        Guid? studentId,
        Guid? feeStructureId,
        bool? isActive,
        DateTime? effectiveFrom,
        DateTime? effectiveTo)
    {
        var data = await GetFiltersAsync(filter, studentId, feeStructureId, isActive, effectiveFrom, effectiveTo);

        return await data
            .OrderBy(sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync();
    }

    private async Task<IQueryable<StudentFeeProfile>> GetFiltersAsync(
        string? filter,
        Guid? studentId,
        Guid? feeStructureId,
        bool? isActive,
        DateTime? effectiveFrom,
        DateTime? effectiveTo)
    {
        var queryable = await GetQueryableAsync();

        var query = queryable.AsQueryable()
            .WhereIf(!string.IsNullOrWhiteSpace(filter),
                x => x.StudentId.ToString().Contains(filter!)
                  || x.FeeStructureId.ToString().Contains(filter!))
            .WhereIf(studentId.HasValue, x => x.StudentId == studentId)
            .WhereIf(feeStructureId.HasValue, x => x.FeeStructureId == feeStructureId)
            .WhereIf(isActive.HasValue, x => x.IsActive == isActive)
            .WhereIf(effectiveFrom.HasValue, x => x.EffectiveFrom >= effectiveFrom!.Value)
            .WhereIf(effectiveTo.HasValue, x => x.EffectiveFrom <= effectiveTo!.Value)
        ;

        return query;
    }

    public async Task<List<StudentFeeProfile>> GetActiveProfilesByStudentAsync(Guid studentId, Guid? exceptId = null)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => x.StudentId == studentId && x.IsActive)
            .WhereIf(exceptId.HasValue, x => x.Id != exceptId!.Value)
            .ToListAsync();
    }

}
