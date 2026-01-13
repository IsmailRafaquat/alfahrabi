using System;

namespace EHub.StudentAttendances;

public class BulkMarkStudentAttendanceDto
{
    public DateTime AttendanceDate { get; set; }
    public MarkStudentAttendanceItemDto[] Items { get; set; } = [];
}
