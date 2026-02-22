using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.Students;

public class CreateStudentDto
{
    // --- Basic Info ---
    [Required]
    [StringLength(StudentConsts.AdmissionNoMaxLength)]
    public string AdmissionNo { get; set; } = default!;

    [StringLength(StudentConsts.AdmissionNoMaxLength)]
    public string? RollNo { get; set; }

    [Required]
    [StringLength(StudentConsts.NameMaxLength)]
    public string FirstName { get; set; } = default!;

    [Required]
    [StringLength(StudentConsts.NameMaxLength)]
    public string LastName { get; set; } = default!;

    [Required]
    public Gender Gender { get; set; }

    [Required]
    public DateTime DOB { get; set; }

    [EmailAddress]
    [StringLength(StudentConsts.EmailMaxLength)]
    public string? Email { get; set; }

    // --- Home Address ---
    [Required]
    [StringLength(StudentConsts.StreetMaxLength)]
    public string StreetAddress { get; set; } = default!;

    [StringLength(StudentConsts.StreetMaxLength)]
    public string? StreetAddressLine2 { get; set; }

    [Required]
    public City City { get; set; }

    [Required]
    public Province Province { get; set; }

    [Required]
    [StringLength(StudentConsts.ZipCodeMaxLength)]
    public string ZipCode { get; set; } = default!;

    // --- Parent / Guardian Info ---
    [Required]
    [StringLength(StudentConsts.NameMaxLength)]
    public string PFirstName { get; set; } = default!;

    [Required]
    [StringLength(StudentConsts.NameMaxLength)]
    public string PLastName { get; set; } = default!;

    [Required]
    public RelationShipToStudent PRelatonShipToStudent { get; set; }

    [Required]
    [StringLength(StudentConsts.PhoneMaxLength)]
    public string PPhone { get; set; } = default!;

    [EmailAddress]
    [StringLength(StudentConsts.EmailMaxLength)]
    public string? PEmail { get; set; }

    // --- Emergency Contact Info (optional) ---
    [StringLength(StudentConsts.NameMaxLength)]
    public string? ECFirstName { get; set; }

    [StringLength(StudentConsts.NameMaxLength)]
    public string? ECLastName { get; set; }

    public RelationShipToStudent? ECRelationShipToStudent { get; set; }

    [StringLength(StudentConsts.PhoneMaxLength)]
    public string? ECPhone { get; set; }

    [EmailAddress]
    [StringLength(StudentConsts.EmailMaxLength)]
    public string? ECEmail { get; set; }

    // --- Education ---
    [Required]
    public GradeLevel GradeLevel { get; set; }

    [Required]
    public Section Section { get; set; }

    [Required]
    public DateTime EnrollmentDate { get; set; }

    public Status Status { get; set; } = Status.Active;

    [Required]
    public Term Term { get; set; }

    [Required]
    public Shift Shift { get; set; }

    // --- Previous Educational Background (optional) ---
    [StringLength(StudentConsts.SchoolNameMaxLength)]
    public string? PerviousSchool { get; set; }

    public GradeLevel Grade { get; set; }

    [StringLength(StudentConsts.StudentIdNoMaxLength)]
    public string? StudentIdNo { get; set; }

    // --- Additional Info ---
    [StringLength(StudentConsts.DescriptionMaxLength)]
    public string? MedicalConditions { get; set; }

    [StringLength(StudentConsts.DescriptionMaxLength)]
    public string? Extracurrucular { get; set; }

    [StringLength(StudentConsts.DescriptionMaxLength)]
    public string? Commnets { get; set; }

    [StringLength(StudentConsts.DescriptionMaxLength)]
    public string? Accommodations { get; set; }
}
