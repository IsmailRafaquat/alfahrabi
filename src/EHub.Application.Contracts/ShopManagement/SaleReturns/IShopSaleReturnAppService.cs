using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.SaleReturns;

public interface IShopSaleReturnAppService : IApplicationService
{
    Task<PagedResultDto<ShopSaleReturnDto>> GetListAsync(GetShopSaleReturnsInput input);

    Task<ShopSaleReturnDto> GetAsync(Guid id);

    Task<ShopSaleReturnableDto> GetSaleForReturnAsync(Guid saleId);

    Task<ShopSaleReturnDto> CreateAsync(CreateShopSaleReturnDto input);

    Task<ShopSaleReturnDto> UpdateAsync(Guid id, UpdateShopSaleReturnDto input);

    Task DeleteAsync(Guid id);

    Task<ShopSaleReturnDto> CompleteAsync(Guid id);

    Task<ShopSaleReturnDto> CancelAsync(Guid id, CancelShopSaleReturnDto input);
}
