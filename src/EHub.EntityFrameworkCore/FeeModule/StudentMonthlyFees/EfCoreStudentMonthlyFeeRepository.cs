using EHub.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EHub.FeeModule.StudentMonthlyFees;

public class EfCoreStudentMonthlyFeeRepository
    : EfCoreRepository<EHubDbContext, StudentMonthlyFee, Guid>, IStudentMonthlyFeeRepository
{
    public EfCoreStudentMonthlyFeeRepository(IDbContextProvider<EHubDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    private static DateTime NormalizeMonth(DateTime dt) => new(dt.Year, dt.Month, 1);

    public async Task<StudentMonthlyFee?> FindByStudentAndMonthAsync(Guid studentId, DateTime month)
    {
        var dbSet = await GetDbSetAsync();
        var m = NormalizeMonth(month);

        return await dbSet.AsNoTracking()
            .FirstOrDefaultAsync(x => x.StudentId == studentId && x.Month == m);
    }

    public async Task<StudentMonthlyFee?> GetByIdAsync(Guid id)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsAsync(Guid studentId, DateTime month, Guid? exceptId = null)
    {
        var dbSet = await GetDbSetAsync();
        var m = NormalizeMonth(month);

        return await dbSet.AnyAsync(x =>
            x.StudentId == studentId &&
            x.Month == m &&
            (!exceptId.HasValue || x.Id != exceptId.Value));
    }

    public async Task<long> GetCountAsync(string? filter, Guid? studentId, DateTime? month, DateTime? collectedOn)
    {
        var q = await GetFiltersAsync(filter, studentId, month, collectedOn);
        return await q.LongCountAsync();
    }

    public async Task<List<StudentMonthlyFee>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filter,
        Guid? studentId,
        DateTime? month,
        DateTime? collectedOn)
    {
        var q = await GetFiltersAsync(filter, studentId, month, collectedOn);

        return await q
            .OrderBy(sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync();
    }

    private async Task<IQueryable<StudentMonthlyFee>> GetFiltersAsync(string? filter, Guid? studentId, DateTime? month, DateTime? collectedOn)
    {
        var q = (await GetQueryableAsync()).AsNoTracking();

        if (month.HasValue)
        {
            var m = NormalizeMonth(month.Value);
            q = q.Where(x => x.Month == m);
        }

        if(collectedOn.HasValue)
        {
            var selectedDate = collectedOn.Value.Date;
            var nextDate = selectedDate.AddDays(1);

            q = q.Where(x => x.CreationTime >= selectedDate && x.CreationTime < nextDate);
        }

        var f = filter?.Trim();

        q = q.Include(x => x.Student)
            .WhereIf(!f.IsNullOrWhiteSpace(), x=> x.Student.FirstName.Trim().ToLower().Contains(f!.ToLower())
                || x.Student.LastName.Trim().ToLower().Contains(f!.ToLower())
                || x.Student.AdmissionNo.Trim().ToLower().Contains(f!.ToLower()))
            .WhereIf(studentId.HasValue, x => x.StudentId == studentId)
            .WhereIf(!string.IsNullOrWhiteSpace(filter),
                x => x.StudentId.ToString().Contains(filter!));

        return q;
    }
}
