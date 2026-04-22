using System;
using System.Collections.Generic;

namespace EHub.Reports.SalaryReport;

public class StaffSalaryReportFilterDto
{
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public string? Filter { get; set; }
    public List<Guid>? StaffIds { get; set; }
}
