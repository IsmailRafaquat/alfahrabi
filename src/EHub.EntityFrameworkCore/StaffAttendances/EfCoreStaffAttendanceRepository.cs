using EHub.AttendanceStatuss;
using EHub.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EHub.StaffAttendances;

public class EfCoreStaffAttendanceRepository : EfCoreRepository<EHubDbContext, StaffAttendance, Guid>, IStaffAttendanceRepository
{
    public EfCoreStaffAttendanceRepository(IDbContextProvider<EHubDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<StaffAttendance?> FindAsync(Guid staffId, DateTime attendanceDate)
    {
        var date = NormalizeDate(attendanceDate);

        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x =>
            x.StaffId == staffId &&
            x.AttendanceDate == date);
    }

    public async Task<long> GetCountAsync(
        string? filter,
        Guid? staffId,
        DateTime? dateFrom,
        DateTime? dateTo,
        AttendanceStatus? status,
        string? firstName,
        string? lastName,
        string? employeeCode)
    {
        var q = await GetFiltersAsync(filter, staffId, dateFrom, dateTo, status, firstName, lastName, employeeCode);
        return await q.LongCountAsync();
    }

    public async Task<List<StaffAttendance>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filter,
        Guid? staffId,
        DateTime? dateFrom,
        DateTime? dateTo,
        AttendanceStatus? status,
        string? firstName,
        string? lastName,
        string? employeeCode)
    {
        var q = await GetFiltersAsync(filter, staffId, dateFrom, dateTo, status, firstName, lastName, employeeCode);

        sorting = string.IsNullOrWhiteSpace(sorting) ? "AttendanceDate desc" : sorting;

        return await q
            .OrderBy(sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync();
    }

    public async Task<IQueryable<StaffAttendance>> GetFiltersAsync(
        string? filter,
        Guid? staffId,
        DateTime? dateFrom,
        DateTime? dateTo,
        AttendanceStatus? status,
        string? firstName,
        string? lastName,
        string? employeeCode)
    {
        var queryable = await GetQueryableAsync();

        var query = queryable.AsQueryable()
            .Include(x => x.Staff)
            .WhereIf(staffId.HasValue, x => x.Staff.Id == staffId) // or x.StaffId == staffId
            .WhereIf(status.HasValue, x => x.Status == status)
            .WhereIf(dateFrom.HasValue, x => x.AttendanceDate >= NormalizeDate(dateFrom!.Value))
            .WhereIf(dateTo.HasValue, x => x.AttendanceDate <= NormalizeDate(dateTo!.Value))
            .WhereIf(!string.IsNullOrWhiteSpace(firstName),
                x => x.Staff.FirstName.ToLower() == firstName!.Trim().ToLower())
            .WhereIf(!string.IsNullOrWhiteSpace(lastName),
                x => x.Staff.LastName.ToLower() == lastName!.Trim().ToLower())
            .WhereIf(!string.IsNullOrWhiteSpace(employeeCode),
                x => x.Staff.EmployeeCode.ToLower() == employeeCode!.Trim().ToLower())
            .WhereIf(!string.IsNullOrWhiteSpace(filter),
                x =>
                    (x.Staff.FirstName != null && x.Staff.FirstName.ToLower().Contains(filter!.Trim().ToLower())) ||
                    (x.Staff.LastName != null && x.Staff.LastName.ToLower().Contains(filter!.Trim().ToLower())) ||
                    (x.Staff.EmployeeCode != null && x.Staff.EmployeeCode.ToLower().Contains(filter!.Trim().ToLower()))
            );

        return query;
    }

    private static DateTime NormalizeDate(DateTime d)
        => new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Unspecified);
}
