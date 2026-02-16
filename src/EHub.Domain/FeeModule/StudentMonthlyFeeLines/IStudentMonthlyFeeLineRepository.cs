using EHub.Students;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

public interface IStudentMonthlyFeeLineRepository : IRepository<StudentMonthlyFeeLine, Guid>
{
    Task<StudentMonthlyFeeLine?> GetByIdAsync(Guid id);

    Task<List<StudentMonthlyFeeLine>> GetByMonthlyFeeAsync(Guid studentMonthlyFeeId);

    Task<StudentMonthlyFeeLine?> FindByMonthlyFeeAndHeadAsync(Guid studentMonthlyFeeId, Guid feeHeadId);

    Task<long> GetCountAsync(string? filters, Guid? studentMonthlyFeeId, Guid? feeHeadId,
        GradeLevel? gradeLevel,
        Section? section,
        bool? onlyPositiveBalance);

    Task<List<StudentMonthlyFeeLine>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filters,
        Guid? studentMonthlyFeeId,
        Guid? feeHeadId,
        GradeLevel? gradeLevel,
        Section? section,
        bool? onlyPositiveBalance
    );
}
