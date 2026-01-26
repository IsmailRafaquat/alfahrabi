using EHub.AttendanceStatuss;
using System;

namespace EHub.StaffAttendances;

public class MarkStaffAttendanceItemDto
{
    public Guid StaffId { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
}
