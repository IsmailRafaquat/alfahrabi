using System;
using System.Collections.Generic;

namespace EHub.Dashboards;

public class DashboardInput
{
    // Frontend can pass either Month OR From/To OR both
    public string? Month { get; set; }      // "yyyy-MM" or date string
    public string? FromDate { get; set; }   // any JS date string
    public string? ToDate { get; set; }

    // Optional filters for attendance slices
    public int? GradeLevel { get; set; }
    public int? Section { get; set; }
    public int? Shift { get; set; }
    public int? Term { get; set; }

    public int? Department { get; set; } // Staff department enum/int if you have it
}

public class DashboardDto
{
    public DashboardKpisDto Kpis { get; set; } = new();

    public List<TimePointDto> ExpenseTrend { get; set; } = new();
    public List<TimePointDto> EarnTrend { get; set; } = new();

    public AttendanceStatusDistributionDto StaffStatus { get; set; } = new();
    public AttendanceStatusDistributionDto StudentStatus { get; set; } = new();

    public List<AttendanceLeaderboardItemDto> TopStaff { get; set; } = new();
    public List<AttendanceLeaderboardItemDto> BottomStaff { get; set; } = new();

    public List<ClassAttendanceDto> StudentAttendanceByClass { get; set; } = new();
    public List<StudentTopDto> TopStudentsOverall { get; set; } = new(); // Top 3 across all classes
    public List<TimePointByCategoryDto> ExpenseTrendByCategory { get; set; } = new();
    public List<TimePointByNameDto> SalaryTrendByStaff { get; set; } = new();

}

public class DashboardKpisDto
{
    public decimal TotalExpense { get; set; }
    public decimal TotalEarned { get; set; }   // Collected fees
    public decimal NetProfit { get; set; }     // Earned - Expense (optional)
    public int TotalTransactions { get; set; } // expense entries + salary payments + fee payments lines (optional)
}

public class TimePointDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}

public class AttendanceStatusDistributionDto
{
    public int Total { get; set; }
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Late { get; set; }
    public int Excused { get; set; }
    public int Sick { get; set; }
    public int Leave { get; set; }
    public int Holiday { get; set; }
    public int Other { get; set; }
}

public class AttendanceLeaderboardItemDto
{
    public Guid Id { get; set; }              // StaffId
    public string Code { get; set; } = "";    // EmployeeCode
    public string Name { get; set; } = "";
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public double AttendanceRate { get; set; } // Present/Total * 100
}

public class ClassAttendanceDto
{
    public int GradeLevel { get; set; }
    public int Section { get; set; }
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public double AttendanceRate { get; set; }
}

public class StudentTopDto
{
    public Guid StudentId { get; set; }
    public string AdmissionNo { get; set; } = "";
    public string FullName { get; set; } = "";
    public int GradeLevel { get; set; }
    public int Section { get; set; }
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public double AttendanceRate { get; set; }
}

public class TimePointByCategoryDto
{
    public DateTime Date { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class TimePointByNameDto
{
    public DateTime Date { get; set; }
    public string Name { get; set; } = default!;
    public decimal Amount { get; set; }
}

