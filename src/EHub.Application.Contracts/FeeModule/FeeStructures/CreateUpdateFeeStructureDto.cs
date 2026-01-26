using EHub.Students;
using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.FeeModule.FeeStructures;

public class CreateUpdateFeeStructureDto
{
    [Required]
    public GradeLevel GradeLevel { get; set; }

    [Required]
    public Shift Shift { get; set; }

    [Required]
    public Term Term { get; set; }

    [Required]
    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;
}
