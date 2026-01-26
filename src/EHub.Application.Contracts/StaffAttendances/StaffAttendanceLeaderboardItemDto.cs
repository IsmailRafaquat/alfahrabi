using System;

namespace EHub.StaffAttendances;

public class StaffAttendanceLeaderboardItemDto
{
    public Guid StaffId { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;

    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public int ExcusedDays { get; set; }
    public int SickDays { get; set; }
    public int LeaveDays { get; set; }
    public int HolidayDays { get; set; }

    public double AttendanceRate { get; set; } // PresentDays / TotalDays * 100
}
