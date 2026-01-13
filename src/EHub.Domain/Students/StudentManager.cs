using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace EHub.Students;

public class StudentManager : DomainService
{
    private readonly IStudentRepository _studentRepository;

    public StudentManager(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Student> CreateAsync(
        string admissionNo,
        string firstName,
        string lastName,
        Gender gender,
        DateTime dob,
        DateTime enrollmentDate,
        GradeLevel gradeLevel,
        Section section,
        Term term,
        Shift shift,
        City city,
        Province province,
        string streetAddress,
        string zipCode,
        string pFirstName,
        string pLastName,
        GradeLevel previousGrade,
        RelationShipToStudent pRelation,
        string pPhone,
        Status status = Status.Active,
        string? email = null,
        string? streetAddressLine2 = null,
        string? pEmail = null,
        string? eFirstName = null,
        string? eLastName = null,
        RelationShipToStudent? eRelation = null,
        string? ePhone = null,
        string? eEmail = null,
        string? previousSchool = null,
        string? studentIdNo = null,
        string? medicalConditions = null,
        string? extracurricular = null,
        string? comments = null,
        string? accommodations = null)
        {
        Check.NotNullOrWhiteSpace(firstName, nameof(firstName));
        Check.NotNullOrWhiteSpace(lastName, nameof(lastName));
        Check.NotNullOrWhiteSpace(streetAddress, nameof(streetAddress));
        Check.NotNullOrWhiteSpace(zipCode, nameof(zipCode));
        Check.NotNullOrWhiteSpace(pFirstName, nameof(pFirstName));
        Check.NotNullOrWhiteSpace(pLastName, nameof(pLastName));
        Check.NotNullOrWhiteSpace(pPhone, nameof(pPhone));

        if (!email.IsNullOrWhiteSpace())
        {
            var existingByEmail = await _studentRepository.FindByEmailAsync(email!);
            if (existingByEmail != null)
            {
                throw new StudentEmailAlreadyUsedException(email!);
            }
        }

        if (dob > DateTime.UtcNow.Date)
            throw new DateOfBirthException();
        if (enrollmentDate.Date > DateTime.UtcNow.Date.AddDays(7))
            throw new InvalidEnrollmentDateException();

        //var admissionNo = await GenerateUniqueAdmissionNoAsync();

        var student = new Student(
           id: GuidGenerator.Create(),
           tenantId: CurrentTenant.Id,
           admissionNo: admissionNo,
           firstName: firstName,
           lastName: lastName,
           gender: gender,
           dob: dob,
           email: email,
           // Home Address
           streetAddress: streetAddress,
           streetAddressLine2: streetAddressLine2,
           city: city,
           province: province,
           zipCode: zipCode,
           // Parent Info
           pFirstName: pFirstName,
           pLastName: pLastName,
           pGrade: previousGrade,

           pRelationShipToStudent: pRelation,
           pPhoneNo: pPhone,
           pEmail: pEmail,
           // Emergency Contact
           eFirstName: eFirstName,
           eLastName: eLastName,
           
           eRelationShipToStudent: eRelation,
           ePhoneNo: ePhone,
           eEmail: eEmail,
           // Term & Shift
           term: term,
           shift: shift,
           // Education
           gradeLevel: gradeLevel,
           section: section,
           enrollmentDate: enrollmentDate,
           status: status,
           // Background
           pervoiusSchool: previousSchool,
           pStudentIdNumber: studentIdNo,
           // Additional
           medicalConditions: medicalConditions,
           extracurrucular: extracurricular,
           commnets: comments,
           accommodations: accommodations
       );

        return student;

    }

    public Task ChangeStatusAsync(Student student, Status status)
    {
        Check.NotNull(student, nameof(student));
        student.Status = status;
        return Task.CompletedTask;
    }

    public Task ChangeNameAsync(Student student, string firstName, string lastName)
    {
        Check.NotNull(student, nameof(student));
        Check.NotNullOrWhiteSpace(firstName, nameof(firstName));
        Check.NotNullOrWhiteSpace(lastName, nameof(lastName));

        student.ChangeName(firstName, lastName);
        return Task.CompletedTask;
    }

    public async Task ChangeContactsAsync(Student student, string? email)
    {
        Check.NotNull(student, nameof(student));

        if (!email.IsNullOrWhiteSpace())
        {
            var existing = await _studentRepository.FindByEmailAsync(email!);
            if (existing != null && existing.Id != student.Id)
            {
                throw new BusinessException(EHubDomainErrorCodes.StudentEmailAlreadyUsed)
                    .WithData("Email", email);
            }
        }

        student.ChangeContacts(email);
    }

    public Task ChangeAddressAsync(Student student, string streetAddress, string? streetAddressLine2, City city, Province province, string zip)
    {
        Check.NotNull(student, nameof(student));
        Check.NotNullOrWhiteSpace(streetAddress, nameof(streetAddress));
        Check.NotNullOrWhiteSpace(zip, nameof(zip));

        student.ChangeAddress(streetAddress, streetAddressLine2, city, province, zip);
        return Task.CompletedTask;
    }

    public Task ChangeParentInfoAsync(Student student, string first, string last, RelationShipToStudent relation, string phone, string? email)
    {
        Check.NotNull(student, nameof(student));
        Check.NotNullOrWhiteSpace(first, nameof(first));
        Check.NotNullOrWhiteSpace(last, nameof(last));
        Check.NotNullOrWhiteSpace(phone, nameof(phone));

        student.ChangeParentInfo(first, last, relation, phone, email);
        return Task.CompletedTask;
    }

    public Task ChangeEmergencyContactAsync(Student student, string? first, string? last, RelationShipToStudent? relation, string? phone, string? email)
    {
        Check.NotNull(student, nameof(student));
        student.ChangeEmergencyContact(first, last, relation, phone, email);
        return Task.CompletedTask;
    }

    public Task ChangeEducationInfoAsync(Student student, GradeLevel gradeLevel, Section section, Term term, Shift shift)
    {
        Check.NotNull(student, nameof(student));

        student.GradeLevel = gradeLevel;
        student.Section = section;
        student.Term = term;
        student.Shift = shift;

        return Task.CompletedTask;
    }

    //private async Task<string> GenerateUniqueAdmissionNoAsync()
    //{
    //    var year = DateTime.UtcNow.Year.ToString();

    //    var rnd = new Random();

    //    for (var attempt = 0; attempt < 20; attempt++)
    //    {
    //        var number = rnd.Next(0, 100000);
    //        var candidate = $"{year}-{number:00000}";

    //        var exists = await _studentRepository.FindByAdmissionNoAsync(candidate);
    //        if (exists == null)
    //            return candidate;
    //    }

    //    throw new BusinessException(EHubDomainErrorCodes.AdmissionNumberGenerationFailed);
    //}
}
