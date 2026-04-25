using System;
using System.Collections.Generic;

namespace EHub.Reports.StudentFeeReport;

public class StudentFeeClassReportDto
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;

    public decimal ExpectedAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal PendingAmount { get; set; }

    public List<StudentFeeStudentReportDto> Students { get; set; } = new();
}

public class StudentFeeStudentReportDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? AdmissionNo { get; set; }

    public decimal ExpectedAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public List<string> PendingMonths { get; set; } = new();
    public bool IsPending => PendingAmount > 0;
}
