using EHub.Staffs;
using EHub.Students;
using System;
using Volo.Abp.Application.Dtos;

namespace EHub.StaffAttendances;

public class GetStaffAttendanceLeaderboardDto : EntityDto<Guid>
{
    public Department Department { get; set; } // staff grouping dimension

    public int Count { get; set; } = 10; // N staff
    public AttendanceLeaderboardOrder Order { get; set; } = AttendanceLeaderboardOrder.Top;

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
