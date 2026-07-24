using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.CashRegisters;

public interface IShopCashRegisterAppService : IApplicationService
{
    Task<PagedResultDto<ShopCashRegisterDto>> GetListAsync(GetShopCashRegistersInput input);

    Task<ShopCashRegisterDto> GetAsync(Guid id);

    Task<ShopCashRegisterDto> CreateAsync(CreateUpdateShopCashRegisterDto input);

    Task<ShopCashRegisterDto> UpdateAsync(Guid id, CreateUpdateShopCashRegisterDto input);

    Task DeleteAsync(Guid id);

    Task<ListResultDto<ShopCashRegisterLookupDto>> GetLookupAsync();

    Task<ShopCashClosingDto?> GetOpenClosingAsync(Guid cashRegisterId);

    Task<ShopCashClosingDto> GetClosingAsync(Guid closingId);

    Task<PagedResultDto<ShopCashClosingDto>> GetClosingsAsync(GetShopCashTransactionsInput input);

    Task<ShopCashClosingDto> OpenAsync(Guid cashRegisterId, OpenShopCashRegisterDto input);

    Task<ShopCashClosingDto> CloseAsync(Guid closingId, CloseShopCashRegisterDto input);

    Task<ShopCashClosingDto> CancelClosingAsync(Guid closingId, CancelShopCashClosingDto input);

    Task<ShopCashRegisterTransactionDto> CreateManualMovementAsync(CreateManualCashMovementDto input);

    Task<PagedResultDto<ShopCashRegisterTransactionDto>> GetTransactionsAsync(GetShopCashTransactionsInput input);

    Task<ShopCashRegisterSummaryDto> GetSummaryAsync(Guid closingId);
}
