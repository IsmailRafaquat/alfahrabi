using System;

namespace EHub.Reports.StudentFeeReport;

public class StudentFeeReportFilterDto
{
    public DateTime? MonthStart { get; set; }
    public DateTime? MonthEnd { get; set; }
    public int? ClassId { get; set; }
    public string? Filter { get; set; }
    public int? GradeLevel { get; set; }
}
