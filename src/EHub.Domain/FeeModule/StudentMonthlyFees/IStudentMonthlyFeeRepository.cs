using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EHub.FeeModule.StudentMonthlyFees;

public interface IStudentMonthlyFeeRepository : IRepository<StudentMonthlyFee, Guid>
{
    Task<StudentMonthlyFee?> FindByStudentAndMonthAsync(Guid studentId, DateTime month);
    Task<StudentMonthlyFee?> GetByIdAsync(Guid id);

    Task<long> GetCountAsync(string? filter, Guid? studentId, DateTime? month, DateTime? collectedOn);
    Task<List<StudentMonthlyFee>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filter,
        Guid? studentId,
        DateTime? month,
        DateTime? collectedOn);

    Task<bool> ExistsAsync(Guid studentId, DateTime month, Guid? exceptId = null);
}
