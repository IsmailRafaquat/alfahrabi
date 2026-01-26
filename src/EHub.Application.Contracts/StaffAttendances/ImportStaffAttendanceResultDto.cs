using System.Collections.Generic;

namespace EHub.StaffAttendances;

public class ImportStaffAttendanceResultDto
{
    public int StaffInFile { get; set; }
    public int StaffMatched { get; set; }
    public int DatesInFile { get; set; }
    public int RecordsUpserted { get; set; }
    public List<string> Errors { get; set; } = new();
}
