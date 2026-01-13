using EHub.AttendanceStatuss;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EHub.StudentAttendances;

public interface IStudentAttendanceRepository : IRepository<StudentAttendance, Guid>
{
    Task<StudentAttendance?> FindAsync(Guid studentId, DateTime attendanceDate);

    Task<long> GetCountAsync(
        string? filter,
        Guid? studentId,
        DateTime? dateFrom,
        DateTime? dateTo,
        AttendanceStatus? status,
        string? firstName,
        string? lastName,
        string? admissionNo);

    Task<List<StudentAttendance>> GetListAsync(
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
        string? admissionNo
        );
}

