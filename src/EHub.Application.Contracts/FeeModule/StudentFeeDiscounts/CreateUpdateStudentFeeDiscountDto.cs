using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.FeeModule.StudentFeeDiscounts;

public class CreateUpdateStudentFeeDiscountDto
{
    [Required]
    public Guid StudentId { get; set; }

    public Guid? FeeHeadId { get; set; }

    [Required]
    public DiscountType DiscountType { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Value { get; set; }

    [Range(1, 12)]
    public int? StartMonth { get; set; }

    [Range(1, 12)]
    public int? EndMonth { get; set; }

    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;

    public Guid? ApprovedByStaffId { get; set; }

    public bool IsActive { get; set; } = true;
}
