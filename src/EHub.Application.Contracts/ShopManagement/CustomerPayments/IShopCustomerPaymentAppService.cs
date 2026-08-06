using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.CustomerPayments;

public interface IShopCustomerPaymentAppService : IApplicationService
{
    Task<PagedResultDto<ShopCustomerPaymentDto>> GetListAsync(GetShopCustomerPaymentsInput input);
    Task<ShopCustomerPaymentDto> GetAsync(Guid id);
    Task<ShopCustomerPaymentDto> CreateAsync(CreateUpdateShopCustomerPaymentDto input);
    Task<ShopCustomerPaymentDto> UpdateAsync(Guid id, CreateUpdateShopCustomerPaymentDto input);
    Task DeleteAsync(Guid id);
    Task<ShopCustomerPaymentDto> PostAsync(Guid id);
    Task<ShopCustomerPaymentDto> CancelAsync(Guid id, CancelShopCustomerPaymentDto input);
    Task<ListResultDto<ShopCustomerOutstandingSaleDto>> GetOutstandingSalesAsync(Guid customerId);
}
