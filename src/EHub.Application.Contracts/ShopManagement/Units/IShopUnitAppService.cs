using System; using System.Threading.Tasks; using Volo.Abp.Application.Dtos; using Volo.Abp.Application.Services;
namespace EHub.ShopManagement.Units;
public interface IShopUnitAppService : IApplicationService { Task<PagedResultDto<ShopUnitDto>> GetListAsync(GetShopUnitsInput input); Task<ShopUnitDto> GetAsync(Guid id); Task<ShopUnitDto> CreateAsync(CreateUpdateShopUnitDto input); Task<ShopUnitDto> UpdateAsync(Guid id, CreateUpdateShopUnitDto input); Task DeleteAsync(Guid id); Task<ListResultDto<ShopUnitLookupDto>> GetLookupAsync(); }
