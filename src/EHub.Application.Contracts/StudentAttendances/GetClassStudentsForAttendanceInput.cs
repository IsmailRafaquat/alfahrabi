using EHub.AttendanceStatuss;
using EHub.Students;
using System;

namespace EHub.StudentAttendances;

public class GetClassStudentsForAttendanceInput
{
    public GradeLevel GradeLevel { get; set; }
    public Section Section { get; set; }
    public DateTime AttendanceDate { get; set; }
}

public class ClassStudentAttendanceRowDto
{
    public Guid StudentId { get; set; }
    public string? AdmissionNo { get; set; }
    public string FullName { get; set; } = string.Empty;
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Remarks { get; set; }
}

public class BulkMarkClassStudentAttendanceDto
{
    public DateTime AttendanceDate { get; set; }
    public BulkMarkClassStudentAttendanceItemDto[] Items { get; set; } = [];
}

public class BulkMarkClassStudentAttendanceItemDto
{
    public Guid StudentId { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
}
