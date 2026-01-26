using System;
using System.Collections.Generic;

namespace EHub.StaffAttendances;

public class StaffAttendanceLeaderboardDto
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }

    public int TotalRecords { get; set; }
    public int PresentRecords { get; set; }
    public int AbsentRecords { get; set; }
    public int LateRecords { get; set; }
    public int OtherRecords { get; set; }
    public int ExcusedRecords { get; set; }
    public int SickRecords { get; set; }
    public int LeaveRecords { get; set; }
    public int HolidayRecords { get; set; }

    public List<StaffAttendanceLeaderboardItemDto> Items { get; set; } = new();
}
