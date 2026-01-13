using EHub.Students;
using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.Staffs;

public class CreateStaffDto
{
    // --- Personal Information ---
    [Required]
    [StringLength(StaffConsts.NameMaxLength)]
    public string FirstName { get; set; } = default!;

    [Required]
    [StringLength(StaffConsts.NameMaxLength)]
    public string LastName { get; set; } = default!;

    [Required]
    [StringLength(StaffConsts.PhoneMaxLength)]
    public string PhoneNo { get; set; } = default!;

    [EmailAddress]
    [StringLength(StaffConsts.EmailMaxLength)]
    public string? Email { get; set; }

    [Required]
    public DateTime DOB { get; set; }

    [Required]
    public Nationality Nationality { get; set; }

    [Required]
    public RelationshipStatus RelationshipStatus { get; set; }

    [Required]
    public Gender Gender { get; set; }

    [Required]
    [StringLength(StaffConsts.LanguageKnownMaxLength)]
    public string LanguageKnown { get; set; } = default!;

    [Required]
    public DisabilityStatus DisabilityStatus { get; set; }

    // --- Home Address ---
    [Required]
    [StringLength(StaffConsts.StreetMaxLength)]
    public string StreetAddress { get; set; } = default!;

    [StringLength(StaffConsts.StreetMaxLength)]
    public string? StreetAddressLine2 { get; set; }

    [Required]
    public City City { get; set; }

    [Required]
    public Province Province { get; set; }

    [Required]
    [StringLength(StaffConsts.ZipCodeMaxLength)]
    public string ZipCode { get; set; } = default!;

    // --- Job Information ---
    [Required]
    public DateTime JoiningDate { get; set; }

    [StringLength(StaffConsts.DesignationMaxLength)]
    public string? Designation { get; set; }

    [Required]
    public Department Department { get; set; }

    [Required]
    public EmploymentType EmploymentType { get; set; }

    [Required]
    public JobStatus JobStatus { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? Salary { get; set; }

    [StringLength(StaffConsts.ReportingManagerMaxLength)]
    public string? ReportingManager { get; set; }

    public Shift? WorkShift { get; set; }

    public DateTime? ContractStartDate { get; set; }

    public DateTime? ContractEndDate { get; set; }

    [StringLength(StaffConsts.RemarksMaxLength)]
    public string? Remarks { get; set; }
}
