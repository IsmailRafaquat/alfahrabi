using EHub.Students;
using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.Subjects;

public class CreateSubjectDto
{
    [StringLength(SubjectConsts.CodeMaxLength)]
    public string? Code { get; set; }   // nullable => auto-generate allowed

    [Required]
    [StringLength(SubjectConsts.NameMaxLength)]
    public string Name { get; set; } = default!;

    [StringLength(SubjectConsts.ShortNameMaxLength)]
    public string? ShortName { get; set; }

    [StringLength(SubjectConsts.DescriptionMaxLength)]
    public string? Description { get; set; }

    public GradeLevel? GradeLevel { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CreditHours { get; set; }

    public bool IsActive { get; set; } = true;
}

