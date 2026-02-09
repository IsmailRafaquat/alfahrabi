using EHub.Students;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EHub.Staffs;

public interface IStaffRepository : IRepository<Staff, Guid>
{
    Task<Staff?> FindByEmployeeCodeAsync(string employeeCode);
    Task<Staff?> GetLastCreatedStaffAsync(Guid? tenantId);

    Task<List<Staff>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filter,
        string? firstName,
        string? lastName,
        Department? department,
        JobStatus? jobStatus,
        Shift? shift);

    Task<long> GetCountAsync(
        string? filter,
        string? firstName,
        string? lastName,
        Department? department,
        JobStatus? jobStatus,
        Shift? shift);
    Task<Staff?> GetByIdAsync(Guid id);
    Task<List<Staff>> GetStaffLookupAsync();
    Task<List<string>> GetDeleteBlockersAsync(Guid staffId);
}
