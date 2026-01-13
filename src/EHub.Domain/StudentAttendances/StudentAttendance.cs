using EHub.AttendanceStatuss;
using EHub.Students;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.StudentAttendances;

public class StudentAttendance : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }

    public virtual Student Students { get; set; }

    private StudentAttendance() { }

    internal StudentAttendance(
        Guid id,
        Guid studentId,
        DateTime attendanceDate,
        AttendanceStatus status,
        string? remarks = null,
        Guid? markedByStaffId = null
    ) : base(id)
    {
        StudentId = Check.NotNull(studentId, nameof(studentId));
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