namespace EHub.FeeModule.StudentMonthlyFeeLines;

public class GroupSummaryDto
{
    public int? GradeLevel { get; set; }
    public int? Section { get; set; }
    public int? Shift { get; set; }
    public int? Term { get; set; }

    public decimal Net { get; set; }
    public decimal Paid { get; set; }
    public decimal Outstanding { get; set; }
}
