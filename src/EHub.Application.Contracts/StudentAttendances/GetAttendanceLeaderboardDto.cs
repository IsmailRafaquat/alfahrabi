using EHub.Students;
using System;
using Volo.Abp.Application.Dtos;

namespace EHub.StudentAttendances;

public class GetAttendanceLeaderboardDto : EntityDto<Guid>
{
    public GradeLevel GradeLevel { get; set; }          // Class
    public Section Section { get; set; } // "A", "B", etc.

    public int Count { get; set; } = 10;         // N students
    public AttendanceLeaderboardOrder Order { get; set; } = AttendanceLeaderboardOrder.Top;

    // Optional (recommended). If frontend doesn't send, backend will default.
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
