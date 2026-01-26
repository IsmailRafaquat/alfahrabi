using EHub.AttendanceStatuss;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace EHub.StaffAttendances;

public class StaffAttendanceManager : DomainService
{
    private readonly IStaffAttendanceRepository _repo;

    public StaffAttendanceManager(IStaffAttendanceRepository repo, ICurrentTenant currentTenant)
    {
        _repo = repo;
    }

    public async Task<StaffAttendance> MarkAsync(
        Guid staffId,
        DateTime date,
        AttendanceStatus status,
        string? remarks = null)
    {
        Check.NotNull(staffId, nameof(staffId));

        var normalized = NormalizeDate(date);

        var existing = await _repo.FindAsync(staffId, normalized);

        if (existing != null)
        {
            existing.ChangeStatus(status, remarks);
            return await _repo.UpdateAsync(existing, autoSave: true);
        }

        var entity = new StaffAttendance(
            GuidGenerator.Create(),
            staffId,
            normalized,
            status,
            remarks);

        return await _repo.InsertAsync(entity, autoSave: true);
    }

    private static DateTime NormalizeDate(DateTime d)
        => new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Unspecified);
}
