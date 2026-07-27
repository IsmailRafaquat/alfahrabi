using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.BankAccounts;

public interface IShopBankAccountAppService : IApplicationService
{
    Task<PagedResultDto<ShopBankAccountDto>> GetListAsync(GetShopBankAccountsInput input);

    Task<ShopBankAccountDto> GetAsync(Guid id);

    Task<ShopBankAccountDto> CreateAsync(CreateUpdateShopBankAccountDto input);

    Task<ShopBankAccountDto> UpdateAsync(Guid id, CreateUpdateShopBankAccountDto input);

    Task DeleteAsync(Guid id);

    Task<ListResultDto<ShopBankAccountLookupDto>> GetLookupAsync();
}
