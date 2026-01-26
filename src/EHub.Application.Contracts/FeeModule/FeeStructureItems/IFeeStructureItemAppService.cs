using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.FeeModule.FeeStructureItems;

public interface IFeeStructureItemAppService : IApplicationService
{
    Task<FeeStructureItemDto> GetAsync(Guid id);

    Task<PagedResultDto<FeeStructureItemDto>> GetListAsync(GetFeeStructureItemListInput input);

    Task<FeeStructureItemDto> CreateAsync(CreateUpdateFeeStructureItemDto input);

    Task<FeeStructureItemDto> UpdateAsync(Guid id, CreateUpdateFeeStructureItemDto input);

    Task DeleteAsync(Guid id);
}
