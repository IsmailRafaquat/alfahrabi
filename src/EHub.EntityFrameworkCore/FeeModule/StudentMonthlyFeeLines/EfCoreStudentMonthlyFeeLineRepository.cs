using EHub.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

public class EfCoreStudentMonthlyFeeLineRepository
    : EfCoreRepository<EHubDbContext, StudentMonthlyFeeLine, Guid>,
      IStudentMonthlyFeeLineRepository
{
    public EfCoreStudentMonthlyFeeLineRepository(IDbContextProvider<EHubDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<StudentMonthlyFeeLine?> GetByIdAsync(Guid id)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<StudentMonthlyFeeLine>> GetByMonthlyFeeAsync(Guid studentMonthlyFeeId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.StudentMonthlyFeeId == studentMonthlyFeeId)
            .OrderBy(x => x.CreationTime)
            .ToListAsync();
    }

    public async Task<StudentMonthlyFeeLine?> FindByMonthlyFeeAndHeadAsync(Guid studentMonthlyFeeId, Guid feeHeadId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x =>
            x.StudentMonthlyFeeId == studentMonthlyFeeId &&
            x.FeeHeadId == feeHeadId);
    }

    public async Task<long> GetCountAsync(string? filters, Guid? studentMonthlyFeeId, Guid? feeHeadId)
    {
        var q = await GetQueryableAsync();
        q = q.WhereIf(studentMonthlyFeeId.HasValue, x => x.StudentMonthlyFeeId == studentMonthlyFeeId)
             .WhereIf(feeHeadId.HasValue, x => x.FeeHeadId == feeHeadId);
        return await q.LongCountAsync();
    }

    public async Task<List<StudentMonthlyFeeLine>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filters,
        Guid? studentMonthlyFeeId,
        Guid? feeHeadId)
    {
        var q = await GetQueryableAsync();

        q = q.Include(x => x.StudentMonthlyFee)
                .ThenInclude(x => x.Student)
                 .WhereIf(studentMonthlyFeeId.HasValue, x => x.StudentMonthlyFeeId == studentMonthlyFeeId)
                 .WhereIf(feeHeadId.HasValue, x => x.FeeHeadId == feeHeadId)
                 .WhereIf(!filters.IsNullOrWhiteSpace(), x =>
                    (x.StudentMonthlyFee != null &&
                     x.StudentMonthlyFee.Student != null) &&
                    (
                        ((x.StudentMonthlyFee.Student.FirstName ?? "").ToLower().Contains(filters!.ToLower())) ||
                        ((x.StudentMonthlyFee.Student.LastName ?? "").ToLower().Contains(filters!.ToLower())) ||
                        ((x.StudentMonthlyFee.Student.AdmissionNo ?? "").ToLower().Contains(filters!.ToLower())) ||
                        ((x.StudentMonthlyFee.Student.AdmissionNo ?? "").ToLower().Contains(filters!.ToLower()))
                    )
    );

        return await q.OrderBy(sorting).PageBy(skipCount, maxResultCount).ToListAsync();
    }
}
