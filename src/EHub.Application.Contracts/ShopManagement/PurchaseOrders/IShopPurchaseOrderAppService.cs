using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.PurchaseOrders;

public interface IShopPurchaseOrderAppService : IApplicationService
{
    Task<PagedResultDto<ShopPurchaseOrderDto>> GetListAsync(GetShopPurchaseOrdersInput input);
    Task<ShopPurchaseOrderDto> GetAsync(Guid id);
    Task<ShopPurchaseOrderDto> CreateAsync(CreateShopPurchaseOrderDto input);
    Task<ShopPurchaseOrderDto> UpdateAsync(Guid id, UpdateShopPurchaseOrderDto input);
    Task DeleteAsync(Guid id);
    Task<ShopPurchaseOrderDto> SubmitAsync(Guid id);
    Task<ShopPurchaseOrderDto> ApproveAsync(Guid id);
    Task<ShopPurchaseOrderDto> RejectAsync(Guid id, RejectShopPurchaseOrderDto input);
    Task<ShopPurchaseOrderDto> CancelAsync(Guid id, CancelShopPurchaseOrderDto input);
}
