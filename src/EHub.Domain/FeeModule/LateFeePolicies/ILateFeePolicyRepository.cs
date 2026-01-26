using EHub.Students;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EHub.FeeModule.LateFeePolicies;

public interface ILateFeePolicyRepository : IRepository<LateFeePolicy, Guid>
{
    Task<LateFeePolicy?> GetByIdAsync(Guid id);

    Task<long> GetCountAsync(
        string? filter,
        GradeLevel? gradeLevel,
        Section? section,
        Shift? shift,
        Term? term,
        LateFeeType? type,
        bool? isActive,
        bool? isGlobal
    );

    Task<List<LateFeePolicy>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filter,
        GradeLevel? gradeLevel,
        Section? section,
        Shift? shift,
        Term? term,
        LateFeeType? type,
        bool? isActive,
        bool? isGlobal
    );

    Task<bool> HasPolicyWithCriteriaAsync(
        GradeLevel? gradeLevel,
        Section? section,
        Shift? shift,
        Term? term,
        Guid? exceptId = null);

    Task<List<LateFeePolicy>> GetGlobalActivePoliciesAsync(Guid? exceptId = null);

    Task<LateFeePolicy?> FindApplicablePolicyAsync(
        int? gradeLevel,
        int? section,
        int? shift,
        int? term);
}
