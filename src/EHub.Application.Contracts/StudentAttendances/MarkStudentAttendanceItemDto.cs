using EHub.AttendanceStatuss;
using System;

namespace EHub.StudentAttendances;

public class MarkStudentAttendanceItemDto
{
    public Guid StudentId { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
}
