using System;

namespace EHub.StaffAttendances;

public class BulkMarkStaffAttendanceDto
{
    public DateTime AttendanceDate { get; set; }
    public MarkStaffAttendanceItemDto[] Items { get; set; } = [];
}
