using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.Suppliers;

public interface IShopSupplierAppService : IApplicationService
{
    Task<PagedResultDto<ShopSupplierDto>> GetListAsync(GetShopSuppliersInput input);
    Task<ShopSupplierDto> GetAsync(Guid id);
    Task<ShopSupplierDto> CreateAsync(CreateUpdateShopSupplierDto input);
    Task<ShopSupplierDto> UpdateAsync(Guid id, CreateUpdateShopSupplierDto input);
    Task DeleteAsync(Guid id);
    Task<ListResultDto<ShopSupplierLookupDto>> GetLookupAsync(string? filter = null);
}
