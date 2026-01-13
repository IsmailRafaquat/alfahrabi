using EHub.Students;
using System;

namespace EHub.StudentAttendances;

public class GenerateStudentAttendanceTemplateDto
{
    public GradeLevel GradeLevel { get; set; }

    public Section Section { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}
