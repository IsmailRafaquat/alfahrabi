using EHub.AttendanceStatuss;
using EHub.Staffs;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.StaffAttendances;

public class StaffAttendance : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid StaffId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }

    public virtual Staff Staff { get; set; } = default!;

    private StaffAttendance() { }

    internal StaffAttendance(
        Guid id,
        Guid staffId,
        DateTime attendanceDate,
        AttendanceStatus status,
        string? remarks = null
    ) : base(id)
    {
        StaffId = Check.NotNull(staffId, nameof(staffId));
        AttendanceDate = NormalizeDate(attendanceDate);

        Status = status;
        Remarks = remarks;
    }

    internal void ChangeStatus(AttendanceStatus status, string? remarks)
    {
        Status = status;
        Remarks = remarks;
    }

    private static DateTime NormalizeDate(DateTime date)
        => new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
}
