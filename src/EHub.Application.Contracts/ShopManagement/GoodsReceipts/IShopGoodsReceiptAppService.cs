using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.GoodsReceipts;

public interface IShopGoodsReceiptAppService : IApplicationService
{
    Task<PagedResultDto<ShopGoodsReceiptDto>> GetListAsync(GetShopGoodsReceiptsInput input);
    Task<ShopGoodsReceiptDto> GetAsync(Guid id);
    Task<ShopPurchaseOrderReceivingDto> GetPurchaseOrderForReceivingAsync(Guid purchaseOrderId);
    Task<ShopGoodsReceiptDto> CreateAsync(CreateShopGoodsReceiptDto input);
    Task<ShopGoodsReceiptDto> UpdateAsync(Guid id, UpdateShopGoodsReceiptDto input);
    Task DeleteAsync(Guid id);
    Task<ShopGoodsReceiptDto> CompleteAsync(Guid id);
    Task<ShopGoodsReceiptDto> CancelAsync(Guid id, CancelShopGoodsReceiptDto input);
}
