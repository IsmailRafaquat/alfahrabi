using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EHub.Students;

public interface IStudentRepository : IRepository<Student, Guid>
{
    Task<Student?> FindByEmailAsync(string email);

    Task<Student?> FindByAdmissionNoAsync(string admissionNo);

    Task<List<Student>> GetListAsync(
          int skipCount,
          int maxResultCount,
          string sorting,
          string? filter,
          string? admissionNo,
          string? firstName,
          string? lastName,
          GradeLevel? gradeLevel,
          Section? section,
          Shift? shift,
          Term? term,
          DateTime? dob,
          Gender? gender,
          Status? status
      );

    Task<long> GetCountAsync(
          string? filter,
          string? admissionNo,
          string? firstName,
          string? lastName,
          GradeLevel? gradeLevel,
          Section? section,
          Shift? shift,
          Term? term,
          DateTime? dob,
          Gender? gender,
          Status? status
    );

    Task<Student?> GetByIdAsync(Guid id);
    Task<List<Student>> GetStudentLookupAsync();

    Task<List<Student>> GetByClassSectionAsync(GradeLevel gradeLevel, Section section);
    Task<List<string>> GetDeleteBlockersAsync(Guid studentId);

}
