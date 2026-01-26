using EHub.AttendanceStatuss;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EHub.StaffAttendances;

public interface IStaffAttendanceRepository : IRepository<StaffAttendance, Guid>
{
    Task<StaffAttendance?> FindAsync(Guid staffId, DateTime attendanceDate);

    Task<long> GetCountAsync(
        string? filter,
        Guid? staffId,
        DateTime? dateFrom,
        DateTime? dateTo,
        AttendanceStatus? status,
        string? firstName,
        string? lastName,
        string? employeeCode);

    Task<List<StaffAttendance>> GetListAsync(
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
        string? employeeCode);
}
