using System;
using System.Collections.Generic;

namespace EHub.Reports.SalaryReport;

public class StaffSalaryReportDto
{
    public List<StaffSalaryReportRowDto> Rows { get; set; } = new();
    public decimal GrandTotal { get; set; }
}

public class StaffSalaryReportRowDto
{
    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public DateTime SalaryDate { get; set; }
    public decimal SalaryAmount { get; set; }
}
