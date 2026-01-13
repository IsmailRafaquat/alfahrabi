using EHub.StaffDocuments;
using EHub.StudentDocuments;
using EHub.Students;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace EHub.Staffs;

public class StaffDto : EntityDto<Guid>
{
    // Personal Info
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNo { get; set; }
    public string? Email { get; set; }
    public DateTime DOB { get; set; }
    public Nationality Nationality { get; set; }
    public RelationshipStatus RelationshipStatus { get; set; }
    public Gender Gender { get; set; }
    public string LanguageKnown { get; set; }
    public DisabilityStatus DisabilityStatus { get; set; }

    // Address
    public string StreetAddress { get; set; }
    public string? StreetAddressLine2 { get; set; }
    public City City { get; set; }
    public Province Province { get; set; }
    public string ZipCode { get; set; }

    // Job Info

    public string EmployeeCode { get; set; }
    public DateTime JoiningDate { get; set; }
    public string? Designation { get; set; }
    public Department Department { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public JobStatus JobStatus { get; set; }
    public decimal? Salary { get; set; }
    public string? ReportingManager { get; set; }
    public Shift? WorkShift { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public string? Remarks { get; set; }
    public List<StaffDocumentDto> StaffDocuments { get; set; }
}
