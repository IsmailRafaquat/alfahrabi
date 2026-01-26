using System;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

public class TopDefaulterDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = "";
    public decimal Outstanding { get; set; }
}
