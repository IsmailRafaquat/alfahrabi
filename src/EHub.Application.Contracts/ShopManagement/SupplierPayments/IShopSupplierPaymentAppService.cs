using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.SupplierPayments;

public interface IShopSupplierPaymentAppService : IApplicationService
{
    Task<PagedResultDto<ShopSupplierPaymentDto>> GetListAsync(GetShopSupplierPaymentsInput input);
    Task<ShopSupplierPaymentDto> GetAsync(Guid id);
    Task<List<ShopSupplierOutstandingReceiptDto>> GetOutstandingReceiptsAsync(Guid supplierId);
    Task<ShopSupplierPaymentDto> CreateAsync(CreateUpdateShopSupplierPaymentDto input);
    Task<ShopSupplierPaymentDto> UpdateAsync(Guid id, CreateUpdateShopSupplierPaymentDto input);
    Task DeleteAsync(Guid id);
    Task<ShopSupplierPaymentDto> PostAsync(Guid id);
    Task<ShopSupplierPaymentDto> CancelAsync(Guid id, CancelShopSupplierPaymentDto input);
}
