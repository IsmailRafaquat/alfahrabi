using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.StockAdjustments;

public interface IShopStockAdjustmentAppService : IApplicationService
{
    Task<PagedResultDto<ShopStockAdjustmentDto>> GetListAsync(GetShopStockAdjustmentsInput input);

    Task<ShopStockAdjustmentDto> GetAsync(Guid id);

    Task<ShopStockAdjustmentDto> CreateAsync(CreateShopStockAdjustmentDto input);

    Task<ShopStockAdjustmentDto> UpdateAsync(Guid id, UpdateShopStockAdjustmentDto input);

    Task DeleteAsync(Guid id);

    Task<ShopStockAdjustmentDto> PostAsync(Guid id);

    Task<ShopStockAdjustmentDto> CancelAsync(Guid id, CancelShopStockAdjustmentDto input);

    Task<ListResultDto<ShopStockAdjustmentProductLookupDto>> GetProductLookupAsync(string? filter = null);
}
