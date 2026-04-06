using System;
using System.Collections.Generic;

namespace EHub.FeeModule.StudentRecentFeeHistory;

public class StudentRecentFeeHistoryDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;

    public Guid SelectedStudentMonthlyFeeId { get; set; }
    public DateTime SelectedMonth { get; set; }

    public decimal TotalBalance { get; set; }
    public decimal PreviousBalance { get; set; }

    public List<StudentRecentFeeHistoryMonthDto> Months { get; set; } = new();
}

public class StudentRecentFeeHistoryMonthDto
{
    public Guid StudentMonthlyFeeId { get; set; }
    public DateTime Month { get; set; }

    public decimal ExpectedAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal AdjustmentAmount { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance { get; set; }

    public List<StudentRecentFeeHistoryLineDto> Lines { get; set; } = new();
}

public class StudentRecentFeeHistoryLineDto
{
    public Guid StudentMonthlyFeeLineId { get; set; }
    public Guid FeeHeadId { get; set; }
    public string FeeHeadName { get; set; } = string.Empty;

    public decimal ExpectedAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal AdjustmentAmount { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance { get; set; }
}
