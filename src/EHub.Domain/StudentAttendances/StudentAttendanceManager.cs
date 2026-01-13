using EHub.AttendanceStatuss;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace EHub.StudentAttendances;

public class StudentAttendanceManager : DomainService
{
    private readonly IStudentAttendanceRepository _repo;

    public StudentAttendanceManager(IStudentAttendanceRepository repo, ICurrentTenant currentTenant)
    {
        _repo = repo;
    }

    public async Task<StudentAttendance> MarkAsync(
        Guid studentId,
        DateTime date,
        AttendanceStatus status,
        string? remarks = null)
    {
        Check.NotNull(studentId, nameof(studentId));

        var normalized = NormalizeDate(date);

        var existing = await _repo.FindAsync(studentId, normalized);

        if (existing != null)
        {
            existing.ChangeStatus(status, remarks);
            return await _repo.UpdateAsync(existing, autoSave: true);
        }

        var entity = new StudentAttendance(
            GuidGenerator.Create(),
            studentId,
            normalized,
            status,
            remarks);

        return await _repo.InsertAsync(entity, autoSave: true);
    }

    private static DateTime NormalizeDate(DateTime d)
        => new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Unspecified);
}
