using EHub.StudentAttendances;
using EHub.StudentDocuments;
using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.Students;

public class Student : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public string AdmissionNo { get;  set; }
    public string FirstName { get;  set; }
    public string LastName { get;  set; }
    public DateTime DOB { get;  set; }
    public Gender Gender { get;  set; }
    public string? Email { get;  set; }

    // Home Address
    public string StreetAddress { get;  set; }
    public string? StreetAddressLine2 { get;  set; }
    public City City { get;  set; }
    public Province Province { get;  set; }
    public string ZipCode { get;  set; }

    // Parent / Guardian Info
    public string PFirstName { get;  set; }
    public string PLastName { get;  set; }
    public RelationShipToStudent PRelatonShipToStudent { get;  set; }
    public string PPhone { get;  set; }
    public string? PEmail { get;  set; }

    // Emergency Contact Info
    public string? ECFirstName { get;  set; }
    public string? ECLastName { get;  set; }
    public RelationShipToStudent? ECRelationShipToStudent { get;  set; }
    public string? ECPhone { get;  set; }
    public string? ECEmail { get;  set; }

    // Education
    public GradeLevel GradeLevel { get;  set; }
    public Section Section { get;  set; }
    public DateTime EnrollmentDate { get;  set; }
    public Status Status { get;  set; } = Status.Active;
    public Term Term { get; set; } = Term.Fall;
    public Shift Shift { get; set; } = Shift.Morning;
 
    // Previous Educational Background
    public string? PerviousSchool { get;  set; }
    public GradeLevel? Grade { get;  set; }
    public string? StudentIdNo { get;  set; }

    // Additional Info
    public string? MedicalConditions { get;  set; }
    public string? Extracurrucular { get;  set; }
    public string? Commnets { get;  set; }
    public string? Accommodations { get;  set; }

    public virtual ICollection<StudentDocument> StudentDocuments { get; set; } 
    public virtual ICollection<StudentAttendance> StudentAttendances { get; set; }

    private Student() {

        StudentDocuments = new List<StudentDocument>();
        StudentAttendances = new List<StudentAttendance>();
    }

    internal Student(
        Guid id,
        Guid? tenantId,
        string admissionNo,
        string firstName,
        string lastName,
        Gender gender,
        DateTime dob,
        string? email,

        // Home Address
        string streetAddress,
        string? streetAddressLine2,
        City city,
        Province province,
        string zipCode,

        // Parent
        string pFirstName,
        string pLastName,
        RelationShipToStudent pRelationShipToStudent,
        string pPhoneNo,
        string? pEmail,

        // Emergency Contact
        string? eFirstName,
        string? eLastName,
        RelationShipToStudent? eRelationShipToStudent,
        string? ePhoneNo,
        string? eEmail,
        Term term,
        Shift shift,

        // Education
        GradeLevel gradeLevel,
        Section section,
        DateTime enrollmentDate,
        Status status,

        // Previous Education
        string? pervoiusSchool,
        GradeLevel pGrade,
        string? pStudentIdNumber,

        // Additional
        string? medicalConditions,
        string? extracurrucular,
        string? commnets,
        string? accommodations
    ) : base(id)
    {
        TenantId = tenantId;
        AdmissionNo = Check.NotNullOrWhiteSpace(admissionNo, nameof(AdmissionNo));
        SetFirstName(firstName);
        SetLastName(lastName);
        Gender = gender;
        DOB = dob;
        SetEmail(email);

        // Home Address
        SetStreetAddress(streetAddress);
        SetStreetAddressLine2(streetAddressLine2);
        City = city;
        Province = province;
        SetZipCode(zipCode);

        // Parent
        SetParentInfo(pFirstName, pLastName, pRelationShipToStudent, pPhoneNo, pEmail);

        // Emergency Contact
        SetEmergencyContact(eFirstName, eLastName, eRelationShipToStudent, ePhoneNo, eEmail);

        // Education
        GradeLevel = gradeLevel;
        Section = section;
        EnrollmentDate = enrollmentDate;
        Status = status;
        Term = term;
        Shift = shift;

        // Previous Background
        PerviousSchool = pervoiusSchool;
        Grade = pGrade;
        StudentIdNo = pStudentIdNumber;

        // Additional
        MedicalConditions = medicalConditions;
        Extracurrucular = extracurrucular;
        Commnets = commnets;
        Accommodations = accommodations;
    }

    internal Student ChangeName(string firstName, string lastName)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        return this;
    }

    internal Student ChangeContacts(string? email)
    {
        SetEmail(email);
        return this;
    }

    internal Student ChangeAddress(string streetAddress, string? streetAddressLine2, City city, Province province, string zip)
    {
        SetStreetAddress(streetAddress);
        SetStreetAddressLine2(streetAddressLine2);
        City = city;
        Province = province;
        SetZipCode(zip);
        return this;
    }

    internal Student ChangeParentInfo(string first, string last, RelationShipToStudent relation, string phone, string? email)
    {
        SetParentInfo(first, last, relation, phone, email);
        return this;
    }

    internal Student ChangeEmergencyContact(string? first, string? last, RelationShipToStudent? relation, string? phone, string? email)
    {
        SetEmergencyContact(first, last, relation, phone, email);
        return this;
    }


    private void SetFirstName(string firstName)
    {
        FirstName = Check.NotNullOrWhiteSpace(firstName, nameof(FirstName), maxLength: StudentConsts.NameMaxLength);
    }

    private void SetLastName(string lastName)
    {
        LastName = Check.NotNullOrWhiteSpace(lastName, nameof(LastName), maxLength: StudentConsts.NameMaxLength);
    }

    private void SetEmail(string? email)
    {
        if (!email.IsNullOrWhiteSpace())
            Check.Length(email, nameof(Email), StudentConsts.EmailMaxLength);

        Email = email;
    }

    private void SetStreetAddress(string address)
    {
        StreetAddress = Check.NotNullOrWhiteSpace(address, nameof(StreetAddress), maxLength: StudentConsts.StreetMaxLength);
    }

    private void SetStreetAddressLine2(string? value)
    {
        if (!value.IsNullOrWhiteSpace())
            Check.Length(value, nameof(StreetAddressLine2), StudentConsts.StreetMaxLength);
        StreetAddressLine2 = value;
    }

    private void SetZipCode(string value)
    {
        ZipCode = Check.NotNullOrWhiteSpace(value, nameof(ZipCode), maxLength: StudentConsts.ZipCodeMaxLength);
    }

    private void SetParentInfo(string firstName, string lastName, RelationShipToStudent relation, string phone, string? email)
    {
        PFirstName = Check.NotNullOrWhiteSpace(firstName, nameof(PFirstName), maxLength: StudentConsts.NameMaxLength);
        PLastName = Check.NotNullOrWhiteSpace(lastName, nameof(PLastName), maxLength: StudentConsts.NameMaxLength);
        PRelatonShipToStudent = relation;
        PPhone = Check.NotNullOrWhiteSpace(phone, nameof(PPhone), maxLength: StudentConsts.PhoneMaxLength);
        if (!email.IsNullOrWhiteSpace())
            Check.Length(email, nameof(PEmail), StudentConsts.EmailMaxLength);
        PEmail = email;
    }

    private void SetEmergencyContact(string? firstName, string? lastName, RelationShipToStudent? relation, string? phone, string? email)
    {
        ECFirstName = firstName;
        ECLastName = lastName;
        ECRelationShipToStudent = relation;
        ECPhone = phone;
        ECEmail = email;
    }
}
