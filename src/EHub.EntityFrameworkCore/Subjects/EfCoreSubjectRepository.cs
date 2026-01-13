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

namespace EHub.Subjects;

public class EfCoreSubjectRepository : EfCoreRepository<EHubDbContext, Subject, Guid>, ISubjectRepository
{
    public EfCoreSubjectRepository(IDbContextProvider<EHubDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }


    public async Task<Subject?> FindByCodeAsync(Guid? tenantId, string code)
    {
        using (CurrentTenant.Change(tenantId))
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.FirstOrDefaultAsync(x => x.Code == code);
        }
    }

    public async Task<Subject?> GetLastCreatedSubjectAsync(Guid? tenantId)
    {
        using (CurrentTenant.Change(tenantId))
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefaultAsync();
        }
    }

    public async Task<long> GetCountAsync(
        string? filter,
        string? code,
        string? name,
        GradeLevel? gradeLevel,
        bool? isActive)
    {
        var data = await GetFiltersAsync(filter, code, name, gradeLevel, isActive);
        return await data.LongCountAsync();
    }

    public async Task<List<Subject>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filter,
        string? code,
        string? name,
        GradeLevel? gradeLevel,
        bool? isActive)
    {
        var data = await GetFiltersAsync(filter, code, name, gradeLevel, isActive);
        return await data
            .OrderBy(sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync();
    }

    public async Task<IQueryable<Subject>> GetFiltersAsync(
        string? filter,
        string? code,
        string? name,
        GradeLevel? gradeLevel,
        bool? isActive)
    {
        var queryable = await GetQueryableAsync();

        var query = queryable.AsQueryable()
            .WhereIf(!string.IsNullOrWhiteSpace(filter),
                x => x.Code.ToLower().Contains(filter!.ToLower())
                  || x.Name.ToLower().Contains(filter!.ToLower())
                  || (x.ShortName != null && x.ShortName.ToLower().Contains(filter!.ToLower())))
            .WhereIf(!string.IsNullOrWhiteSpace(code), x => x.Code.ToLower().Contains(code!.ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(name), x => x.Name.ToLower().Contains(name!.ToLower()))
            .WhereIf(gradeLevel.HasValue, x => x.GradeLevel == gradeLevel)
            .WhereIf(isActive.HasValue, x => x.IsActive == isActive);

        return query;
    }

    public async Task<Subject?> FindByNameAndGradeAsync(Guid? tenantId, string name, GradeLevel? gradeLevel)
    {
       using (CurrentTenant.Change(tenantId))
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.FirstOrDefaultAsync(x => x.Name == name && x.GradeLevel == gradeLevel);
        }
    }
}
