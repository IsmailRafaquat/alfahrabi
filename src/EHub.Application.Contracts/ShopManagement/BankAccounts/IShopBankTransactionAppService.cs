using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.BankAccounts;

public interface IShopBankTransactionAppService : IApplicationService
{
    Task<PagedResultDto<ShopBankTransactionDto>> GetListAsync(GetShopBankTransactionsInput input);

    Task<ShopBankTransactionDto> GetAsync(Guid id);

    Task<ShopBankAccountSummaryDto> GetSummaryAsync(Guid bankAccountId, DateTime? dateFrom = null, DateTime? dateTo = null);

    Task<ShopBankTransactionDto> CreateManualMovementAsync(CreateManualBankMovementDto input);
}
