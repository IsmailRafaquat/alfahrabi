using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.ExpenseCategories;

public interface IShopExpenseCategoryAppService : IApplicationService
{
    Task<PagedResultDto<ShopExpenseCategoryDto>> GetListAsync(GetShopExpenseCategoriesInput input);

    Task<ShopExpenseCategoryDto> GetAsync(Guid id);

    Task<ShopExpenseCategoryDto> CreateAsync(CreateUpdateShopExpenseCategoryDto input);

    Task<ShopExpenseCategoryDto> UpdateAsync(Guid id, CreateUpdateShopExpenseCategoryDto input);

    Task DeleteAsync(Guid id);

    Task<ListResultDto<ShopExpenseCategoryLookupDto>> GetLookupAsync(string? filter = null);
}
