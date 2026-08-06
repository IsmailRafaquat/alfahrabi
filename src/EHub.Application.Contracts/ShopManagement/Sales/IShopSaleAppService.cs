using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.Sales;

public interface IShopSaleAppService : IApplicationService
{
    Task<PagedResultDto<ShopSaleDto>> GetListAsync(GetShopSalesInput input);
    Task<ShopSaleDto> GetAsync(Guid id);
    Task<ShopSaleDto> CreateAsync(CreateShopSaleDto input);
    Task<ShopSaleDto> UpdateAsync(Guid id, UpdateShopSaleDto input);
    Task DeleteAsync(Guid id);
    Task<ShopSaleDto> CompleteAsync(Guid id, CompleteShopSaleDto input);
    Task<ShopSaleDto> CancelAsync(Guid id, CancelShopSaleDto input);
    Task<ListResultDto<ShopSaleProductLookupDto>> GetSaleProductLookupAsync(string? filter = null);
}
