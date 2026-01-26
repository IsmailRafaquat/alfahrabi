using System;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

public class FeeHeadSummaryDto
{
    public Guid FeeHeadId { get; set; }
    public string FeeHeadName { get; set; } = "";
    public decimal Expected { get; set; }
    public decimal Discount { get; set; }
    public decimal LateFee { get; set; }
    public decimal Net { get; set; }
    public decimal Paid { get; set; }
    public decimal Pending { get; set; }
}
