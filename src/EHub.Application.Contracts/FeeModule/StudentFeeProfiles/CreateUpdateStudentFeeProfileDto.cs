using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.FeeModule.StudentFeeProfiles;

public class CreateUpdateStudentFeeProfileDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid FeeStructureId { get; set; }

    [Required]
    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;
}
