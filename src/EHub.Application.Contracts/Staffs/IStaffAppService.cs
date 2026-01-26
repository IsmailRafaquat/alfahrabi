using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.Staffs;

public interface IStaffAppService : IApplicationService
{
    Task<StaffDto> GetAsync(Guid id);

    Task<PagedResultDto<StaffDto>> GetListAsync(GetStaffListDto input);

    Task<StaffDto> CreateAsync(CreateStaffDto input);

    Task UpdateAsync(Guid id, UpdateStaffDto input);

    Task DeleteAsync(Guid id);
    Task<List<StaffLookupDto>> GetStaffLookupAsync();
}
