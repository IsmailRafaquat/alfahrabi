using EHub.AttendanceStatuss;
using System;

namespace EHub.StudentAttendances;

public class MarkStudentAttendanceDto
{
    public Guid StudentId { get; set; }
    public DateTime AttendanceDate { get; set; } // expected date-only from UI
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
}
