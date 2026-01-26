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

namespace EHub.Staffs;

public class EfCoreStaffRepository : EfCoreRepository<EHubDbContext, Staff, Guid>, IStaffRepository
{
    public EfCoreStaffRepository(IDbContextProvider<EHubDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<Staff?> FindByEmployeeCodeAsync(string employeeCode)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.EmployeeCode == employeeCode);
    }

    public async Task<Staff?> GetLastCreatedStaffAsync(Guid? tenantId)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.AsQueryable();

        if (tenantId.HasValue)
            query = query.Where(x => x.TenantId == tenantId);

        return await query
            .OrderByDescending(x => x.CreationTime)
            .FirstOrDefaultAsync();
    }

    public async Task<long> GetCountAsync(
        string? filter,
        string? firstName,
        string? lastName,
        Department? department,
        JobStatus? jobStatus,
        Shift? shift)
    {
        var data = await GetFiltersAsync(filter, firstName, lastName, department, jobStatus, shift);
        return await data.LongCountAsync();
    }

    public async Task<List<Staff>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filter,
        string? firstName,
        string? lastName,
        Department? department,
        JobStatus? jobStatus,
        Shift? shift)
    {
        var data = await GetFiltersAsync(filter, firstName, lastName, department, jobStatus, shift);
        return await data.OrderBy(sorting).PageBy(skipCount, maxResultCount).ToListAsync();
    }

    private async Task<IQueryable<Staff>> GetFiltersAsync(
        string? filter,
        string? firstName,
        string? lastName,
        Department? department,
        JobStatus? jobStatus,
        Shift? shift)
    {
        var queryable = await GetQueryableAsync();

        var query = queryable.AsQueryable()
            .WhereIf(!string.IsNullOrWhiteSpace(filter),
                x => x.FirstName.ToLower().Contains(filter!.ToLower())
                  || x.LastName.ToLower().Contains(filter!.ToLower())
                  || x.EmployeeCode.ToLower().Contains(filter!.ToLower())
                  || x.PhoneNo.ToLower().Contains(filter!.ToLower())
                  || x.Email!.ToLower().Contains(filter!.ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(firstName),
                x => x.FirstName.ToLower().Contains(firstName!.ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(lastName),
                x => x.LastName.ToLower().Contains(lastName!.ToLower()))
            .WhereIf(department.HasValue, x => x.Department == department)
            .WhereIf(jobStatus.HasValue, x => x.JobStatus == jobStatus)
            .WhereIf(shift.HasValue, x => x.WorkShift == shift);

        return query;
    }

    public async Task<Staff?> GetByIdAsync(Guid id)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Include(x => x.StaffDocuments).ThenInclude(x => x.FileAttachments)
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Staff>> GetStaffLookupAsync()
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .OrderBy(x => x.FirstName)
            .ToListAsync();
    }
}
