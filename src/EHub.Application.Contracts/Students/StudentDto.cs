using EHub.FileAttachments;
using EHub.StudentDocuments;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace EHub.Students;

public class StudentDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    // --- Basic Info ---
    public string AdmissionNo { get; set; } = default!;
    public string? RollNo { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public Gender Gender { get; set; }
    public DateTime DOB { get; set; }
    public string? Email { get; set; }

    // --- Home Address ---
    public string StreetAddress { get; set; } = default!;
    public string? StreetAddressLine2 { get; set; }
    public City City { get; set; }
    public Province Province { get; set; }
    public string ZipCode { get; set; } = default!;

    // --- Parent / Guardian Info ---
    public string PFirstName { get; set; } = default!;
    public string PLastName { get; set; } = default!;
    public RelationShipToStudent PRelatonShipToStudent { get; set; }
    public string PPhone { get; set; } = default!;
    public string? PEmail { get; set; }

    // --- Emergency Contact Info ---
    public string? ECFirstName { get; set; }
    public string? ECLastName { get; set; }
    public RelationShipToStudent? ECRelationShipToStudent { get; set; }
    public string? ECPhone { get; set; }
    public string? ECEmail { get; set; }

    // --- Education ---
    public GradeLevel GradeLevel { get; set; }
    public Section Section { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public Status Status { get; set; }
    public Term Term { get; set; }
    public Shift Shift { get; set; }

    // --- Previous Education ---
    public string? PerviousSchool { get; set; }
    public GradeLevel Grade { get; set; }
    public string? StudentIdNo { get; set; }

    // --- Additional Info ---
    public string? MedicalConditions { get; set; }
    public string? Extracurrucular { get; set; }
    public string? Commnets { get; set; }
    public string? Accommodations { get; set; }

    public List<StudentDocumentDto> StudentDocument { get; set; } 
}
