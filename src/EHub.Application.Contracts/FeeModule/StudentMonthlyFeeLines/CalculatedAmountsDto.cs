namespace EHub.FeeModule.StudentMonthlyFeeLines;

public class CalculatedAmountsDto
{
    public decimal ExpectedAmount { get; set; }  // Now fetched from FeeStructureItem
    public decimal DiscountAmount { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal NetAmount { get; set; }
}
