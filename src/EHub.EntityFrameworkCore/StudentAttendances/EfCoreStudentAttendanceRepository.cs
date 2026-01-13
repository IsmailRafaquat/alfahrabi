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

namespace EHub.StudentAttendances;

public class EfCoreStudentAttendanceRepository : EfCoreRepository<EHubDbContext, StudentAttendance, Guid>, IStudentAttendanceRepository
{
    public EfCoreStudentAttendanceRepository(IDbContextProvider<EHubDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<StudentAttendance?> FindAsync(Guid studentId, DateTime attendanceDate)
    {
        var date = NormalizeDate(attendanceDate);

        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x =>
            x.StudentId == studentId &&
            x.AttendanceDate == date);
    }

    public async Task<long> GetCountAsync(
        string? filter,
        Guid? studentId,
        DateTime? dateFrom,
        DateTime? dateTo,
        AttendanceStatus? status,
        string? firstName,
        string? lastName,
        string? admissionNo
        )
    {
        var q = await GetFiltersAsync(filter, studentId, dateFrom, dateTo, status, firstName, lastName, admissionNo);
        return await q.LongCountAsync();
    }

    public async Task<List<StudentAttendance>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filter,
        Guid? studentId,
        DateTime? dateFrom,
        DateTime? dateTo,
        AttendanceStatus? status,
        string? firstName,
        string? lastName,
        string? admissionNo)
    {
        var q = await GetFiltersAsync(filter, studentId, dateFrom, dateTo, status, firstName, lastName, admissionNo);

        sorting = string.IsNullOrWhiteSpace(sorting) ? "AttendanceDate desc" : sorting;

        return await q
            .OrderBy(sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync();
    }

    public async Task<IQueryable<StudentAttendance>> GetFiltersAsync(
        string? filter,
        Guid? studentId,
        DateTime? dateFrom,
        DateTime? dateTo,
        AttendanceStatus? status,
        string? firstName,
        string? lastName,
        string? admissionNo)
    {
        var queryable = await GetQueryableAsync();

        var query = queryable.AsQueryable()
            .Include(s => s.Students)
        .WhereIf(studentId.HasValue, x => x.Students.Id == studentId)
            .WhereIf(status.HasValue, x => x.Status == status)
            .WhereIf(dateFrom.HasValue, x => x.AttendanceDate >= NormalizeDate(dateFrom!.Value))
            .WhereIf(dateTo.HasValue, x => x.AttendanceDate <= NormalizeDate(dateTo!.Value))
            .WhereIf(!string.IsNullOrWhiteSpace(firstName), x => x.Students.FirstName.ToLower() == firstName!.Trim().ToLower())
            .WhereIf(!string.IsNullOrWhiteSpace(lastName), x => x.Students.LastName.ToLower() == lastName!.Trim().ToLower())
            .WhereIf(!string.IsNullOrWhiteSpace(admissionNo), x => x.Students.AdmissionNo.ToLower() == admissionNo!.Trim().ToLower())
            .WhereIf(!string.IsNullOrWhiteSpace(filter),
                x =>
                    (x.Students.FirstName != null && x.Students.FirstName.ToLower().Contains(filter!.Trim().ToLower())) ||
                    (x.Students.LastName != null && x.Students.LastName.ToLower().Contains(filter!.Trim().ToLower())) ||
                    (x.Students.AdmissionNo != null && x.Students.AdmissionNo.ToLower().Contains(filter!.Trim().ToLower()))
            );

        return query;
    }

    private static DateTime NormalizeDate(DateTime d)
        => new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Unspecified);
}
