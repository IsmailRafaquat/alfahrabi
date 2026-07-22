using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.StockTransactions;

public interface IShopStockTransactionAppService : IApplicationService
{
    Task<PagedResultDto<ShopStockTransactionDto>> GetListAsync(GetShopStockTransactionsInput input);
    Task<ShopStockTransactionDto> GetAsync(Guid id);
}
