using EHub.AttendanceStatuss;
using System;

namespace EHub.StaffAttendances;

public class MarkStaffAttendanceDto
{
    public Guid StaffId { get; set; }
    public DateTime AttendanceDate { get; set; } // expected date-only from UI
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
}
