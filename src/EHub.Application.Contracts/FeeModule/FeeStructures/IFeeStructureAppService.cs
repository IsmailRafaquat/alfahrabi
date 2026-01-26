using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.FeeModule.FeeStructures;

public interface IFeeStructureAppService : IApplicationService
{
    Task<FeeStructureDto> GetAsync(Guid id);

    Task<PagedResultDto<FeeStructureDto>> GetListAsync(GetFeeStructureListInput input);

    Task<FeeStructureDto> CreateAsync(CreateUpdateFeeStructureDto input);

    Task<FeeStructureDto> UpdateAsync(Guid id, CreateUpdateFeeStructureDto input);

    Task DeleteAsync(Guid id);

    Task SetActiveAsync(Guid id, bool isActive);
    Task<List<FeeStructureLookupDto>> GetFeeStructureLookupAsync();
}
