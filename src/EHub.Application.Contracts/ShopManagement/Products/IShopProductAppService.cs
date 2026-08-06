using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.Products;

public interface IShopProductAppService : IApplicationService
{
    Task<PagedResultDto<ShopProductDto>> GetListAsync(GetShopProductsInput input);
    Task<ShopProductDto> GetAsync(Guid id);
    Task<ShopProductDto> CreateAsync(CreateShopProductDto input);
    Task<ShopProductDto> UpdateAsync(Guid id, UpdateShopProductDto input);
    Task DeleteAsync(Guid id);
    Task<ListResultDto<ShopProductLookupDto>> GetLookupAsync(string? filter = null, Guid? categoryId = null);
}
