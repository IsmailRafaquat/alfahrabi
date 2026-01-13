using EHub.Students;
using EHub.Teaching;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EHub.TeacherSubjects;

public interface ITeacherSubjectRepository : IRepository<TeacherSubject, Guid>
{
    Task<TeacherSubject?> FindByStaffAndSectionAsync(Guid staffId, Section? section);

    Task<List<TeacherSubject>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filter,
        Guid? staffId,
        Section? section,
        bool? isPrimaryTeacher
        );

    Task<long> GetCountAsync(
        string? filter,
        Guid? staffId,
        Section? section,
        bool? isPrimaryTeacher
        );
}
