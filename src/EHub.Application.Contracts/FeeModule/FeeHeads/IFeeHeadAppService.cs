using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.FeeModule.FeeHeads;

public interface IFeeHeadAppService : IApplicationService
{
    Task<FeeHeadDto> GetAsync(Guid id);

    Task<PagedResultDto<FeeHeadDto>> GetListAsync(GetFeeHeadListInput input);

    Task<FeeHeadDto> CreateAsync(CreateUpdateFeeHeadDto input);

    Task<FeeHeadDto> UpdateAsync(Guid id, CreateUpdateFeeHeadDto input);

    Task DeleteAsync(Guid id);

    Task SetActiveAsync(Guid id, bool isActive);
    Task<List<FeeHeadLookupDto>> GetFeeLookupAsync();
}
