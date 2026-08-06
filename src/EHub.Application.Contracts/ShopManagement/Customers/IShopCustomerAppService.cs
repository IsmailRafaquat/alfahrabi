using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.Customers;

public interface IShopCustomerAppService : IApplicationService
{
    Task<PagedResultDto<ShopCustomerDto>> GetListAsync(GetShopCustomersInput input);
    Task<ShopCustomerDto> GetAsync(Guid id);
    Task<ShopCustomerDto> CreateAsync(CreateUpdateShopCustomerDto input);
    Task<ShopCustomerDto> UpdateAsync(Guid id, CreateUpdateShopCustomerDto input);
    Task DeleteAsync(Guid id);
    Task<ListResultDto<ShopCustomerLookupDto>> GetLookupAsync(string? filter = null);
    Task<ShopCustomerLookupDto?> GetWalkInCustomerAsync();
}
