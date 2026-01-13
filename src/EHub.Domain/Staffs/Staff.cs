using EHub.StaffDocuments;
using EHub.Students;
using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.Staffs;

public class Staff : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    // Personal Information
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

    // Home Address
    public string StreetAddress { get; set; }
    public string? StreetAddressLine2 { get; set; }
    public City City { get; set; }
    public Province Province { get; set; }
    public string ZipCode { get; set; }

    // Job Information
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

    public virtual ICollection<StaffDocument> StaffDocuments { get; set; }

    private Staff()
    {
        StaffDocuments = new List<StaffDocument>();
    }

    internal Staff(
        Guid id,
        Guid? tenantId,
        string firstName,
        string lastName,
        string phoneNo,
        string? email,
        DateTime dob,
        Nationality nationality,
        RelationshipStatus relationshipStatus,
        Gender gender,
        string languageKnown,
        DisabilityStatus disabilityStatus,

        // Address
        string streetAddress,
        string? streetAddressLine2,
        City city,
        Province province,
        string zipCode,

        // Job Info
        string employeeCode,
        DateTime joiningDate,
        string? designation,
        Department department,
        EmploymentType employmentType,
        JobStatus jobStatus,
        decimal? salary,
        string? reportingManager,
        Shift? workShift,
        DateTime? contractStart,
        DateTime? contractEnd,
        string? remarks
    ) : base(id)
    {
        TenantId = tenantId;
        SetName(firstName, lastName);
        SetPhone(phoneNo);
        SetEmail(email);
        DOB = dob;
        Nationality = nationality;
        RelationshipStatus = relationshipStatus;
        Gender = gender;
        LanguageKnown = Check.NotNullOrWhiteSpace(languageKnown, nameof(LanguageKnown));
        DisabilityStatus = disabilityStatus;

        SetAddress(streetAddress, streetAddressLine2, city, province, zipCode);

        EmployeeCode = Check.NotNullOrWhiteSpace(employeeCode, nameof(EmployeeCode));
        JoiningDate = joiningDate;
        Designation = designation;
        Department = department;
        EmploymentType = employmentType;
        JobStatus = jobStatus;
        Salary = salary;
        ReportingManager = reportingManager;
        WorkShift = workShift;
        ContractStartDate = contractStart;
        ContractEndDate = contractEnd;
        Remarks = remarks;
    }

    // Change Methods
    internal Staff ChangePersonalInfo(
        string firstName,
        string lastName,
        string phoneNo,
        string? email,
        DateTime dob,
        Nationality nationality,
        RelationshipStatus relationshipStatus,
        Gender gender,
        string languageKnown,
        DisabilityStatus disabilityStatus)
    {
        SetName(firstName, lastName);
        SetPhone(phoneNo);
        SetEmail(email);
        DOB = dob;
        Nationality = nationality;
        RelationshipStatus = relationshipStatus;
        Gender = gender;
        LanguageKnown = Check.NotNullOrWhiteSpace(languageKnown, nameof(LanguageKnown));
        DisabilityStatus = disabilityStatus;
        return this;
    }

    internal Staff ChangeAddress(string street, string? street2, City city, Province province, string zip)
    {
        SetAddress(street, street2, city, province, zip);
        return this;
    }

    internal Staff ChangeJobInfo(
        string? designation,
        Department department,
        EmploymentType employmentType,
        JobStatus jobStatus,
        decimal? salary,
        string? reportingManager,
        Shift? workShift,
        DateTime? contractStart,
        DateTime? contractEnd,
        string? remarks)
    {
        Designation = designation;
        Department = department;
        EmploymentType = employmentType;
        JobStatus = jobStatus;
        Salary = salary;
        ReportingManager = reportingManager;
        WorkShift = workShift;
        ContractStartDate = contractStart;
        ContractEndDate = contractEnd;
        Remarks = remarks;
        return this;
    }

    // Private Setters / Validation
    private void SetName(string first, string last)
    {
        FirstName = Check.NotNullOrWhiteSpace(first, nameof(FirstName), maxLength: StaffConsts.NameMaxLength);
        LastName = Check.NotNullOrWhiteSpace(last, nameof(LastName), maxLength: StaffConsts.NameMaxLength);
    }

    private void SetPhone(string phone)
    {
        PhoneNo = Check.NotNullOrWhiteSpace(phone, nameof(PhoneNo), maxLength: StaffConsts.PhoneMaxLength);
    }

    private void SetEmail(string? email)
    {
        if (!email.IsNullOrWhiteSpace())
            Check.Length(email, nameof(Email), maxLength: StaffConsts.EmailMaxLength);
        Email = email;
    }


    private void SetAddress(string street, string? street2, City city, Province province, string zip)
    {
        StreetAddress = Check.NotNullOrWhiteSpace(street, nameof(StreetAddress), maxLength: StaffConsts.StreetMaxLength);
        if (!street2.IsNullOrWhiteSpace())
            Check.Length(street2, nameof(StreetAddressLine2), maxLength: StaffConsts.StreetMaxLength);
        StreetAddressLine2 = street2;
        City = city;
        Province = province;
        ZipCode = Check.NotNullOrWhiteSpace(zip, nameof(ZipCode), maxLength: StaffConsts.ZipCodeMaxLength);
    }
}
