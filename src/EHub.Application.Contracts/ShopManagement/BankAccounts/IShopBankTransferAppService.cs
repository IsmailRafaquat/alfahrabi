using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.BankAccounts;

public interface IShopBankTransferAppService : IApplicationService
{
    Task<PagedResultDto<ShopBankTransferDto>> GetListAsync(GetShopBankTransfersInput input);

    Task<ShopBankTransferDto> GetAsync(Guid id);

    Task<ShopBankTransferDto> CreateAsync(CreateUpdateShopBankTransferDto input);

    Task<ShopBankTransferDto> UpdateAsync(Guid id, CreateUpdateShopBankTransferDto input);

    Task DeleteAsync(Guid id);

    Task<ShopBankTransferDto> PostAsync(Guid id);

    Task<ShopBankTransferDto> CancelAsync(Guid id, CancelShopBankTransferDto input);
}
