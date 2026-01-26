using System.Collections.Generic;

namespace EHub.FeeModule.StudentFeeDiscounts;

public class BulkAssignStudentFeeDiscountResultDto
{
    public int TotalStudents { get; set; }
    public int Created { get; set; }
    public int SkippedExisting { get; set; }
    public List<string> SkippedStudentNames { get; set; } = new();
}
