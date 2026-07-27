using System;
using System.Threading.Tasks;
using EHub.ShopManagement.StockTransactions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.ProductBatches;

public interface IShopProductBatchAppService : IApplicationService
{
    Task<PagedResultDto<ShopProductBatchDto>> GetListAsync(GetShopProductBatchesInput input);

    Task<ShopProductBatchDto> GetAsync(Guid id);

    Task<ShopProductBatchDto> UpdateAsync(Guid id, UpdateShopProductBatchDto input);

    Task<ListResultDto<ShopProductBatchLookupDto>> GetAvailableBatchesAsync(Guid productId);

    Task<ShopBatchAvailabilityDto> CheckAvailabilityAsync(Guid productId, decimal requiredQuantity);

    Task<ShopBatchSummaryDto> GetSummaryAsync(Guid productId);

    Task<ListResultDto<ShopStockTransactionDto>> GetTransactionsAsync(Guid id);

    Task<ShopProductBatchDto> BlockAsync(Guid id, BlockShopProductBatchDto input);

    Task<ShopProductBatchDto> UnblockAsync(Guid id);

    Task RefreshStatusesAsync();
}
