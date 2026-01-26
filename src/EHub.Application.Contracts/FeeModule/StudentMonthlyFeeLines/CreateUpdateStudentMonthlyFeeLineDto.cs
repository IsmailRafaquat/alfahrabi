using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

public class CreateUpdateStudentMonthlyFeeLineDto
{
    [Required]
    public Guid StudentMonthlyFeeId { get; set; }

    [Required]
    public Guid FeeHeadId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ExpectedAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; }

    public decimal AdjustmentAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal LateFeeAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PaidAmount { get; set; }
}
