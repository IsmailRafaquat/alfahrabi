using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

public class FeesDashboardDto
{
    public decimal TotalExpected { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalLateFee { get; set; }
    public decimal TotalNet { get; set; }          // Expected - Discount + Late
    public decimal TotalPaid { get; set; }
    public decimal TotalOutstanding { get; set; }  // Net - Paid

    public int OverdueCount { get; set; }
    public decimal OverdueAmount { get; set; }

    public List<FeeHeadSummaryDto> FeeHeads { get; set; } = new();
    public List<TopDefaulterDto> TopDefaulters { get; set; } = new();
    public List<GroupSummaryDto> Groups { get; set; } = new(); // g
}
