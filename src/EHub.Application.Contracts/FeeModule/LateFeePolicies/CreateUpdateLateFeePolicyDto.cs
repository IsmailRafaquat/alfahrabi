using EHub.Students;
using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.FeeModule.LateFeePolicies;

public class CreateUpdateLateFeePolicyDto
{
    public GradeLevel? GradeLevel { get; set; }
    public Section? Section { get; set; }
    public Shift? Shift { get; set; }
    public Term? Term { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int GraceDays { get; set; }

    [Required]
    public LateFeeType Type { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Value { get; set; }

    public bool IsActive { get; set; } = true;
}
