using System.Collections.Generic;

namespace EHub.Students;

public class ImportStudentResultDto
{
    public int TotalRows { get; set; }
    public int SkippedRows { get; set; }

    public int Created { get; set; }
    public int Updated { get; set; }

    public List<string> Errors { get; set; } = new();
}
