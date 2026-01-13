using EHub.Students;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EHub.Subjects;

public interface ISubjectRepository : IRepository<Subject, Guid>
{
    Task<Subject?> FindByCodeAsync(Guid? tenantId, string code);
    Task<Subject?> GetLastCreatedSubjectAsync(Guid? tenantId);
    Task<Subject?> FindByNameAndGradeAsync(Guid? tenantId, string name, GradeLevel? gradeLevel);
    Task<long> GetCountAsync(
        string? filter,
        string? code,
        string? name,
        GradeLevel? gradeLevel,
        bool? isActive);
    Task<List<Subject>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filter,
        string? code,
        string? name,
        GradeLevel? gradeLevel,
        bool? isActive);
}
