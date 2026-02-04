using System;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

public class StudentFeeSummaryDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = "";
    public decimal Net { get; set; }
    public decimal Paid { get; set; }
    public decimal Pending { get; set; }
    public string? ParentContact { get; set; }
}
