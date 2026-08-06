using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.StockCounts;

public interface IShopStockCountAppService : IApplicationService
{
    Task<PagedResultDto<ShopStockCountDto>> GetListAsync(GetShopStockCountsInput input);

    Task<ShopStockCountDto> GetAsync(Guid id);

    Task<ShopStockCountDto> CreateAsync(CreateShopStockCountDto input);

    Task<ShopStockCountDto> UpdateAsync(Guid id, UpdateShopStockCountDto input);

    Task DeleteAsync(Guid id);

    Task<ShopStockCountDto> StartAsync(Guid id);

    Task<ShopStockCountDto> UpdateItemQuantityAsync(Guid id, UpdateShopStockCountItemQuantityDto input);

    Task<ShopStockCountDto> CompleteCountAsync(Guid id);

    Task<ShopStockCountPostingPreviewDto> GetPostingPreviewAsync(Guid id);

    Task<ShopStockCountDto> PostAsync(Guid id);

    Task<ShopStockCountDto> CancelAsync(Guid id, CancelShopStockCountDto input);

    Task<ListResultDto<ShopStockCountProductLookupDto>> GetProductLookupAsync(string? filter = null);
}
