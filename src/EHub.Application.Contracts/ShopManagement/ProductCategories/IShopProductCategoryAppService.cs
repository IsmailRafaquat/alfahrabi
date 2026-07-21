using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
namespace EHub.ShopManagement.ProductCategories;
public interface IShopProductCategoryAppService : IApplicationService
{
    Task<PagedResultDto<ShopProductCategoryDto>> GetListAsync(GetShopProductCategoriesInput input);
    Task<ShopProductCategoryDto> GetAsync(Guid id);
    Task<ShopProductCategoryDto> CreateAsync(CreateUpdateShopProductCategoryDto input);
    Task<ShopProductCategoryDto> UpdateAsync(Guid id, CreateUpdateShopProductCategoryDto input);
    Task DeleteAsync(Guid id);
    Task<ListResultDto<ShopProductCategoryLookupDto>> GetLookupAsync();
    Task<List<ShopProductCategoryTreeDto>> GetTreeAsync();
}
