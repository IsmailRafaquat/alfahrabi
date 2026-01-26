using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EHub.FeeModule.StudentFeeProfiles;

public interface IStudentFeeProfileRepository : IRepository<StudentFeeProfile, Guid>
{
    Task<StudentFeeProfile?> FindActiveByStudentAsync(Guid studentId);
    Task<StudentFeeProfile?> GetByIdAsync(Guid id);

    Task<long> GetCountAsync(
        string? filter,
        Guid? studentId,
        Guid? feeStructureId,
        bool? isActive,
        DateTime? effectiveFrom,
        DateTime? effectiveTo
    );

    Task<List<StudentFeeProfile>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filter,
        Guid? studentId,
        Guid? feeStructureId,
        bool? isActive,
        DateTime? effectiveFrom,
        DateTime? effectiveTo

    );

    Task<List<StudentFeeProfile>> GetActiveProfilesByStudentAsync(Guid studentId, Guid? exceptId = null);
}
