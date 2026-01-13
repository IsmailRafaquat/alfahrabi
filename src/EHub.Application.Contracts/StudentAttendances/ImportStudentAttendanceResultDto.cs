using System.Collections.Generic;

namespace EHub.StudentAttendances;

public class ImportStudentAttendanceResultDto
{
    public int StudentsInFile { get; set; }
    public int StudentsMatched { get; set; }
    public int DatesInFile { get; set; }
    public int RecordsUpserted { get; set; }
    public List<string> Errors { get; set; } = new();
}
