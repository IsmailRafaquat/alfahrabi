using EHub.Students;
using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.FeeModule.StudentFeeProfiles;

public class BulkAssignStudentFeeProfileDto
{
    [Required]
    public Guid FeeStructureId { get; set; }

    [Required]
    public GradeLevel GradeLevel { get; set; }     // or GradeLevel enum
    public Section? Section { get; set; }
    public Shift? Shift { get; set; }         // or Shift enum
    public Term? Term { get; set; }          // optional

    [Required]
    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;

    // Optional: if true, skip duplicates; if false, throw on first duplicate.
    public bool SkipExisting { get; set; } = true;
}
