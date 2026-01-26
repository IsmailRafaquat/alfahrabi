using EHub.Students;
using System;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

public class FeesDashboardInput
{
    public DateTime Month { get; set; }
    public GradeLevel? GradeLevel { get; set; }
    public Section? Section { get; set; }
    public Shift? Shift { get; set; }
    public Term? Term { get; set; }
}
