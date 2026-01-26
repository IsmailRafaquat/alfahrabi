using System;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

public class CalculateFeeLineAmountsInput
{
    public Guid StudentMonthlyFeeId { get; set; }
    public Guid FeeHeadId { get; set; }
}
