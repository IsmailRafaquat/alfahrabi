using System;
using System.Collections.Generic;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

public class CheckFeesDashboardDto
{
    public DateTime Month { get; set; }

    // Summary
    public decimal TotalExpected { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalLateFee { get; set; }
    public decimal TotalNet { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }

    // Groups
    public List<FeeHeadSummaryDto> ByFeeHead { get; set; } = new();
    public List<StudentFeeSummaryDto> ByStudent { get; set; } = new();

    // For tables
    public int TotalStudents { get; set; }
}
