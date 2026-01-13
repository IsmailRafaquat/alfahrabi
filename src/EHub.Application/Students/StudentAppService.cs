using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.Students;

[RemoteService(IsEnabled = false)]
public class StudentAppService : ApplicationService, IStudentAppService
{
    private readonly IStudentRepository _studentRepository;
    private readonly StudentManager _studentManager;

    public StudentAppService(
        IStudentRepository studentRepository,
        StudentManager studentManager)
    {
        _studentRepository = studentRepository;
        _studentManager = studentManager;
    }

    public async Task<StudentDto> GetAsync(Guid id)
    {
        var entity = await _studentRepository.GetByIdAsync(id);
        return ObjectMapper.Map<Student, StudentDto>(entity!);
    }

    public async Task<PagedResultDto<StudentDto>> GetListAsync(GetStudentListDto input)
    {
        if (input.Sorting.IsNullOrWhiteSpace())
        {
            input.Sorting = nameof(Student.FirstName);
        }

        var totalCount = await _studentRepository.GetCountAsync(
            input.Filter,
            input.AdmissionNo,
            input.FirstName,
            input.LastName,
            input.DOB,
            input.Gender,
            input.Status
        );

        var list = await _studentRepository.GetListAsync(
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            input.Filter,
            input.AdmissionNo,
            input.FirstName,
            input.LastName,
            input.DOB,
            input.Gender,
            input.Status
        );

        var items = list.Select(s => ObjectMapper.Map<Student, StudentDto>(s)).ToList();

        return new PagedResultDto<StudentDto>(totalCount, items);
    }

    public async Task<StudentDto> CreateAsync(CreateStudentDto input)
    {
        var student = await _studentManager.CreateAsync(
            input.AdmissionNo,
            input.FirstName,
            input.LastName,
            input.Gender,
            input.DOB,
            input.EnrollmentDate,
            input.GradeLevel,
            input.Section,
            input.Term,
            input.Shift,
            input.City,
            input.Province,
            input.StreetAddress,
            input.ZipCode,
            input.PFirstName,
            input.PLastName,
            input.Grade,
            input.PRelatonShipToStudent,
            input.PPhone,
            input.Status,
            input.Email,
            input.StreetAddressLine2,
            input.PEmail,
            input.ECFirstName,
            input.ECLastName,
            input.ECRelationShipToStudent,
            input.ECPhone,
            input.ECEmail,
            input.PerviousSchool,
            input.StudentIdNo,
            input.MedicalConditions,
            input.Extracurrucular,
            input.Commnets,
            input.Accommodations
        );

        student = await _studentRepository.InsertAsync(student);
        return ObjectMapper.Map<Student, StudentDto>(student);
    }

    public async Task UpdateAsync(Guid id, UpdateStudentDto input)
    {
        var student = await _studentRepository.GetAsync(id);

        // Update core info
        await _studentManager.ChangeNameAsync(student, input.FirstName, input.LastName);
        await _studentManager.ChangeContactsAsync(student, input.Email);
        await _studentManager.ChangeAddressAsync(student, input.StreetAddress, input.StreetAddressLine2, input.City, input.Province, input.ZipCode);
        await _studentManager.ChangeParentInfoAsync(student, input.PFirstName, input.PLastName, input.PRelatonShipToStudent, input.PPhone, input.PEmail);
        await _studentManager.ChangeEmergencyContactAsync(student, input.ECFirstName, input.ECLastName, input.ECRelationShipToStudent, input.ECPhone, input.ECEmail);
        await _studentManager.ChangeStatusAsync(student, input.Status);

        student.AdmissionNo = input.AdmissionNo;
        student.Grade = input.Grade;
        student.GradeLevel = input.GradeLevel;
        student.Gender = input.Gender;
        student.DOB = input.DOB;
        student.EnrollmentDate = input.EnrollmentDate;
        student.Status = input.Status;
        student.Term = input.Term;
        student.Shift = input.Shift;

        //Educational Background
        student.PerviousSchool = input.PerviousSchool;
        student.StudentIdNo = input.StudentIdNo;
        student.MedicalConditions = input.MedicalConditions;
        student.Extracurrucular = input.Extracurrucular;
        student.Commnets = input.Commnets;
        student.Accommodations = input.Accommodations;

        // Save
        await _studentRepository.UpdateAsync(student, autoSave: true);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _studentRepository.DeleteAsync(id);
    }

    public async Task<List<StudentLookupDto>> GetStudentLookupAsync()
    {
        var students = await _studentRepository.GetStudentLookupAsync();

       return students.Select(x => new StudentLookupDto
        {
            Id = x.Id,
            AdmissionNo = x.AdmissionNo,
            FirstName = x.FirstName,
            LastName = x.LastName
        }).ToList();
    }
}
